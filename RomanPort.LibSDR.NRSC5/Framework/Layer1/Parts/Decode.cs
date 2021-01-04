using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    unsafe class Decode : Nrsc5Layer1Part
    {
        sbyte[] buffer_pm;
        int idx_pm;
        sbyte[] buffer_px1;
        int idx_px1;

        sbyte[] viterbi_p1;
        byte[] scrambler_p1;
        sbyte[] viterbi_pids;
        byte[] scrambler_pids;
        sbyte[] internal_p3;
        int i_p3;
        int ready_p3;
        int[] pt_p3;
        sbyte[] viterbi_p3;
        byte[] scrambler_p3;

        ConvDec convP1;
        ConvDec convP3;
        ConvDec convPids;

        public Decode()
        {
            buffer_pm = new sbyte[720 * BLKSZ * 16];
            buffer_px1 = new sbyte[144 * BLKSZ * 2];
            viterbi_p1 = new sbyte[P1_FRAME_LEN * 3];
            scrambler_p1 = new byte[P1_FRAME_LEN];
            viterbi_pids = new sbyte[PIDS_FRAME_LEN * 3];
            scrambler_pids = new byte[PIDS_FRAME_LEN];
            internal_p3 = new sbyte[P3_FRAME_LEN * 32];
            pt_p3 = new int[4];
            viterbi_p3 = new sbyte[P3_FRAME_LEN * 3];
            scrambler_p3 = new byte[P3_FRAME_LEN];
            convP1 = new ConvDec(P1_FRAME_LEN);
            convP3 = new ConvDec(P3_FRAME_LEN);
            convPids = new ConvDec(PIDS_FRAME_LEN);
        }

        public void decode_push_pm(sbyte sbit)
        {
            buffer_pm[idx_pm++] = sbit;
            if (idx_pm % (720 * BLKSZ) == 0)
            {
                /*char[] output = new char[(720 * BLKSZ)];
                for (int i = 0; i < (720 * BLKSZ); i++)
                {
                    output[i] = (char)(((buffer_pm[i] >> 7) & 1) + 48);
                }
                Console.WriteLine(output);
                Console.ReadLine();*/
                //^appears to be known good. identical results
                decode_process_pids();
            }
            if (idx_pm == 720 * BLKSZ * 16)
            {
                decode_process_p1();
                idx_pm = 0;
            }
        }

        public void decode_push_px1(sbyte sbit)
        {
            buffer_px1[idx_px1++] = sbit;
            if (idx_px1 % (144 * BLKSZ * 2) == 0)
            {
                decode_process_p3();
                idx_px1 = 0;
            }
        }

        static float calc_cber(sbyte[] coded, byte[] decoded)
        {
            byte r = 0;
            int i, j, errors = 0;

            // tail biting
            for (i = 0; i < 6; i++)
                r = (byte)((r >> 1) | (decoded[P1_FRAME_LEN - 6 + i] << 6));

            for (i = 0, j = 0; i < P1_FRAME_LEN; i++)
            {
                // shift in new bit
                r = (byte)((r >> 1) | (decoded[i] << 6));

                if ((coded[j++] > 0) != __builtin_parity_bool(r & 0133))
                    errors++;

                if ((coded[j++] > 0) != __builtin_parity_bool(r & 0171))
                    errors++;

                if ((j % 6) == 5)
                    j++;
                else if ((coded[j++] > 0) != __builtin_parity_bool(r & 0165))
                    errors++;
            }

            return (float)errors / P1_FRAME_LEN_ENCODED;
        }

        static void descramble(byte[] buf, int length)
        {
            int width = 11;
            uint i, val = 0x3ff;
            for (i = 0; i < length; i += 8)
            {
                uint j;
                for (j = 0; j < 8; ++j)
                {
                    int bit = (int)(((val >> 9) ^ val) & 1);
                    val |= (uint)(bit << width);
                    val >>= 1;
                    buf[i + j] ^= (byte)bit;
                }
            }
        }

        void decode_process_p1()
        {
            int J = 20, B = 16, C = 36;
            sbyte[] v = new sbyte[] {
                10, 2, 18, 6, 14, 8, 16, 0, 12, 4,
                11, 3, 19, 7, 15, 9, 17, 1, 13, 5
            };
            int i, o = 0;
            for (i = 0; i < P1_FRAME_LEN_ENCODED; i++)
            {
                int partition = v[i % J];
                int block = ((i / J) + (partition * 7)) % B;
                int k = i / (J * B);
                int row = (k * 11) % 32;
                int column = (k * 11 + k / (32 * 9)) % C;
                this.viterbi_p1[o++] = this.buffer_pm[(block * 32 + row) * 720 + partition * C + column];
                if ((o % 6) == 5) // depuncture, [1, 1, 1, 1, 1, 0]
                    this.viterbi_p1[o++] = 0;
            }

            convP1.nrsc5_conv_decode(this.viterbi_p1, this.scrambler_p1);
            //nrsc5_report_ber(this.input->radio, calc_cber(this.viterbi_p1, this.scrambler_p1));
            descramble(this.scrambler_p1, P1_FRAME_LEN);
            frame.frame_push(this.scrambler_p1, P1_FRAME_LEN);
            //frame_push(&this.input->frame, this.scrambler_p1, P1_FRAME_LEN);
        }

        void decode_process_pids()
        {
            int J = 20, B = 16, C = 36;
            sbyte[] v = new sbyte[] {
                10, 2, 18, 6, 14, 8, 16, 0, 12, 4,
                11, 3, 19, 7, 15, 9, 17, 1, 13, 5
            };
            int i, o = 0;
            for (i = 0; i < PIDS_FRAME_LEN_ENCODED; i++)
            {
                int partition = v[i % J];
                int block = decode_get_block() - 1;
                int k = ((i / J) % (PIDS_FRAME_LEN_ENCODED / J)) + (P1_FRAME_LEN_ENCODED / (J * B));
                int row = (k * 11) % 32;
                int column = (k * 11 + k / (32 * 9)) % C;
                this.viterbi_pids[o++] = this.buffer_pm[(block * 32 + row) * 720 + partition * C + column];
                if ((o % 6) == 5) // depuncture, [1, 1, 1, 1, 1, 0]
                    this.viterbi_pids[o++] = 0;
            }

            convPids.nrsc5_conv_decode(viterbi_pids, scrambler_pids);
            descramble(this.scrambler_pids, PIDS_FRAME_LEN);


            pids.pids_frame_push(this.scrambler_pids);
        }

        void decode_process_p3()
        {
            int J = 4, B = 32, C = 36, M = 2, N = 147456;
            int bk_bits = 32 * C;
            int bk_adj = 32 * C - 1;
            int i, o = 0;
            for (i = 0; i < P3_FRAME_LEN_ENCODED; i++)
            {
                int partition = ((this.i_p3 + 2 * (M / 4)) / M) % J;
                int pti = (this.pt_p3[partition])++;
                int block = (pti + (partition * 7) - (bk_adj * (pti / bk_bits))) % B;
                int row = ((11 * pti) % bk_bits) / C;
                int column = (pti * 11) % C;
                this.viterbi_p3[o++] = this.internal_p3[(block * 32 + row) * 144 + partition * C + column];
                if ((o % 6) == 1 || (o % 6) == 4) // depuncture, [1, 0, 1, 1, 0, 1]
                    this.viterbi_p3[o++] = 0;

                this.internal_p3[this.i_p3] = this.buffer_px1[i];
                (this.i_p3)++;
            }
            if (this.ready_p3 != 0)
            {
                convP3.nrsc5_conv_decode(this.viterbi_p3, this.scrambler_p3);
                descramble(this.scrambler_p3, P3_FRAME_LEN);
                Console.WriteLine("P3 Frame");
                //frame.frame_push(this.scrambler_p3, P3_FRAME_LEN);
                //frame_push(&this.input->frame, this.scrambler_p3, P3_FRAME_LEN);
            }
            if (this.i_p3 == N)
            {
                this.i_p3 = 0;
                this.ready_p3 = 1;
            }
        }

        public void decode_reset()
        {
            this.idx_pm = 0;
            this.idx_px1 = 0;
            this.i_p3 = 0;
            this.ready_p3 = 0;
            for (int i = 0; i < pt_p3.Length; i++)
                pt_p3[i] = 0;
            //pids_init(&this.pids, this.input);
        }

        void decode_init()
        {
            decode_reset();
        }

        int decode_get_block()
        {
            return idx_pm / (720 * BLKSZ);
        }
    }
}
