/*
Copyright (C) 2014, Youssef Touil <youssef@airspy.com>

>> PORTED TO CSHARP BY ROMANPORT, only applies to this file <<

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Hardware.AirSpy.IqConverter
{
	unsafe class AirSpyIqConverter : IDisposable
	{
		float avg;
		float hbc;
		int len;
		int fir_index;
		int delay_index;
		float* fir_kernel;
		float* fir_queue;
		float* delay_line;

		UnsafeBuffer fir_kernel_buffer;
		UnsafeBuffer fir_queue_buffer;
		UnsafeBuffer delay_line_buffer;

		const float SCALE = 0.01f;
		const int SIZE_FACTOR = 32;
		const float HPF_COEFF = 0.01f;

		public AirSpyIqConverter(float* hb_kernel, int len)
		{
			int i, j;
			this.len = len / 2 + 1;
			this.hbc = hb_kernel[len / 2];
			fir_kernel_buffer = UnsafeBuffer.Create(this.len, out fir_kernel);
			fir_queue_buffer = UnsafeBuffer.Create(this.len * SIZE_FACTOR, out fir_queue);
			delay_line_buffer = UnsafeBuffer.Create(this.len / 2, out delay_line);
			iqconverter_float_reset();
			for (i = 0, j = 0; i < this.len; i++, j += 2)
			{
				this.fir_kernel[i] = hb_kernel[j];
			}
		}

		public void Dispose()
		{
			fir_kernel_buffer.Dispose();
			fir_queue_buffer.Dispose();
			delay_line_buffer.Dispose();
		}

		void iqconverter_float_reset()
		{
			this.avg = 0.0f;
			this.fir_index = 0;
			this.delay_index = 0;
			for (int i = 0; i < this.len / 2; i++)
				delay_line[i] = 0;
			for (int i = 0; i < this.len * SIZE_FACTOR; i++)
				fir_queue[i] = 0;
		}

		float process_fir_taps(float* kernel, float* queue, int len)
		{
			int i;
			float sum = 0.0f;

			if (len >= 8)
			{
				int it = len >> 3;

				for (i = 0; i < it; i++)
				{
					sum += kernel[0] * queue[0]
						+ kernel[1] * queue[1]
						+ kernel[2] * queue[2]
						+ kernel[3] * queue[3]
						+ kernel[4] * queue[4]
						+ kernel[5] * queue[5]
						+ kernel[6] * queue[6]
						+ kernel[7] * queue[7];

					queue += 8;
					kernel += 8;
				}

				len &= 7;
			}

			if (len >= 4)
			{
				sum += kernel[0] * queue[0]
					+ kernel[1] * queue[1]
					+ kernel[2] * queue[2]
					+ kernel[3] * queue[3];
				kernel += 4;
				queue += 4;
				len &= 3;
			}

			if (len >= 2)
			{
				sum += kernel[0] * queue[0]
					+ kernel[1] * queue[1];

				//kernel += 2;
				//queue += 2;
				//len &= 1;
			}

			//if (len >= 1)
			//{
			//	sum += kernel[0] * queue[0];
			//}

			return sum;
		}

		void fir_interleaved_4(float* samples, int len)
		{
			int i;
			int fir_index = this.fir_index;
			int fir_len = this.len;
			float* fir_kernel = this.fir_kernel;
			float* fir_queue = this.fir_queue;
			float* queue;
			float acc;

			for (i = 0; i < len; i += 2)
			{
				queue = fir_queue + fir_index;

				queue[0] = samples[i];

				acc = fir_kernel[0] * (queue[0] + queue[4 - 1])
					+ fir_kernel[1] * (queue[1] + queue[4 - 2]);

				samples[i] = acc;

				if (--fir_index < 0)
				{
					fir_index = fir_len * (SIZE_FACTOR - 1);
					Utils.Memcpy(fir_queue + fir_index + 1, fir_queue, (fir_len - 1) * sizeof(float));
				}
			}

			this.fir_index = fir_index;
		}

		void fir_interleaved_8(float* samples, int len)
		{
			int i;
			int fir_index = this.fir_index;
			int fir_len = this.len;
			float* fir_kernel = this.fir_kernel;
			float* fir_queue = this.fir_queue;
			float* queue;
			float acc;

			for (i = 0; i < len; i += 2)
			{
				queue = fir_queue + fir_index;

				queue[0] = samples[i];

				acc = fir_kernel[0] * (queue[0] + queue[8 - 1])
					+ fir_kernel[1] * (queue[1] + queue[8 - 2])
					+ fir_kernel[2] * (queue[2] + queue[8 - 3])
					+ fir_kernel[3] * (queue[3] + queue[8 - 4]);

				samples[i] = acc;

				if (--fir_index < 0)
				{
					fir_index = fir_len * (SIZE_FACTOR - 1);
					Utils.Memcpy(fir_queue + fir_index + 1, fir_queue, (fir_len - 1) * sizeof(float));
				}
			}

			this.fir_index = fir_index;
		}

		void fir_interleaved_12(float* samples, int len)
		{
			int i;
			int fir_index = this.fir_index;
			int fir_len = this.len;
			float* fir_kernel = this.fir_kernel;
			float* fir_queue = this.fir_queue;
			float* queue;
			float acc = 0;

			for (i = 0; i < len; i += 2)
			{
				queue = fir_queue + fir_index;

				queue[0] = samples[i];

				acc = fir_kernel[0] * (queue[0] + queue[12 - 1])
					+ fir_kernel[1] * (queue[1] + queue[12 - 2])
					+ fir_kernel[2] * (queue[2] + queue[12 - 3])
					+ fir_kernel[3] * (queue[3] + queue[12 - 4])
					+ fir_kernel[4] * (queue[4] + queue[12 - 5])
					+ fir_kernel[5] * (queue[5] + queue[12 - 6]);

				samples[i] = acc;

				if (--fir_index < 0)
				{
					fir_index = fir_len * (SIZE_FACTOR - 1);
					Utils.Memcpy(fir_queue + fir_index + 1, fir_queue, (fir_len - 1) * sizeof(float));
				}
			}

			this.fir_index = fir_index;
		}

		void fir_interleaved_24(float* samples, int len)
		{
			int i;
			int fir_index = this.fir_index;
			int fir_len = this.len;
			float* fir_kernel = this.fir_kernel;
			float* fir_queue = this.fir_queue;
			float* queue;
			float acc = 0;

			for (i = 0; i < len; i += 2)
			{
				queue = fir_queue + fir_index;

				queue[0] = samples[i];

				acc = fir_kernel[0] * (queue[0] + queue[24 - 1])
					+ fir_kernel[1] * (queue[1] + queue[24 - 2])
					+ fir_kernel[2] * (queue[2] + queue[24 - 3])
					+ fir_kernel[3] * (queue[3] + queue[24 - 4])
					+ fir_kernel[4] * (queue[4] + queue[24 - 5])
					+ fir_kernel[5] * (queue[5] + queue[24 - 6])
					+ fir_kernel[6] * (queue[6] + queue[24 - 7])
					+ fir_kernel[7] * (queue[7] + queue[24 - 8])
					+ fir_kernel[8] * (queue[8] + queue[24 - 9])
					+ fir_kernel[9] * (queue[9] + queue[24 - 10])
					+ fir_kernel[10] * (queue[10] + queue[24 - 11])
					+ fir_kernel[11] * (queue[11] + queue[24 - 12]);

				samples[i] = acc;

				if (--fir_index < 0)
				{
					fir_index = fir_len * (SIZE_FACTOR - 1);
					Utils.Memcpy(fir_queue + fir_index + 1, fir_queue, (fir_len - 1) * sizeof(float));
				}
			}

			this.fir_index = fir_index;
		}

		void fir_interleaved_generic(float* samples, int len)
		{
			int i;
			int fir_index = this.fir_index;
			int fir_len = this.len;
			float* fir_kernel = this.fir_kernel;
			float* fir_queue = this.fir_queue;
			float* queue;

			for (i = 0; i < len; i += 2)
			{
				queue = fir_queue + fir_index;

				queue[0] = samples[i];

				samples[i] = process_fir_taps(fir_kernel, queue, fir_len);

				if (--fir_index < 0)
				{
					fir_index = fir_len * (SIZE_FACTOR - 1);
					Utils.Memcpy(fir_queue + fir_index + 1, fir_queue, (fir_len - 1) * sizeof(float));
				}
			}

			this.fir_index = fir_index;
		}

		void fir_interleaved(float* samples, int len)
		{
			switch (this.len)
			{
				case 4:
					fir_interleaved_4(samples, len);
					break;
				case 8:
					fir_interleaved_8(samples, len);
					break;
				case 12:
					fir_interleaved_12(samples, len);
					break;
				case 24:
					fir_interleaved_24(samples, len);
					break;
				default:
					fir_interleaved_generic(samples, len);
					break;
			}
		}

		void delay_interleaved(float* samples, int len)
		{
			int i;
			int index;
			int half_len;
			float res;

			half_len = this.len >> 1;
			index = this.delay_index;

			for (i = 0; i < len; i += 2)
			{
				res = this.delay_line[index];
				this.delay_line[index] = samples[i];
				samples[i] = res;

				if (++index >= half_len)
				{
					index = 0;
				}
			}

			this.delay_index = index;
		}

		void remove_dc(float* samples, int len)
		{
			int i;
			float avg = this.avg;

			for (i = 0; i < len; i++)
			{
				samples[i] -= avg;
				avg += SCALE * samples[i];
			}

			this.avg = avg;
		}

		void translate_fs_4(float* samples, int len)
		{
			int i;
			float hbc = this.hbc;

			int j;

			for (i = 0; i < len / 4; i++)
			{
				j = i << 2;
				samples[j + 0] = -samples[j + 0];
				samples[j + 1] = -samples[j + 1] * hbc;
				//samples[j + 2] = samples[j + 2];
				samples[j + 3] = samples[j + 3] * hbc;
			}

			fir_interleaved(samples, len);
			delay_interleaved(samples + 1, len);
		}

		public void Process(float* samples, int len)
		{
			remove_dc(samples, len);
			translate_fs_4(samples, len);
		}

	}
}
