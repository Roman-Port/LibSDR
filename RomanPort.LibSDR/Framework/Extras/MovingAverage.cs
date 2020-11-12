using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Extras
{
    public unsafe class MovingAverage : IDisposable
    {
        public readonly uint blockSize;
        public readonly uint blockCount;
        public readonly uint elementSize;

        private ulong totalBlockIndex;
        private UnsafeBuffer buffer;
        private float* bufferPtr;
        
        /// <summary>
        /// Block size is the number of T to store as a block, and blockCount is the number of those to keep. When getting the average, we go through blockCount blocks
        /// </summary>
        /// <param name="blockSize"></param>
        /// <param name="blockCount"></param>
        public MovingAverage(uint blockSize, uint blockCount)
        {
            this.blockSize = blockSize;
            this.blockCount = blockCount;
            elementSize = sizeof(float);

            //Create buffer
            buffer = UnsafeBuffer.Create((int)(blockSize * blockCount), (int)elementSize);
            bufferPtr = (float*)buffer;
        }

        /// <summary>
        /// Writes a block. Data must be the size of blockSize
        /// </summary>
        /// <param name="data"></param>
        public void WriteBlock(float* data)
        {
            int blockIndex = (int)(totalBlockIndex % blockCount);
            Utils.Memcpy(bufferPtr + (blockSize * blockIndex), data, (int)(blockSize * elementSize));
            totalBlockIndex++;
        }

        /// <summary>
        /// Reads blockSize into output with the average of the last blockCount blocks
        /// </summary>
        /// <param name="output"></param>
        public bool ReadAverage(float* output)
        {
            //Clear out output, as we use it as a temporary buffer
            for (int i = 0; i < blockSize; i++)
                output[i] = 0;
            
            //Total all blocks and also weigh them
            int readableBlocks = (int)Math.Min(totalBlockIndex, blockCount);
            for (int i = 0; i<readableBlocks; i++)
            {
                for (int j = 0; j < blockSize; j++)
                {
                    output[j] += bufferPtr[(i * blockSize) + j];
                }
            }

            //Divide all by the blocks read to get the final average
            for (int i = 0; i < blockSize; i++)
                output[i] /= readableBlocks;

            return readableBlocks > 0;
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
