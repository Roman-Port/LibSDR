using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    unsafe class Acquire : Nrsc5Layer1Part
    {
        public const int BASE_SAMPLE_RATE = 744187;

        public const int DECIMATION = 2;
        public const int FFT_ARB = 2048* DECIMATION;
        public const int CP_ARB = 112* DECIMATION;
        public const int FFTCP_ARB = (FFT_ARB + CP_ARB);

        UnsafeBuffer inBufferB;
        UnsafeBuffer bufferB;
        UnsafeBuffer sumsB;
        UnsafeBuffer fftinB;
        UnsafeBuffer fftoutB;
        UnsafeBuffer shapeB;

        Complex* in_buffer;
        Complex* buffer;
        Complex* sums;//[FFTCP];
        Complex* fftin;//[FFT];
        Complex* fftout;//[FFT];
        float* shape;//[FFTCP];
                     //fftwf_plan fft;

        int inputBufferUsed;
        float prev_angle;
        Complex phase = new Complex(1, 0);
        int cfo;
        int skip;
        IQFirFilter filter;

        SYNC_STATE syncState { get => sync.syncState; set => sync.syncState = value; }

        public Acquire()
        {
            //Create buffers
            filter = new IQFirFilter(FilterBuilder.MakeBandPassKernel(SAMPLE_RATE, 32, 122457, 200000, WindowType.None));
            inBufferB = UnsafeBuffer.Create(FFTCP_ARB * (ACQUIRE_SYMBOLS + 1), sizeof(Complex));
            in_buffer = (Complex*)inBufferB;
            bufferB = UnsafeBuffer.Create(FFTCP_ARB * (ACQUIRE_SYMBOLS + 1), sizeof(Complex));
            buffer = (Complex*)bufferB;
            sumsB = UnsafeBuffer.Create(FFTCP_ARB, sizeof(Complex));
            sums = (Complex*)sumsB;
            fftinB = UnsafeBuffer.Create(FFT_ARB, sizeof(Complex));
            fftin = (Complex*)fftinB;
            fftoutB = UnsafeBuffer.Create(FFT_ARB, sizeof(Complex));
            fftout = (Complex*)fftoutB;
            shapeB = UnsafeBuffer.Create(FFTCP_ARB, sizeof(float));
            shape = (float*)shapeB;

            //Create window
            for (int i = 0; i < FFTCP_ARB; ++i)
            {
                // Pulse shaping window function
                if (i < CP_ARB)
                    this.shape[i] = (float)Math.Sin(M_PI / 2 * i / CP_ARB);
                else if (i < FFT_ARB)
                    this.shape[i] = 1;
                else
                    this.shape[i] = (float)Math.Cos(M_PI / 2 * (i - FFT_ARB) / CP_ARB);
            }
        }

        public void SetSkipOffset(int skip)
        {
            this.skip += skip * FFTCP;
        }

        const int TARGET_BUFFER_USAGE = FFTCP_ARB * (ACQUIRE_SYMBOLS + 1);

        public void Process(Complex* buf, int count)
        {
            //Process skipped samples
            int skipped = Math.Min(skip, count);
            buf += skipped;
            count -= skipped;
            skip -= skipped;

            //Process
            int blocksProcessed = 0;
            while (count != 0)
            {
                //Transfer the samples we're able to use into the buffer
                int additionalSamplesNeeded = TARGET_BUFFER_USAGE - inputBufferUsed;
                int additionalSamplesTransferrable = Math.Min(additionalSamplesNeeded, count);
                Utils.Memcpy(in_buffer + inputBufferUsed, buf, additionalSamplesTransferrable * sizeof(Complex));
                buf += additionalSamplesTransferrable;
                count -= additionalSamplesTransferrable;
                inputBufferUsed += additionalSamplesTransferrable;

                //Check if we can process a block or not
                if (inputBufferUsed + count >= TARGET_BUFFER_USAGE)
                {
                    //We have enough samples to process a block!
                    acquire_process_block();
                }

                //Notify
                if (blocksProcessed == 0)
                    sync.OnBuffersCleared();

                //Update
                blocksProcessed++;
            }
        }

        private int acquire_process_block()
        {
            Complex max_v = 0, phase_increment;
            float angle, angle_diff, angle_factor, max_mag = -1.0f;
            int samperr = 0;
            int i, j;

            if (inputBufferUsed != TARGET_BUFFER_USAGE)
                throw new Exception();

            /*byte[] bb = new byte[sizeof(Complex)];
            fixed(byte* bbp = bb)
            {
                Complex* bbc = (Complex*)bbp;
                for(i = 0; i<FFTCP * (ACQUIRE_SYMBOLS + 1); i++)
                {
                    test.Read(bb, 0, bb.Length);
                    buffer[i] = bbc[0];
                }
            }*/

            //ForwardTransform(buffer, FFT);
            /*byte[] mBuf = new byte[FFT * sizeof(Complex)];
            fixed (byte* mBufPtr = mBuf)
                Utils.Memcpy(mBufPtr, buffer, FFT * sizeof(Complex));
            output.Write(mBuf, 0, mBuf.Length);*/

            //t.WriteSamples(in_buffer, FFTCP * (ACQUIRE_SYMBOLS + 1));

            if (syncState == SYNC_STATE.SYNC_STATE_FINE)
            {
                samperr = FFTCP_ARB / 2 + sync.samperr;
                sync.samperr = 0;

                angle_diff = -sync.angle;
                sync.angle = 0;
                angle = this.prev_angle + angle_diff;
                this.prev_angle = angle;
            }
            else
            {
                filter.Process(in_buffer, FFTCP_ARB * (ACQUIRE_SYMBOLS + 1));
                for (i = 0; i < FFTCP_ARB * (ACQUIRE_SYMBOLS + 1); i++)
                {
                    //Complex y = filter.fir_q15_execute(in_buffer[i]);
                    Complex y = in_buffer[i];
                    this.buffer[i] = (y.Real + I * y.Imag);
                }
                test.WriteSamples(this.buffer, FFTCP_ARB * (ACQUIRE_SYMBOLS + 1));
                //return FFTCP_ARB * (ACQUIRE_SYMBOLS + 1);

                memset(this.sums, 0, sizeof(Complex) * FFTCP_ARB);
                for (i = 0; i < FFTCP_ARB; ++i)
                {
                    for (j = 0; j < ACQUIRE_SYMBOLS; ++j)
                        this.sums[i] += this.buffer[i + j * FFTCP_ARB] * conjf(this.buffer[i + j * FFTCP_ARB + FFT_ARB]);
                }

                for (i = 0; i < FFTCP_ARB; ++i)
                {
                    float mag;
                    Complex v = 0;

                    for (j = 0; j < CP_ARB; ++j)
                        v += this.sums[(i + j) % FFTCP_ARB] * this.shape[j] * this.shape[j + FFT_ARB];

                    mag = normf(v);
                    if (mag > max_mag)
                    {
                        max_mag = mag;
                        max_v = v;
                        samperr = (i + FFTCP_ARB) % FFTCP_ARB;
                    }
                }

                angle_diff = cargf(max_v * cexpf(I * -this.prev_angle));
                angle_factor = (this.prev_angle != 0) ? 0.25f : 1.0f;
                angle = this.prev_angle + (angle_diff * angle_factor);
                this.prev_angle = angle;
                syncState = SYNC_STATE.SYNC_STATE_COARSE;
            }

            for (i = 0; i < FFTCP_ARB * (ACQUIRE_SYMBOLS + 1); i++)
                this.buffer[i] = in_buffer[i].Conjugate();

            sync.sync_adjust(FFTCP_ARB / 2 - samperr);
            angle -= 2 * M_PI * this.cfo;

            this.phase *= cexpf(-(FFTCP_ARB / 2 - samperr) * angle / FFT_ARB * I);

            phase_increment = cexpf(angle / FFT_ARB * I);
            for (i = 0; i < ACQUIRE_SYMBOLS; ++i)
            {
                for (j = 0; j < FFTCP_ARB; ++j)
                {
                    Complex sample = this.phase * this.buffer[i * FFTCP_ARB + j + samperr];
                    if (j < CP_ARB)
                        this.fftin[j] = this.shape[j] * sample;
                    else if (j < FFT_ARB)
                        this.fftin[j] = sample;
                    else
                        this.fftin[j - FFT_ARB] += this.shape[j] * sample;

                    this.phase *= phase_increment;
                }
                this.phase /= cabsf(this.phase);

                //WEIRD FIX
                /*for (j = 0; j < FFT; j++)
                    fftin[j] = fftin[j] * -0.5f;*/

                //Console.WriteLine(this.fftin[10]);
                //Console.ReadLine();
                //fftwf_execute(this.fft);
                //fftshift(this.fftout, FFT);
                //Fourier.ForwardTransform(fftin, FFT);
                //fftout contains an FFT of the data of size FFT (NOT FFTCP). When plotted, it looks like this https://i.imgur.com/xaooN0I.png
                //sync_push(&this.input->sync, this.fftout); //This pushes FFT frames out

                

                ForwardTransform((float*)fftin, FFT_ARB);
                fftshift(fftin, FFT_ARB);

                //test
                Complex* fftOffset = fftin + ((FFT_ARB - FFT) / 2);
                /*for (int c = 0; c < 2048 / 2; c++)
                {
                    Complex temp = fftOffset[c];
                    fftOffset[c] = fftOffset[2047 - c];
                    fftOffset[2047 - c] = temp;
                }*/


                //debug
                byte[] tt = new byte[2048*8];
                fixed (byte* ttp = tt)
                    Utils.Memcpy(ttp, fftOffset, 2048 * sizeof(Complex));
                test2.Write(tt, 0, tt.Length);

                sync.sync_push(fftOffset);
            }

            //Calculate the number of consumed samples and move them to the beginning of the buffer
            int consumed = inputBufferUsed - (FFTCP_ARB + (FFTCP_ARB / 2 - samperr));
            Utils.Memcpy(in_buffer, in_buffer + consumed, (inputBufferUsed - consumed) * sizeof(Complex));
            inputBufferUsed -= consumed;

            //Return the number of consumed samples
            return consumed;
        }

        LibSDR.Extras.TestOutput test = new Extras.TestOutput(SAMPLE_RATE/2);
        System.IO.FileStream test2 = new System.IO.FileStream("E:\\debug_test_new.bin", System.IO.FileMode.Create);

        public static void fftshift(Complex* x, int size)
        {
            int i, h = size / 2;
            for (i = 0; i < h; i += 4)
            {
                Complex t1 = x[i], t2 = x[i + 1], t3 = x[i + 2], t4 = x[i + 3];
                x[i] = x[i + h];
                x[i + 1] = x[i + 1 + h];
                x[i + 2] = x[i + 2 + h];
                x[i + 3] = x[i + 3 + h];
                x[i + h] = t1;
                x[i + 1 + h] = t2;
                x[i + 2 + h] = t3;
                x[i + 3 + h] = t4;
            }
        }

        public static void ForwardTransform(float* samples, int length)
        {
            //Provided by SDRSharp
            int nm1 = length - 1;
            int nd2 = length / 2;
            int i, j, jm1, k, l, m, le, le2, ip;
            float ur, ui, sr, si, tr, ti;

            m = 0;
            i = length;
            while (i > 1)
            {
                ++m;
                i = (i >> 1);
            }

            j = nd2;

            for (i = 1; i < nm1; ++i)
            {
                if (i < j)
                {
                    tr = samples[2 * j];
                    ti = samples[2 * j + 1];
                    samples[2 * j] = samples[2 * i];
                    samples[2 * j + 1] = samples[2 * i + 1];
                    samples[2 * i] = tr;
                    samples[2 * i + 1] = ti;
                }

                k = nd2;

                while (k <= j)
                {
                    j = j - k;
                    k = k / 2;
                }

                j += k;
            }

            for (l = 1; l <= m; ++l)
            {
                le = 1 << l;
                le2 = le / 2;
                ur = 1;
                ui = 0;

                sr = (float)Math.Cos(M_PI / le2);
                si = -(float)Math.Sin(M_PI / le2);

                for (j = 1; j <= le2; ++j)
                {
                    jm1 = j - 1;

                    for (i = jm1; i <= nm1; i += le)
                    {
                        ip = i + le2;
                        tr = samples[2 * ip] * ur - samples[2 * ip + 1] * ui;
                        ti = samples[2 * ip] * ui + samples[2 * ip + 1] * ur;
                        samples[2 * ip] = samples[2 * i] - tr;
                        samples[2 * ip + 1] = samples[2 * i + 1] - ti;
                        samples[2 * i] = samples[2 * i] + tr;
                        samples[2 * i + 1] = samples[2 * i + 1] + ti;
                    }

                    tr = ur;
                    ur = tr * sr - ui * si;
                    ui = tr * si + ui * sr;
                }
            }
        }

        public void acquire_cfo_adjust(int cfo)
        {
            float hz;

            if (cfo == 0)
                return;

            this.cfo += cfo;
            hz = (float)this.cfo * SAMPLE_RATE / FFT_ARB;

            Console.WriteLine("CFO: " + hz);
        }
    }
}
