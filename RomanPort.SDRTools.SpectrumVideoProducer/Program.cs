using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.FFT;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Sources;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;

namespace RomanPort.SDRTools.SpectrumVideoProducer
{
    unsafe class Program
    {
        public static int samplesPerFrame = 1800;
        public static int fftBins = 2048;
        public static int iqSampleRate;
        public static int bufferSize { get { return samplesPerFrame; } }
        public static int imgSpectrumHeight = 300;
        public static int imgWaterfallHeight = 800;
        public static float minDb = -80;
        public static float offsetDb = 0;

        public static UnsafeBuffer buffer;
        public static Complex* bufferPtr;

        public static UnsafeBuffer fftBuffer;
        public static float* fftBufferPtr;
        public static int[] fftPixelBuffer;

        public static WbFmDemodulator demodulator;
        public static FloatArbResampler audioResamplerL;
        public static FloatArbResampler audioResamplerR;
        public static byte[] audioFinalBuffer;
        public static UnsafeBuffer audioBufferA;
        public static float* audioBufferAPtr;
        public static UnsafeBuffer audioBufferB;
        public static float* audioBufferBPtr;
        public static UnsafeBuffer audioBufferC;
        public static float* audioBufferCPtr;

        public static byte[] img;
        public static int imgWidth;
        public static int imgHeight;

        public static byte[][] spectrumGradient;
        public static byte[][] spectrumGradientHalf;
        public static float spectrumScale;

        public static int[] horizontalLines;
        public static int[] verticalLines;

        public static FileStream sourceFile;
        public static WavStreamSource source;

        public static ComplexFftView fft;
        public static Process encoder;
        public static NamedPipeServerStream videoPipe;
        public static NamedPipeServerStream audioPipe;

        public static readonly float[] SPECTRUM_TOP_COLOR = new float[] { 112, 180, 255 };
        public static readonly float[] SPECTRUM_BOTTOM_COLOR = new float[] { 0, 0, 80 };

        public static readonly float[][] WATERFALL_COLORS = new float[][]
        {
            new float[] {0, 0, 32},
            new float[] {0, 0, 48},
            new float[] {0, 0, 80},
            new float[] {0, 0, 145},
            new float[] {30, 144, 255},
            new float[] {255, 255, 255},
            new float[] {255, 255, 0},
            new float[] {254, 109, 22},
            new float[] {255, 0, 0},
            new float[] {198, 0, 0},
            new float[] {159, 0, 0},
            new float[] {117, 0, 0},
            new float[] {74, 0, 0}
        };

