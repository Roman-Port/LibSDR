using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS
{
    public class RDSBitDecoder
    {
        public RDSBitDecoder()
        {

        }

		public event RDSFrameDecoded OnFrameDecoded;
		public event RDSSyncStateChanged OnSyncStateChanged;

		public bool IsSynced
		{
			get => isSynced;
			private set
			{
				//Reset respective values depending on the state
				if(value)
                {
					badBlocks = 0;
					blocks = 0;
					blockBits = 0;
					groupAssemblyRunning = false;
				} else
                {
					presync = false;
				}
				
				//Update value and dispatch event
				isSynced = value;
				OnSyncStateChanged?.Invoke(value);
			}
		}

		private bool isSynced;
		private long bits;
		private long presyncOffsetBits;
		private long bitsBuffer;
		private int blockBits;
		private int badBlocks;
		private int blocks;
		private int goodBlocks;
		private int[] group = new int[4];
		private bool presync;
		private bool groupAssemblyRunning;
		private int lastOffset;
		private int blockIndex;

		private readonly static int[] OFFSET_POS = { 0, 1, 2, 3, 2 };
		private readonly static int[] OFFSET_WORD = { 252, 408, 360, 436, 848 };
		private readonly static int[] SYNDROME = { 383, 14, 303, 663, 748 };

		public void ProcessBit(byte bit)
		{
			//Push bit to the buffer
			bitsBuffer = (bitsBuffer << 1) | bit;

			//Attempt to sync
			if (isSynced)
				ProcessSynced();
			else
				ProcessUnsynced();

			//Update counter
			bits++;
		}

		private void ProcessSynced()
        {
			/* wait until 26 bits enter the buffer */
			if (blockBits < 25)
			{
				blockBits++;
			}
			else
			{
				//Calculate word
				int dataword = (int)((bitsBuffer >> 10) & 0xffff);

				//Check CRC
				bool crcOk = CheckBlockCrc(dataword);
				if (!crcOk)
					badBlocks++;

				//If this was decoded OK and this is the first block, we know to begin assembly
				if (blockIndex == 0 && crcOk)
				{
					groupAssemblyRunning = true;
					goodBlocks = 1;
				}

				//Begin assembling group
				if (groupAssemblyRunning)
				{
					//Read in parts
					if (!crcOk)
					{
						groupAssemblyRunning = false;
					}
					else
					{
						group[blockIndex] = dataword;
						goodBlocks++;
					}

					//If we get all blocks successfully, submit the frame
					if (goodBlocks == 5)
                    {
						RDSFrame frame = new RDSFrame
						{
							a = (ushort)group[0],
							b = (ushort)group[1],
							c = (ushort)group[2],
							d = (ushort)group[3]
						};
						OnFrameDecoded?.Invoke(frame);
					}
				}

				//Update state
				blockBits = 0;
				blockIndex = (blockIndex + 1) % 4;
				blocks++;
				
				//Reset state if needed
				if (blocks == 50)
				{
					if (badBlocks > 35)
						IsSynced = false;
					blocks = 0;
					badBlocks = 0;
				}
			}
		}

		private void ProcessUnsynced()
        {
			//Calculate
			int decodedSyndrome = CalculateSyndrome(bitsBuffer, 26);

			//Try each group so we can detect which one we're looking at
			for (int j = 0; j < 5; j++)
			{
				//Check if it matches
				if (decodedSyndrome == SYNDROME[j])
				{
					//Matches! We now know what block we are located on
					if (!presync)
					{
						//Enter presync
						lastOffset = j;
						presyncOffsetBits = bits;
						presync = true;
					}
					else
					{
						//Calculate bit distance
						long bitDistance = bits - presyncOffsetBits;

						//Calculate block distance
						int blockDistance;
						if (OFFSET_POS[lastOffset] >= OFFSET_POS[j])
							blockDistance = OFFSET_POS[j] + 4 - OFFSET_POS[lastOffset];
						else
							blockDistance = OFFSET_POS[j] - OFFSET_POS[lastOffset];

						//Detect sync
						if ((blockDistance * 26) != bitDistance)
						{
							presync = false;
						}
						else
						{
							//We are now synced!
							blockIndex = (j + 1) % 4;
							IsSynced = true;
						}
					}
					break;
				}
			}
		}

		private bool CheckBlockCrc(int dataword)
        {
			//Calculate
			int calculatedCrc = CalculateSyndrome(dataword, 16);
			long checkword = (int)(bitsBuffer & 0x3ff);

			//Check
			if (blockIndex == 2)
			{
				long actualCrc = checkword ^ OFFSET_WORD[blockIndex];
				if (actualCrc == calculatedCrc)
				{
					return true;
				}
				else
				{
					actualCrc = checkword ^ OFFSET_WORD[4];
					return actualCrc == calculatedCrc;
				}
			}
			else
			{
				long actualCrc = checkword ^ OFFSET_WORD[blockIndex];
				return actualCrc == calculatedCrc;
			}
		}

		private int CalculateSyndrome(long block, int length)
		{
			long x = 0;
			for (int i = length; i > 0; i--)
			{
				x = (x << 1) | ((block >> (i - 1)) & 0x01);
				if ((x & (1 << 10)) != 0)
					x = x ^ 0x5B9;
			}
			for (int i = 10; i > 0; i--)
			{
				x = x << 1;
				if ((x & (1 << 10)) != 0)
					x = x ^ 0x5B9;
			}
			return (int)(x & ((1 << 10) - 1));
		}
	}
}
