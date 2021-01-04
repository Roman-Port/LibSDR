using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.NRSC5.Framework;
using RomanPort.LibSDR.NRSC5.Framework.Layer1;
using RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.NRSC5
{
    public unsafe class HDRadioDecoder
    {
        private Nrsc5Layer1Decoder basebandDecoder;
        private IAudioDecoder audioDecoder;
        private UnsafeBuffer audioDecoderBuffer;
        private short* audioDecoderBufferPtr;

        public event HDRadioDecoderAudioEventArgs OnAudioEvent
        {
            add
            {
                for (int i = 0; i < programEvents.Length; i++) { programEvents[i] += value; }
            }
            remove
            {
                for (int i = 0; i < programEvents.Length; i++) { programEvents[i] -= value; }
            }
        }
        public event HDRadioDecoderAudioEventArgs OnAudioProgram1Event { add => programEvents[0] += value; remove => programEvents[0] -= value; }
        public event HDRadioDecoderAudioEventArgs OnAudioProgram2Event { add => programEvents[1] += value; remove => programEvents[1] -= value; }
        public event HDRadioDecoderAudioEventArgs OnAudioProgram3Event { add => programEvents[2] += value; remove => programEvents[2] -= value; }
        public event HDRadioDecoderAudioEventArgs OnAudioProgram4Event { add => programEvents[3] += value; remove => programEvents[3] -= value; }
        private HDRadioDecoderAudioEventArgs[] programEvents = new HDRadioDecoderAudioEventArgs[4];

        private const int AUDIO_DECODER_BUFFER_SIZE = 4096;

        public HDRadioDecoder(IAudioDecoder audioDecoder, int sampleRate)
        {
            //Open the layer1 decoder
            basebandDecoder = new Nrsc5Layer1Decoder(sampleRate);
            basebandDecoder.OnPidsFrame += Decoder_OnPidsFrame;
            basebandDecoder.OnAasFrame += Decoder_OnAasFrame;
            basebandDecoder.OnPduFrame += Decoder_OnPduFrame;

            //Open audio decoder buffer
            audioDecoderBuffer = UnsafeBuffer.Create<short>(AUDIO_DECODER_BUFFER_SIZE, out audioDecoderBufferPtr);

            //Open the audio decoder
            this.audioDecoder = audioDecoder;
            audioDecoder.OpenDecoder(22050);
        }

        /// <summary>
        /// Writes IQ samples for processing.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="count"></param>
        public void Process(Complex* samples, int count)
        {
            basebandDecoder.Process(samples, count);
        }

        /// <summary>
        /// Handles audio frames
        /// </summary>
        /// <param name="decoder"></param>
        /// <param name="frame"></param>
        private void Decoder_OnPduFrame(Nrsc5Layer1Decoder decoder, FramePdu frame)
        {
            //Get the event for this program
            HDRadioDecoderAudioEventArgs evt = programEvents[frame.program];
            if (evt == null)
                return; //No listeners for this event, so no purpose decoding it

            //Process this
            int processed;
            fixed (byte* dataPtr = frame.payload)
                processed = audioDecoder.Process(dataPtr, frame.payload.Length, audioDecoderBufferPtr, AUDIO_DECODER_BUFFER_SIZE);

            //Marshal into a managed array
            short[] pcm = new short[processed];
            fixed (short* pcmPtr = pcm)
                Utils.Memcpy(pcmPtr, audioDecoderBuffer, processed * sizeof(short));

            //Dispatch
            evt?.Invoke(this, frame.program, pcm, processed);
        }

        private void Decoder_OnAasFrame(Nrsc5Layer1Decoder decoder, FrameAas frame)
        {
            
        }

        private void Decoder_OnPidsFrame(Nrsc5Layer1Decoder decoder, FramePids frame)
        {
            
        }
    }
}
