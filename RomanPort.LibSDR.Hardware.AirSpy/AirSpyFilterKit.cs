using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Hardware.AirSpy
{
    unsafe class AirSpyFilterKit
    {
		private int fir_index;
		private int len;
		int delay_index;
		private float* fir_kernel;
		private float* fir_queue;
		private float* delay_line;
		private UnsafeBuffer fir_kernel_buffer;
		private UnsafeBuffer fir_queue_buffer;
		private UnsafeBuffer delay_line_buffer;
		private float hbc;
		private float avg;

		private const int SIZE_FACTOR = 32;

		public AirSpyFilterKit()
        {
			len = AirSpyConst.HP_KERNEL.Length / 2 + 1;
			fir_kernel_buffer = UnsafeBuffer.Create(len, out fir_kernel);
			fir_queue_buffer = UnsafeBuffer.Create(len * SIZE_FACTOR, out fir_queue);
			delay_line_buffer = UnsafeBuffer.Create(len, out delay_line);
			for (int i = 0; i < len; i++)
				fir_kernel[i] = AirSpyConst.HP_KERNEL[i * 2];
			hbc = AirSpyConst.HP_KERNEL[AirSpyConst.HP_KERNEL.Length / 2];
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

		private void fir_interleaved(float* samples, int len)
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

		const float SCALE = 0.01f;

		void remove_dc(float* samples, int len)
		{
			for (int i = 0; i < len; i++)
			{
				samples[i] -= avg;
				avg += SCALE * samples[i];
			}
		}

		public void Apply(float* samples, int count)
        {
			//Translate
			int j;
			for (int i = 0; i < count / 4; i++)
			{
				j = i << 2;
				samples[j + 0] = -samples[j + 0];
				samples[j + 1] = -samples[j + 1] * hbc;
				//samples[j + 2] = samples[j + 2];
				samples[j + 3] = samples[j + 3] * hbc;
			}

			//Remove DC
			//remove_dc(samples, count);

			//FIR filter
			fir_interleaved(samples, count);

			//Delay
			delay_interleaved(samples + 1, count);
		}
	}
}
