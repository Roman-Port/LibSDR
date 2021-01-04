using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    unsafe class Sync : Nrsc5Layer1Part
    {
        public const int PM_PARTITIONS = 10;
        public const int MAX_PARTITIONS = 14;
        public const int PARTITION_DATA_CARRIERS = 18;
        public const int PARTITION_WIDTH = 19;
        public const int MIDDLE_REF_SC = 30; // midpoint of Table 11-3 in 1011s.pdf

        private SYNC_STATE _syncState;
        public SYNC_STATE syncState
        {
            get => _syncState;
            set
            {
                Console.WriteLine("STATE UPDATED: " + _syncState);
                _syncState = value;
            }
        }

        UnsafeBuffer costas_freqB;
        UnsafeBuffer costas_phaseB;
        Complex[,] buffer;
        float[,] phases;
        int idx;
        int psmi;
        bool cfo_waiting;
        public int samperr;
        public float angle;

        float alpha;
        float beta;
        float* costas_freq;
        float* costas_phase;

        int mer_cnt;
        float error_lb;
        float error_ub;

        public Sync()
        {
            buffer = new Complex[FFT, BLKSZ];
            phases = new float[FFT, BLKSZ];
            costas_freqB = UnsafeBuffer.Create(FFT, sizeof(float));
            costas_freq = (float*)costas_freqB;
            costas_phaseB = UnsafeBuffer.Create(FFT, sizeof(float));
            costas_phase = (float*)costas_phaseB;

            float loop_bw = 0.05f, damping = 0.70710678f;
            float denom = 1 + (2 * damping * loop_bw) + (loop_bw * loop_bw);
            alpha = (4 * damping * loop_bw) / denom;
            beta = (4 * loop_bw * loop_bw) / denom;
        }

        public void OnBuffersCleared()
        {
            cfo_waiting = false;
        }

        public void sync_adjust(int sample_adj)
        {
            int i;
            for (i = 0; i < MAX_PARTITIONS * PARTITION_WIDTH + 1; i++)
            {
                costas_phase[LB_START + i] -= sample_adj * (LB_START + i - (FFT / 2)) * 2 * M_PI / FFT;
                costas_phase[UB_END - i] -= sample_adj * (UB_END - i - (FFT / 2)) * 2 * M_PI / FFT;
                //Console.WriteLine($"costas_phase={costas_phase[LB_START + i]}, sample_adj={sample_adj}");
            }
        }

        public void sync_push(Complex* fftout)
        {
            //We're now reading the FFT of length FFT (NOT FFTCP)
            for (int i = 0; i < MAX_PARTITIONS * PARTITION_WIDTH + 1; i++)
            {
                buffer[LB_START + i, idx] = fftout[LB_START + i];
                buffer[UB_END - i, idx] = fftout[UB_END - i];
            }

            if (++idx == BLKSZ)
            {
                idx = 0;
                sync_process();
            }
        }

        void sync_process()
        {
            int i, partitions_per_band;

            switch (this.psmi)
            {
                case 2:
                    partitions_per_band = 11;
                    break;
                case 3:
                    partitions_per_band = 12;
                    break;
                case 5:
                case 6:
                case 11:
                    partitions_per_band = 14;
                    break;
                default:
                    partitions_per_band = 10;
                    break;
            }

            /*if(syncState == SYNC_STATE.SYNC_STATE_FINE)
            {
                Console.WriteLine($"[SS] costas_phase={costas_phase[LB_START]}, buffer-r={buffer[LB_START, 0].Real}, buffer-i={buffer[LB_START, 0].Imag}");
                Console.ReadLine();
            }*/


            //Console.WriteLine(phases[497, 0]);
            for (i = 0; i < partitions_per_band * PARTITION_WIDTH + 1; i += PARTITION_WIDTH)
            {
                adjust_ref(LB_START + i, 0);
                adjust_ref(UB_END - i, 0);
            }
            //Console.WriteLine(phases[497, 0]);
            //Console.ReadLine();

            // check if we now have synchronization
            if (this.syncState == SYNC_STATE.SYNC_STATE_COARSE)
            {
                int good_refs = 0;
                for (i = 0; i <= partitions_per_band; i++)
                {
                    if (find_first_block(LB_START + i * PARTITION_WIDTH, (MIDDLE_REF_SC - i) & 0x3) == 0)
                        good_refs++;
                    if (find_first_block(UB_END - i * PARTITION_WIDTH, (MIDDLE_REF_SC - i) & 0x3) == 0)
                        good_refs++;
                }

                if (good_refs >= 4)
                {
                    syncState = SYNC_STATE.SYNC_STATE_FINE;
                    decode.decode_reset();
                    //frame_reset(&this.input->frame);
                }
                else if (!this.cfo_waiting)
                {
                    detect_cfo();
                }
            }

            // if we are still synchronized
            if (this.syncState == SYNC_STATE.SYNC_STATE_FINE)
            {
                float samperr = 0, angle = 0;
                float sum_xy = 0, sum_x2 = 0;
                for (i = 0; i < partitions_per_band * PARTITION_WIDTH; i += PARTITION_WIDTH)
                {
                    adjust_data(LB_START + i, LB_START + i + PARTITION_WIDTH);
                    adjust_data(UB_END - i - PARTITION_WIDTH, UB_END - i);

                    samperr += phase_diff(this.phases[LB_START + i, 0], this.phases[LB_START + i + PARTITION_WIDTH, 0]);
                    samperr += phase_diff(this.phases[UB_END - i - PARTITION_WIDTH, 0], this.phases[UB_END - i, 0]);
                }
                samperr = samperr / (partitions_per_band * 2) * FFT / PARTITION_WIDTH / (2 * M_PI);

                for (i = 0; i < partitions_per_band * PARTITION_WIDTH + 1; i += PARTITION_WIDTH)
                {
                    float x, y;

                    x = LB_START + i - (FFT / 2);
                    y = this.costas_freq[LB_START + i];
                    angle += y;
                    sum_xy += x * y;
                    sum_x2 += x * x;

                    x = UB_END - i - (FFT / 2);
                    y = this.costas_freq[UB_END - i];
                    angle += y;
                    sum_xy += x * y;
                    sum_x2 += x * x;
                }
                samperr -= (sum_xy / sum_x2) * FFT / (2 * M_PI) * ACQUIRE_SYMBOLS;
                this.samperr = (int)roundf(samperr);

                angle /= (partitions_per_band + 1) * 2;
                this.angle = angle;

                // Calculate modulation error (appears to be known good)
                float error_lb = 0, error_ub = 0;
                for (int n = 0; n < BLKSZ; n++)
                {
                    Complex c, ideal;
                    for (i = 0; i < partitions_per_band * PARTITION_WIDTH; i += PARTITION_WIDTH)
                    {
                        int j;
                        for (j = 1; j < PARTITION_WIDTH; j++)
                        {
                            c = this.buffer[LB_START + i + j, n];
                            ideal = CMPLXF(c.Real >= 0 ? 1 : -1, c.Imag >= 0 ? 1 : -1);
                            error_lb += normf(ideal - c);

                            c = this.buffer[UB_END - i - PARTITION_WIDTH + j, n];
                            ideal = CMPLXF(c.Real >= 0 ? 1 : -1, c.Imag >= 0 ? 1 : -1);
                            error_ub += normf(ideal - c);
                        }
                    }
                }

                this.error_lb += error_lb;
                this.error_ub += error_ub;

                // Display average MER for each sideband
                if (++this.mer_cnt == 16)
                {
                    float signal = 2 * BLKSZ * (partitions_per_band * PARTITION_DATA_CARRIERS) * this.mer_cnt;
                    float mer_db_lb = 10 * (float)Math.Log10(signal / this.error_lb);
                    float mer_db_ub = 10 * (float)Math.Log10(signal / this.error_ub);

                    //nrsc5_report_mer(this.input->radio, mer_db_lb, mer_db_ub);

                    this.mer_cnt = 0;
                    this.error_lb = 0;
                    this.error_ub = 0;
                }

                // Soft demod based on MER for each sideband
                float mer_lb = 2 * BLKSZ * (partitions_per_band * PARTITION_DATA_CARRIERS) / error_lb; //NO LONGER A PROBLEM, I THINK: error_lb and error_ub are very high for us. In the working example, they are error_lb=125.279953, error_ub=128.680084
                float mer_ub = 2 * BLKSZ * (partitions_per_band * PARTITION_DATA_CARRIERS) / error_ub;
                float mult_lb = Math.Max(Math.Min(mer_lb * 10, 127), 1);
                float mult_ub = Math.Max(Math.Min(mer_ub * 10, 127), 1);

                for (int n = 0; n < BLKSZ; n++)
                {
                    Complex c;
                    for (i = LB_START; i < LB_START + (PM_PARTITIONS * PARTITION_WIDTH); i += PARTITION_WIDTH)
                    {
                        int j;
                        for (j = 1; j < PARTITION_WIDTH; j++)
                        {
                            c = this.buffer[i + j, n];
                            decode_push_pm(DEMOD(c.Real) * mult_lb);
                            decode_push_pm(DEMOD(c.Imag) * mult_lb);
                            //Console.WriteLine($"c-real={c.Real}, c-imag={c.Imag}, mult_lb={mult_lb}");
                            //Console.ReadLine();
                        }
                    }
                    for (i = UB_END - (PM_PARTITIONS * PARTITION_WIDTH); i < UB_END; i += PARTITION_WIDTH)
                    {
                        int j;
                        for (j = 1; j < PARTITION_WIDTH; j++)
                        {
                            c = this.buffer[i + j, n];
                            decode_push_pm(DEMOD(c.Real) * mult_ub);
                            decode_push_pm(DEMOD(c.Imag) * mult_ub);
                        }
                    }
                    if (this.psmi == 3)
                    {
                        for (i = LB_START + (PM_PARTITIONS * PARTITION_WIDTH); i < LB_START + (PM_PARTITIONS + 2) * PARTITION_WIDTH; i += PARTITION_WIDTH)
                        {
                            int j;
                            for (j = 1; j < PARTITION_WIDTH; j++)
                            {
                                c = this.buffer[i + j, n];
                                decode_push_px1(DEMOD(c.Real) * mult_lb);
                                decode_push_px1(DEMOD(c.Imag) * mult_lb);
                            }
                        }
                        for (i = UB_END - (PM_PARTITIONS + 2) * PARTITION_WIDTH; i < UB_END - (PM_PARTITIONS * PARTITION_WIDTH); i += PARTITION_WIDTH)
                        {
                            int j;
                            for (j = 1; j < PARTITION_WIDTH; j++)
                            {
                                c = this.buffer[i + j, n];
                                decode_push_px1(DEMOD(c.Real) * mult_ub);
                                decode_push_px1(DEMOD(c.Imag) * mult_ub);
                            }
                        }
                    }
                }
            }
        }

        void decode_push_pm(float sbit)
        {
            //Console.WriteLine("decode_push_pm: " + sbit);
            decode.decode_push_pm((sbyte)sbit);
        }

        void decode_push_px1(float sbit)
        {
            Console.WriteLine("decode_push_px1: " + sbit);
            //decode.decode_push_px1((sbyte)sbit);
        }

        float DEMOD(float v)
        {
            return v >= 0 ? 1 : -1;
        }

        // sync bits (after DBPSK)
        static readonly int[] SYNC_BITS = {
            -1, 1, -1, -1, -1, 1, 1
        };

        void adjust_ref(int r, int cfo)
        {
            int n;
            float cfo_freq = 2 * M_PI * cfo * CP / FFT;

            for (n = 0; n < BLKSZ; n++)
            {
                float error = (float)(cargf(this.buffer[r, n] * this.buffer[r, n] * cexpf(-I * 2 * this.costas_phase[r])) * 0.5);
                //Console.WriteLine($"[ref] ref={r}, costas_freq={this.costas_freq[r]}, error={error}, phases={phases[r, n]}, beta={beta}, buffer-r={this.buffer[r, n].Real}, buffer-i={this.buffer[r, n].Imag}");

                this.phases[r, n] = this.costas_phase[r];
                this.buffer[r, n] *= cexpf(-I * this.costas_phase[r]);

                this.costas_freq[r] += this.beta * error;
                if (this.costas_freq[r] > 0.5) this.costas_freq[r] = 0.5f;
                if (this.costas_freq[r] < -0.5) this.costas_freq[r] = -0.5f;
                this.costas_phase[r] += this.costas_freq[r] + cfo_freq + (this.alpha * error);
                if (this.costas_phase[r] > M_PI) this.costas_phase[r] -= 2 * M_PI;
                if (this.costas_phase[r] < -M_PI) this.costas_phase[r] += 2 * M_PI;

                //Console.WriteLine($"[ref] ref={r}, costas_freq={this.costas_freq[r]}, error={error}, phases={phases[r, n]}, beta={beta}, buffer-r={this.buffer[r, n].Real}, buffer-i={this.buffer[r, n].Imag}");
                //Console.ReadLine();
            }

            // compare to sync bits
            float x = 0;
            for (n = 0; n < SYNC_BITS.Length; n++)
                x += this.buffer[r, n].Real * SYNC_BITS[n];
            if (x < 0)
            {
                // adjust phase by pi to compensate
                for (n = 0; n < BLKSZ; n++)
                {
                    this.phases[r, n] += M_PI;
                    this.buffer[r, n] *= -1;
                }
                this.costas_phase[r] += M_PI;
            }
        }

        int find_first_block(int r, int rsid)
        {
            int[] needle = new int[] {
                0, 1, 1, 0, 0, 1, 0, -1, -1, 1, rsid >> 1, rsid & 1, 0, (rsid >> 1) ^ (rsid & 1), 0, -1, 0, 0, 0, 0, -1, 1, 1, 1
            };
            byte[] data = new byte[BLKSZ];

            decode_dbpsk(r, data, BLKSZ);
            int n = fuzzy_match(needle, needle.Length, data, BLKSZ);
            if (n == 0)
                psmi = (data[25] << 5) | (data[26] << 4) | (data[27] << 3) | (data[28] << 2) | (data[29] << 1) | data[30];
            return n;
        }

        void decode_dbpsk(int dataIndex, byte[] data, int size)
        {
            byte prev = 0;

            for (int n = 0; n < size; n++)
            {
                byte bit = buffer[dataIndex, n].Real <= 0 ? (byte)0 : (byte)1;
                data[n] = (byte)(bit ^ prev);
                prev = bit;
            }
        }

        int fuzzy_match(int[] needle, int needle_size, byte[] data, int size)
        {
            for (int n = 0; n < size; n++)
            {
                int i;
                for (i = 0; i < needle_size; i++)
                {
                    // first bit of data may be wrong, so ignore
                    if ((n + i) % size == 0) continue;
                    // ignore don't care bits
                    if (needle[i] < 0) continue;
                    // test if bit is correct
                    if (needle[i] != data[(n + i) % size])
                        break;
                }
                if (i == needle_size)
                    return n;
            }
            return -1;
        }

        void adjust_data(int lower, int upper)
        {
            float smag0, smag19;
            smag0 = calc_smag(lower);
            smag19 = calc_smag(upper);

            for (int n = 0; n < BLKSZ; n++)
            {
                Complex upper_phase = cexpf(phases[upper, n] * I);
                Complex lower_phase = cexpf(phases[lower, n] * I);

                //Console.WriteLine(phases[upper, n]);
                //Console.WriteLine(upper_phase.ToString());
                //Console.ReadLine();

                for (int k = 1; k < PARTITION_WIDTH; k++)
                {
                    // average phase difference
                    Complex C = CMPLXF(PARTITION_WIDTH, PARTITION_WIDTH) / (k * smag19 * upper_phase + (PARTITION_WIDTH - k) * smag0 * lower_phase);
                    // adjust sample
                    //Console.WriteLine($"[ADJ] samp-r={buffer[lower + k, n].Real}, samp-i={buffer[lower + k, n].Imag}, C-r={C.Real}, C-i={C.Imag}, smag19={smag19}, smag0={smag0}, upper_phase-r={upper_phase.Real}, upper_phase-i={upper_phase.Imag}");
                    //Console.ReadLine();
                    buffer[lower + k, n] *= C;
                }
            }
        }

        float calc_smag(int r)
        {
            float sum = 0;
            // phase was already corrected, so imaginary component is zero
            for (int n = 0; n < BLKSZ; n++)
                sum += Math.Abs(buffer[r, n].Real);
            return sum / BLKSZ;
        }

        float phase_diff(float a, float b)
        {
            float diff = a - b;
            while (diff > M_PI / 2) diff -= M_PI;
            while (diff < -M_PI / 2) diff += M_PI;
            return diff;
        }

        void detect_cfo()
        {
            for (int cfo = -2 * PARTITION_WIDTH; cfo < 2 * PARTITION_WIDTH; cfo++)
            {
                int offset;
                int best_offset = -1;
                int best_count = 0;
                int[] offset_count = new int[BLKSZ];

                for (int i = 0; i <= PM_PARTITIONS; i++)
                {
                    adjust_ref(cfo + LB_START + i * PARTITION_WIDTH, cfo);
                    offset = find_ref(cfo + LB_START + i * PARTITION_WIDTH, (MIDDLE_REF_SC - i) & 0x3);
                    reset_ref(cfo + LB_START + i * PARTITION_WIDTH);
                    if (offset >= 0)
                        offset_count[offset]++;

                    adjust_ref(cfo + UB_END - i * PARTITION_WIDTH, cfo);
                    offset = find_ref(cfo + UB_END - i * PARTITION_WIDTH, (MIDDLE_REF_SC - i) & 0x3);
                    reset_ref(cfo + UB_END - i * PARTITION_WIDTH);
                    if (offset >= 0)
                        offset_count[offset]++;
                }

                for (offset = 0; offset < BLKSZ; offset++)
                {
                    if (offset_count[offset] > best_count)
                    {
                        best_offset = offset;
                        best_count = offset_count[offset];
                    }
                }

                if (best_offset >= 0 && best_count >= 3)
                {
                    // At least three offsets matched, so this is likely the correct CFO.
                    acquire.SetSkipOffset(best_offset);
                    acquire.acquire_cfo_adjust(cfo);

                    Console.WriteLine($"block at {best_offset}");

                    // Wait until the buffers have cleared before measuring again.
                    this.cfo_waiting = true;
                    break;
                }
            }

            void reset_ref(int r)
            {
                for (int n = 0; n < BLKSZ; n++)
                    buffer[r, n] *= cexpf(I * phases[r, n]);
            }

            int find_ref(int r, int rsid)
            {
                int[] needle = {
                    0, 1, 1, 0, 0, 1, 0, -1, -1, 1, rsid >> 1, rsid & 1, 0, (rsid >> 1) ^ (rsid & 1), 0, -1, -1, -1, -1, -1, -1, 1, 1, 1
                };
                byte[] data = new byte[BLKSZ];

                decode_dbpsk(r, data, BLKSZ);
                return fuzzy_match(needle, needle.Length, data, BLKSZ);
            }
        }
    }
}
