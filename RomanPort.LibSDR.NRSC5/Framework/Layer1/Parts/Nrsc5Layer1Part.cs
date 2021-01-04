using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    class Nrsc5Layer1Part : CTranslationLayer
    {
        protected Acquire acquire;
        protected Sync sync;
        protected Decode decode;
        protected Pids pids;
        protected Frame frame;

        public void SetComponents(Acquire acquire, Sync sync, Decode decode, Pids pids, Frame frame)
        {
            this.acquire = acquire;
            this.sync = sync;
            this.decode = decode;
            this.pids = pids;
            this.frame = frame;
        }
    }
}