        /// <summary>
        /// Produces a video file based upon an IQ file. Requires FFMPEG.
        /// </summary>
        /// <param name="input">The input IQ file.</param>
        /// <param name="output">The output MP4 file.</param>
        /// <param name="spectrumHeight">The height of the spectrum, in pixels.</param>
        /// <param name="waterfallHeight">The height of the waterfall, in pixels.</param>
        /// <param name="minDb">Minimum DB to display.</param>
        /// <param name="offsetDb">DB to offset by.</param>
        /// <param name="fftBins">The number of FFT bins. Must be a multiple of 1024.</param>
        /// <param name="averaging">Amount of averaging to use. 30 is recommended for 2048 fftBins, while 60 is recommended for 1024</param>
        static void Main(string input, string output, int spectrumHeight, int waterfallHeight, float offsetDb, float minDb, int fftBins, int averaging)
        {
            //Set
            Program.fftBins = fftBins;
            Program.offsetDb = offsetDb;
            Program.minDb = minDb;
            Program.imgWaterfallHeight = waterfallHeight;
            Program.imgSpectrumHeight = spectrumHeight;

            //Open WAV file as a source
            sourceFile = new FileStream(input, FileMode.Open);
            source = new WavStreamSource(sourceFile, false, 0);
            iqSampleRate = (int)source.Open(bufferSize);

            //Calculate
            samplesPerFrame = (int)((1f / 30) * iqSampleRate);
            source.Open(bufferSize);

            //Create FFT
            fft = new ComplexFftView(fftBins, averaging);

            //Create buffer
            buffer = UnsafeBuffer.Create(bufferSize, sizeof(Complex));
            bufferPtr = (Complex*)buffer;

            //Create FFT buffer
            fftBuffer = UnsafeBuffer.Create(fftBins, sizeof(float));
            fftBufferPtr = (float*)fftBuffer;
            fftPixelBuffer = new int[fftBins + 2];

            //Create demodulator
            demodulator = new WbFmDemodulator();
            demodulator.StereoEnabled = false;
            demodulator.OnAttached(bufferSize);
            demodulator.OnInputSampleRateChanged(iqSampleRate);
            audioResamplerL = new FloatArbResampler(iqSampleRate, 48000, 1, 0);
            audioResamplerR = new FloatArbResampler(iqSampleRate, 48000, 1, 0);
            audioFinalBuffer = new byte[bufferSize * 2 * sizeof(float)];

            //Create audio buffers
            audioBufferA = UnsafeBuffer.Create(bufferSize, sizeof(float));
            audioBufferB = UnsafeBuffer.Create(bufferSize, sizeof(float));
            audioBufferC = UnsafeBuffer.Create(bufferSize, sizeof(float));
            audioBufferAPtr = (float*)audioBufferA;
            audioBufferBPtr = (float*)audioBufferB;
            audioBufferCPtr = (float*)audioBufferC;

            //Allocate space for the image
            imgWidth = fftBins;
            imgHeight = (imgSpectrumHeight + imgWaterfallHeight);
            img = new byte[imgWidth * imgHeight * 4];

            //Precompute spectrum gradient
            spectrumGradient = new byte[imgSpectrumHeight][];
            spectrumGradientHalf = new byte[imgSpectrumHeight][];
            for(int i = 0; i<imgSpectrumHeight; i++)
            {
                float scale = i * (1 / (float)imgSpectrumHeight);
                spectrumGradient[i] = new byte[4];
                spectrumGradient[i][0] = byte.MaxValue;
                spectrumGradientHalf[i] = new byte[4];
                spectrumGradientHalf[i][0] = byte.MaxValue;
                for (int c = 0; c < 3; c++)
                {
                    spectrumGradient[i][c + 1] = (byte)(((1 - scale) * SPECTRUM_TOP_COLOR[c]) + (scale * SPECTRUM_BOTTOM_COLOR[c]));
                    spectrumGradientHalf[i][c + 1] = (byte)(spectrumGradient[i][c + 1] / 3);
                }
            }
            spectrumScale = imgSpectrumHeight / minDb;

            //Precompute the horizontal lines. These are drawn every 10 dB
            float pixelsPerDb = imgSpectrumHeight / -minDb;
            horizontalLines = new int[((int)-minDb / 10) - 1];
            for (int i = 0; i < horizontalLines.Length; i++)
                horizontalLines[i] = (int)((i + 1) * 10 * pixelsPerDb);

            //Precompute the vertical lines. These are drawn every 50 kHz from the center of the screen
            float pixelsPerCycle = imgWidth / (float)iqSampleRate;
            int centerFreq = iqSampleRate / 2;
            int lineCountSide = centerFreq / 50000;
            verticalLines = new int[lineCountSide * 2];
            for (int i = 0; i<lineCountSide; i++)
            {
                verticalLines[(i * 2) + 0] = (int)((centerFreq + (50000 * i)) * pixelsPerCycle);
                verticalLines[(i * 2) + 1] = (int)((centerFreq - (50000 * i)) * pixelsPerCycle);
            }

            //Create pipes
            string pipePrefix = "SpectrumVideoProducerPipe" + Process.GetCurrentProcess().Id;
            string videoPipeName = pipePrefix + "Video";
            string audioPipeName = pipePrefix + "Audio";
            videoPipe = new NamedPipeServerStream(videoPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 10000, 10000);
            audioPipe = new NamedPipeServerStream(audioPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 10000, 10000);

            //Start FFMPEG
            encoder = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-y -f rawvideo -pix_fmt argb -s {imgWidth}x{imgHeight} -r 30 -i \\\\.\\pipe\\{videoPipeName} -f f32le -ar 48000 -ac 2 -i \\\\.\\pipe\\{audioPipeName} " + output,
                RedirectStandardInput = true,
                UseShellExecute = false
            });

