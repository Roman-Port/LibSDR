using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Components.Resamplers;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames;
using RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1
{
    /// <summary>
    /// Emits NRSC5 frames while decoding
    /// </summary>
    public unsafe class Nrsc5Layer1Decoder
    {
        private Acquire acquire;
        private Sync sync;
        private Decode decode;
        private Pids pids;
        private Frame frame;

        private ComplexResamplerPipeline resampler;

        public event Nrsc5Layer1OnFrameEventArgs<FramePids> OnPidsFrame;
        public event Nrsc5Layer1OnFrameEventArgs<FrameAas> OnAasFrame;
        public event Nrsc5Layer1OnFrameEventArgs<FramePdu> OnPduFrame;

        private const int TARGET_SAMPLE_RATE = 744187;

        public Nrsc5Layer1Decoder(float sampleRate)
        {
            //Create resampler and its buffer
            resampler = new ComplexResamplerPipeline(sampleRate, TARGET_SAMPLE_RATE, 2048*8, ProcessBlock);

            //Create compnents
            acquire = new Acquire();
            sync = new Sync();
            decode = new Decode();
            pids = new Pids();
            frame = new Frame();

            //Apply events
            pids.OnPidsFrame += Pids_OnPidsFrame;
            frame.OnAasFrame += Frame_OnAasFrame;
            frame.OnPduFrame += Frame_OnPduFrame;

            //Link components
            acquire.SetComponents(acquire, sync, decode, pids, frame);
            sync.SetComponents(acquire, sync, decode, pids, frame);
            decode.SetComponents(acquire, sync, decode, pids, frame);
            pids.SetComponents(acquire, sync, decode, pids, frame);
            frame.SetComponents(acquire, sync, decode, pids, frame);
        }

        private LibSDR.Extras.TestOutput test = new Extras.TestOutput(TARGET_SAMPLE_RATE);
        //private LibSDR.Extras.TestOutput test = new Extras.TestOutput(1500000);
        private long test2 = 0;

        public unsafe void Process(Complex* ptr, int count)
        {
            /*while(test2 < 6000000)
            {
                test2 += count;
                return;
            }*/
            //resampler.Process(ptr, count);
            ProcessBlock(ptr, count);
        }

        private void ProcessBlock(Complex* input, int count)
        {
            test.WriteSamples(input, count);
            acquire.Process(input, count);
        }

        private void Frame_OnPduFrame(FramePdu frame)
        {
            OnPduFrame?.Invoke(this, frame);
        }

        private void Frame_OnAasFrame(FrameAas frame)
        {
            OnAasFrame?.Invoke(this, frame);
        }

        private void Pids_OnPidsFrame(byte[] bits)
        {
            OnPidsFrame?.Invoke(this, new FramePids(bits));
        }
    }
}
