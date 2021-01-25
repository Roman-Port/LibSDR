using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO
{
    /// <summary>
    /// A kind of decimator, but we take only the last sample in a user-defined block size
    /// </summary>
    public class Keep1InBlock<T> where T : unmanaged
    {
        public Keep1InBlock(int blockSize)
        {
            this.blockSize = blockSize;
            if (blockSize <= 0)
                throw new ArgumentException("Block size must be >0.");
        }

        private int blockSize;
        private int index;

        public unsafe int Process(T* buf, int count)
        {
            return Process(buf, buf, count);
        }

        public unsafe int Process(T* input, T* output, int count)
        {
            int written = 0;
            while(count > 0)
            {
                index++;
                if(index == blockSize)
                {
                    output[0] = input[0];
                    output++;
                    written++;
                    index = 0;
                }
                input++;
                count--;
            }
            return written;
        }
    }
}