            //Loop
            while (MainLoop() != 0) ;
        }

        static int MainLoop()
        {
            //Read
            int read = source.Read(bufferPtr, bufferSize);

            //Add to FFT
            int offset = 0;
            do
            {
                fft.ProcessSamples(bufferPtr + offset);
                offset += fftBins;
            } while (offset + fftBins < read);

            //Calculate FFT
            fft.GetFFTSnapshot(fftBufferPtr);
            for (int i = 0; i < fftBins; i++)
                fftBufferPtr[i] = Math.Min(0, fftBufferPtr[i] + offsetDb);

            //Offset waterfall pixels
            OffsetWaterfall();

            //Render waterfall
            RenderWaterfall();

            //Render spectrum
            RenderSpectrum();
            RenderSpectrumDiagram();

            //Write video
            if (!videoPipe.IsConnected)
                videoPipe.WaitForConnection();
            videoPipe.WriteAsync(img, 0, img.Length);
            videoPipe.Flush();

            //Process audio
            HandleAudio(read);

            return read;
        }

        static void HandleAudio(int read)
        {
            //Demodulate
            int audioRead = demodulator.DemodulateStereo(bufferPtr, audioBufferAPtr, audioBufferBPtr, read);

            //Resample A into C
            int resampledAudioRead = audioResamplerL.Process(audioBufferAPtr, audioRead, audioBufferCPtr, bufferSize, false);

            //Resample B into A
            audioResamplerR.Process(audioBufferBPtr, audioRead, audioBufferAPtr, bufferSize, false);

            //Now, zipper this into the final buffer
            fixed(byte* audioFinalBufferPtr = audioFinalBuffer)
            {
                float* audioFinalBufferPtrFloat = (float*)audioFinalBufferPtr;
                for (int i = 0; i<resampledAudioRead; i++)
                {
                    audioFinalBufferPtrFloat[(i * 2) + 0] = audioBufferCPtr[i];
                    audioFinalBufferPtrFloat[(i * 2) + 1] = audioBufferAPtr[i];
                }
            }

            //Write
            if(!audioPipe.IsConnected)
                audioPipe.WaitForConnection();
            audioPipe.WriteAsync(audioFinalBuffer, 0, resampledAudioRead * 2 * sizeof(float));
            audioPipe.Flush();
        }

        static void RenderSpectrumDiagram()
        {
            //Render horizontal lines
            for(int i = 0; i<horizontalLines.Length; i++)
            {
                int offS = (imgWidth * horizontalLines[i]) * 4;
                for (int x = 0; x < imgWidth * 4; x++)
                    img[offS + x] += 40;
            }

            //Render vertical lines, skipping lines where we've already drawn
            for (int y = 0; y < imgSpectrumHeight; y++)
            {
                bool alreadyWritten = false;
                for (int i = 0; i < horizontalLines.Length; i++)
                    alreadyWritten = alreadyWritten || horizontalLines[i] == y;
                if(!alreadyWritten)
                {
                    for (int i = 1; i < verticalLines.Length; i++)
                    {
                        int offS = GetXYIndex(verticalLines[i], y);
                        for (int c = 0; c < 4; c++)
                            img[offS + c] += 40;
                    }
                }
            }

            //Render the middle line as solid red. We can cheat a bit with this
            for (int y = 0; y < imgSpectrumHeight; y++)
            {
                for (int i = 0; i < 2; i++)
                {
                    int offS = GetXYIndex(verticalLines[i], y);
                    img[offS + 0] = byte.MaxValue;
                    img[offS + 1] = byte.MaxValue;
                    img[offS + 2] = byte.MinValue;
                    img[offS + 3] = byte.MinValue;
                }
            }
        }

        static void OffsetWaterfall()
        {
            fixed (byte* imgPtr = img)
            {
                for (int y = imgWaterfallHeight - 2; y >= 0; y--)
                {
                    Utils.Memcpy(imgPtr + ((imgSpectrumHeight + y) * imgWidth * 4), imgPtr + ((imgSpectrumHeight + y - 1) * imgWidth * 4), 4 * imgWidth);
                }
            }
        }

        static void RenderWaterfall()
        {
            for (int x = 0; x < fftBins; x++)
            {
                GetColor(fftBufferPtr[x] / minDb, GetXYIndex(x, imgSpectrumHeight));
            }
        }

        static void RenderSpectrum()
        {
            //Crunch
            for(int i = 0; i<fftBins; i++)
            {
                fftPixelBuffer[i + 1] = (int)((fftBufferPtr[i] * spectrumScale) + 0.5f);
            }

            //Write first and last element to the space we allocated for over/underflows. This just lets us save some CPU cycles later
            fftPixelBuffer[0] = fftPixelBuffer[1];
            fftPixelBuffer[fftPixelBuffer.Length - 1] = fftPixelBuffer[fftPixelBuffer.Length - 2];
            
            //Render
            for (int x = 0; x < fftBins; x++)
            {
                //Get points
                int pointMax = Math.Max(Math.Max(fftPixelBuffer[x], fftPixelBuffer[x + 2]), fftPixelBuffer[x + 1]);
                int pointMin = Math.Min(Math.Min(fftPixelBuffer[x], fftPixelBuffer[x + 2]), fftPixelBuffer[x + 1]);

                //Compute
                for (int y = 0; y < imgSpectrumHeight; y++)
                {
                    int offS = GetXYIndex(x, y);
                    if (y > pointMin && y < pointMax)
                    {
                        for (int c = 0; c < 4; c++)
                            img[offS + c] = byte.MaxValue;
                    }
                    else if (fftPixelBuffer[x + 1] > y)
                    {
                        for (int c = 0; c < 4; c++)
                            img[offS + c] = spectrumGradientHalf[y][c];
                    }
                    else if (fftPixelBuffer[x + 1] < y)
                    {
                        for (int c = 0; c < 4; c++)
                            img[offS + c] = spectrumGradient[y][c];
                    }
                }
            }
        }

        static int GetXYIndex(int x, int y)
        {
            return (x + (y * imgWidth)) * 4;
        }

        static void GetColor(float percent, int index)
        {
            //Make sure percent is within range
            percent = 1 - percent;
            percent = Math.Max(0, percent);
            percent = Math.Min(1, percent);

            //Calculate
            var scale = WATERFALL_COLORS.Length - 1;

            //Get the two colors to mix
            var mix2 = WATERFALL_COLORS[(int)Math.Floor(percent * scale)];
            var mix1 = WATERFALL_COLORS[(int)Math.Ceiling(percent * scale)];
            if (mix2 == null || mix1 == null)
            {
                throw new Exception("Invalid color!");
            }

            //Get ratio
            var ratio = (percent * scale) - Math.Floor(percent * scale);

            //Mix
            img[index + 0] = byte.MaxValue;
            img[index + 1] = (byte)(Math.Ceiling((mix1[0] * ratio) + (mix2[0] * (1 - ratio))));
            img[index + 2] = (byte)(Math.Ceiling((mix1[1] * ratio) + (mix2[1] * (1 - ratio))));
            img[index + 3] = (byte)(Math.Ceiling((mix1[2] * ratio) + (mix2[2] * (1 - ratio))));
        }
    }
}
