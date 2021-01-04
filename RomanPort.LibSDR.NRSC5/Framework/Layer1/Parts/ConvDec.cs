using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    unsafe class ConvDec : CTranslationLayer
    {
		public const int TAIL_BITING_EXTRA = 32;
		public const int SSE_ALIGN = 16;

		enum CONV_TERM
		{
			FLUSH,
			TAIL_BITING,
		};

		/*
		 * Convolutional code descriptor
		 *
		 * n    - Rate 2, 3, 4 (1/2, 1/3, 1/4)
		 * k    - Constraint length (5 or 7)
		 * rgen - Recursive generator polynomial in octal
		 * gen  - Generator polynomials in octal
		 * punc - Puncturing matrix (-1 terminated)
		 * term - Termination type (zero flush default)
		 */
		struct lte_conv_code
		{
			public int n;
			public int k;
			public int len;
			public int rgen;
			public int[] gen; // len [4];
			public int* punc;
			public CONV_TERM term;
		};

		/*
		 * Trellis State
		 *
		 * state - Internal shift register value
		 * prev  - Register values of previous 0 and 1 states
		 */
		/*struct vstate
		{
			public int state;
			public int[] prev; //len: 2
		};*/

		/*
		 * Trellis Object
		 *
		 * num_states - Number of states in the trellis
		 * sums       - Accumulated path metrics
		 * outputs    - Trellis ouput values
		 * vals       - Input value that led to each state
		 */
		class vtrellis
		{
			public int num_states;
			public UnsafeBuffer sumsBuffer;
			public short* sums;
			public UnsafeBuffer outputsBuffer;
			public short* outputs;
			public UnsafeBuffer valsBuffer;
			public sbyte* vals;
		};

		/*
		 * Viterbi Decoder
		 *
		 * n         - Code order
		 * k         - Constraint length
		 * len       - Horizontal length of trellis
		 * recursive - Set to '1' if the code is recursive
		 * intrvl    - Normalization interval
		 * trellis   - Trellis object
		 * punc      - Puncturing sequence
		 * paths     - Trellis paths
		 */
		class vdecoder
		{
			public int n;
			public int k;
			public int len;
			public int recursive;
			public int intrvl;
			public vtrellis trellis;
			public int* punc;
			public UnsafeBuffer pathsBuffer;
			public short* pathsPtr;
			public short*[] paths;
		};

		/* Left shift and mask for finding the previous state */
		static int vstate_lshift(int reg, int k, int val)
		{
			int mask;

			if (k == 5)
				mask = 0x0e;
			else if (k == 7)
				mask = 0x3e;
			else
				mask = 0;

			return (int)(((reg << 1) & mask) | val);
		}

		/*
		 * Populate non-recursive trellis state
		 *
		 * For a given state defined by the k-1 length shift register, find the
		 * value of the input bit that drove the trellis to that state. Then
		 * generate the N outputs of the generator polynomial at that state.
		 */
		static void gen_state_info(lte_conv_code code,
			   sbyte* val, int reg, short* o)
		{
			int i;
			int prev;

			/* Previous '0' state */
			prev = vstate_lshift(reg, code.k, 0);

			/* Compute output and unpack to NRZ */
			*val = (sbyte)((reg >> (code.k - 2)) & 0x01);
			prev = prev | (int)*val << (code.k - 1);

			for (i = 0; i < code.n; i++)
				o[i] = (short)(__builtin_parity(prev & code.gen[i]) * 2 - 1);

			/*Console.WriteLine($"{__builtin_parity(0)}, {__builtin_parity(1)}, {__builtin_parity(2)}");
			for (i = 0; i < code.n; i++)
				Console.WriteLine(o[i]);
			Console.ReadLine();*/
		}

		/*
		 * Populate recursive trellis state
		 */
		static void gen_rec_state_info(lte_conv_code code, sbyte* val, int reg, short* o)
		{
			int i;
			int prev, rec, mask;

			/* Previous '0' and '1' states */
			prev = vstate_lshift(reg, code.k, 0);

			/* Compute recursive input value (not the value shifted into register) */
			rec = (reg >> (code.k - 2)) & 0x01;

			if ((int)__builtin_parity(prev & code.rgen) == rec)
				*val = 0;
			else
				*val = 1;

			/* Compute outputs and unpack to NRZ */
			prev = prev | rec << (code.k - 1);

			if (code.k == 5)
				mask = 0x0f;
			else
				mask = 0x3f;

			/* Check for recursive outputs */
			for (i = 0; i < code.n; i++)
			{
				if ((code.gen[i] & mask) != 0)
					o[i] = (short)(__builtin_parity(prev & code.gen[i]) * 2 - 1);
				else
					o[i] = (short)(*val * 2 - 1);
			}
		}

		/* Release the trellis */
		static void free_trellis(vtrellis trellis)
		{
			trellis.outputsBuffer.Dispose();
			trellis.sumsBuffer.Dispose();
			trellis.valsBuffer.Dispose();
		}

		static int NUM_STATES(int K)
		{

			return (K == 7 ? 64 : 16);

		}

		/*
		 * Allocate and initialize the trellis object
		 *
		 * Initialization consists of generating the outputs and output value of a
		 * given state. Due to trellis symmetry, only one of the transition paths
		 * is used by the butterfly operation in the forward recursion, so only one
		 * set of N outputs is required per state variable.
		 */
		static vtrellis generate_trellis(lte_conv_code code)
		{
			vtrellis trellis;
			short* o;

			int ns = NUM_STATES(code.k);
			int olen = (code.n == 2) ? 2 : 4;

			trellis = new vtrellis();
			trellis.num_states = ns;
			trellis.sumsBuffer = UnsafeBuffer.Create(ns, sizeof(short));
			trellis.sums = (short*)trellis.sumsBuffer;
			trellis.outputsBuffer = UnsafeBuffer.Create(ns * olen, sizeof(short));
			trellis.outputs = (short*)trellis.outputsBuffer;
			trellis.valsBuffer = UnsafeBuffer.Create(ns, sizeof(sbyte));
			trellis.vals = (sbyte*)trellis.valsBuffer;

			/* Populate the trellis state objects */
			for (int i = 0; i < ns; i++)
			{
				o = &trellis.outputs[olen * i];

				if (code.rgen != 0)
					gen_rec_state_info(code, &trellis.vals[i], i, o);
				else
					gen_state_info(code, &trellis.vals[i], i, o);
			}

			return trellis;
		}

		/*
		 * Reset decoder
		 *
		 * Set accumulated path metrics to zero. For termination other than
		 * tail-biting, initialize the zero state as the encoder starting state.
		 * Intialize with the maximum accumulated sum at length equal to the
		 * constraint length.
		 */
		static void reset_decoder(vdecoder dec, CONV_TERM term)
		{
			int ns = dec.trellis.num_states;

			memset(dec.trellis.sums, 0, sizeof(short) * ns);

			if (term != CONV_TERM.TAIL_BITING)
				dec.trellis.sums[0] = (short)(sbyte.MaxValue * dec.n * dec.k);
		}

		static int _traceback(vdecoder dec, int state, sbyte* o, int len, int offset)
		{
			int i;
			int path;

			for (i = len - 1; i >= 0; i--)
			{
				path = (int)(dec.paths[i + offset][state] + 1);
				o[i] = dec.trellis.vals[state];
				state = vstate_lshift(state, dec.k, (int)path);
			}

			return state;
		}

		static void _traceback_rec(vdecoder dec,
					   int state, sbyte* o, int len)
		{
			int i;
			int path;

			for (i = len - 1; i >= 0; i--)
			{
				path = (int)(dec.paths[i][state] + 1);
				o[i] = (sbyte)(path ^ dec.trellis.vals[state]);
				state = vstate_lshift(state, dec.k, (int)path);
			}
		}

		/*
		 * Traceback and generate decoded output
		 *
		 * For tail biting, find the largest accumulated path metric at the final state
		 * followed by two trace back passes. For zero flushing the final state is
		 * always zero with a single traceback path.
		 */
		static int traceback(vdecoder dec, sbyte* o, CONV_TERM term, int len)
		{
			int i, sum, max_p = -1, max = -1;
			int path, state = 0;

			if (term == CONV_TERM.TAIL_BITING)
			{
				for (i = 0; i < dec.trellis.num_states; i++)
				{
					sum = dec.trellis.sums[i];
					if (sum > max)
					{
						max_p = max;
						max = sum;
						state = (int)i;
					}
				}
				if (max < 0)
					throw new Exception();
				for (i = dec.len - 1; i >= len + TAIL_BITING_EXTRA; i--)
				{
					path = (int)(dec.paths[i][state] + 1);
					int prestate = state;
					state = vstate_lshift(state, dec.k, (int)path);
					//Console.WriteLine($"[Q] path={path}, state={state}, prestate={state}, k={dec.k}, i={i}");
				}
			}
			else
			{
				for (i = dec.len - 1; i >= len; i--)
				{
					path = (int)(dec.paths[i][state] + 1);
					state = vstate_lshift(state, dec.k, (int)path);
				}
			}

			if (dec.recursive != 0)
				_traceback_rec(dec, state, o, len);
			else
				state = _traceback(dec, state, o, len, term == CONV_TERM.TAIL_BITING ? TAIL_BITING_EXTRA : 0);

			/* Don't handle the odd case of recursize tail-biting codes */

			return max - max_p;
		}

		/* Release decoder object */
		static void free_vdec(vdecoder dec)
		{
			dec.pathsBuffer.Dispose();
		}

		/*
		 * Allocate decoder object
		 *
		 * Subtract the constraint length K on the normalization interval to
		 * accommodate the initialization path metric at state zero.
		 */
		static vdecoder alloc_vdec(lte_conv_code code)
		{
			int i, ns;

			ns = NUM_STATES(code.k);

			vdecoder dec = new vdecoder();
			dec.n = code.n;
			dec.k = code.k;
			dec.recursive = code.rgen != 0 ? 1 : 0;
			dec.intrvl = short.MaxValue / (dec.n * sbyte.MaxValue) - dec.k;

			if (dec.n != 3 || dec.k != 7)
				throw new Exception();

			if (code.term == CONV_TERM.FLUSH)
				dec.len = code.len + code.k - 1;
			else
				dec.len = code.len + TAIL_BITING_EXTRA * 2;

			dec.trellis = generate_trellis(code);

			dec.pathsBuffer = UnsafeBuffer.Create(sizeof(short*) * dec.len * ns);
			dec.pathsPtr = (short*)dec.pathsBuffer;
			dec.paths = new short*[dec.len];
			for (i = 0; i < dec.len; i++)
				dec.paths[i] = &dec.pathsPtr[i * ns];

			return dec;
		}

		static void reset_vdec(vdecoder dec, lte_conv_code code)
        {
			dec.n = code.n;
			dec.k = code.k;
			dec.recursive = code.rgen != 0 ? 1 : 0;
			dec.intrvl = short.MaxValue / (dec.n * sbyte.MaxValue) - dec.k;

			if (dec.n != 3 || dec.k != 7)
				throw new Exception();

			if (code.term == CONV_TERM.FLUSH)
				dec.len = code.len + code.k - 1;
			else
				dec.len = code.len + TAIL_BITING_EXTRA * 2;
		}

		/*
		 * Forward trellis recursion
		 *
		 * Generate branch metrics and path metrics with a combined function. Only
		 * accumulated path metric sums and path selections are stored. Normalize on
		 * the interval specified by the decoder.
		 */
		static void _conv_decode(vdecoder dec, sbyte* seq, CONV_TERM term, int len)
		{
			int i, j = 0;
			vtrellis trellis = dec.trellis;

			if (term == CONV_TERM.TAIL_BITING)
				j = len - TAIL_BITING_EXTRA;

			for (i = 0; i < dec.len; i++, j++)
			{
				if (term == CONV_TERM.TAIL_BITING && j == len)
					j = 0;

				gen_metrics_k7_n3(&seq[dec.n * j],
						 trellis.outputs,
						 trellis.sums,
						 dec.paths[i],
						 ((i % dec.intrvl) != 0 ? 0 : 1)); //flipped on purpose
			}
		}

		public ConvDec(int len)
        {
			//Make settings
			code = new lte_conv_code
			{
				n = 3,

				k = 7,

				len = len,

				gen = new int[] { 91, 121, 117, 0 },

				term = CONV_TERM.TAIL_BITING,
			};

			//Create
			vdec = alloc_vdec(code);
		}

		private lte_conv_code code;
		private vdecoder vdec;

		public int nrsc5_conv_decode(sbyte* i, sbyte* o)
		{
			//Configure
			reset_vdec(vdec, code);
			reset_decoder(vdec, CONV_TERM.TAIL_BITING);

			/* Propagate through the trellis with interval normalization */
			_conv_decode(vdec, i, CONV_TERM.TAIL_BITING, code.len);
			int rc = traceback(vdec, o, CONV_TERM.TAIL_BITING, code.len);

			//free_vdec(vdec);
			return rc;
		}

		public int nrsc5_conv_decode(sbyte[] i, byte[] o)
        {
			int result;
			fixed (sbyte* ip = i)
			fixed (byte* op = o)
				result = nrsc5_conv_decode(ip, (sbyte*)op);
			return result;
		}


		/*
		 * Add-Compare-Select (ACS-Butterfly)
		 *
		 * Compute 4 accumulated path metrics and 4 path selections. Note that path
		 * selections are store as -1 and 0 rather than 0 and 1. This is to match
		 * the output format of the sse packed compare instruction 'pmaxuw'.
		 */
		static void acs_butterfly(int state, int num_states,
					  short metric, short* sum,
					  short* new_sum, short* path)
		{
			int state0, state1;
			int sum0, sum1, sum2, sum3;

			state0 = *(sum + (2 * state + 0));
			state1 = *(sum + (2 * state + 1));

			sum0 = state0 + metric;
			sum1 = state1 - metric;
			sum2 = state0 - metric;
			sum3 = state1 + metric;

			if (sum0 > sum1)
			{
				*new_sum = (short)sum0;
				*path = -1;
			}
			else
			{
				*new_sum = (short)sum1;
				*path = 0;
			}

			if (sum2 > sum3)
			{
				*(new_sum + num_states / 2) = (short)sum2;
				*(path + num_states / 2) = -1;
			}
			else
			{
				*(new_sum + num_states / 2) = (short)sum3;
				*(path + num_states / 2) = 0;
			}
		}

		/* Branch metrics unit N=3 */
		static void _gen_branch_metrics_n3(int num_states, sbyte* seq,

				short* o, short* metrics)
		{
			int i;

			for (i = 0; i < num_states / 2; i++)
				metrics[i] = (short)(seq[0] * o[4 * i + 0] +
						 seq[1] * o[4 * i + 1] +
						 seq[2] * o[4 * i + 2]);
		}

		/* Path metric unit */
		static void _gen_path_metrics(int num_states, short* sums,
					   short* metrics, short* paths, int norm)
		{
			int i;
			short min;
			short[] new_sums_arr = new short[num_states];
			fixed (short* new_sums = new_sums_arr)
			{
				for (i = 0; i < num_states / 2; i++)
				{
					acs_butterfly(i, num_states, metrics[i],
							  sums, &new_sums[i], &paths[i]);
				}

				if (norm != 0)
				{
					min = new_sums[0];
					for (i = 1; i < num_states; i++)
					{
						if (new_sums[i] < min)
							min = new_sums[i];
					}

					for (i = 0; i < num_states; i++)
						new_sums[i] -= min;
				}

				Utils.Memcpy(sums, new_sums, num_states * sizeof(short));
			}


		}

		static void gen_metrics_k7_n3(sbyte* seq, short* o, short* sums, short* paths, int norm)
		{
			short[] metrics = new short[32];

			fixed (short* metricsPtr = metrics)
			{
				_gen_branch_metrics_n3(64, seq, o, metricsPtr);
				_gen_path_metrics(64, sums, metricsPtr, paths, norm);
			}

		}
	}
}
