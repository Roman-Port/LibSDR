using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.Framework.Multithreading
{
    /// <summary>
    /// Serves as an interface for multithreaded applications to run work on an additional thread
    /// </summary>
    public class MultithreadWorker
    {
        private Thread worker;
        private MultithreadRequestDelegate waitingCommand;
        private volatile bool waitingCommandFinished;
        private object waitingCommandResult;

        public MultithreadWorker()
        {
            worker = new Thread(RunWorkerThread);
            worker.IsBackground = true;
            worker.Name = "MultithreadWorker Worker Thread";
            worker.Start();
        }
        
        public void BeginWork(MultithreadRequestDelegate command)
        {
            if (waitingCommand != null)
                throw new Exception("There is already a command being processed.");
            waitingCommandFinished = false;
            waitingCommandResult = null;
            waitingCommand = command;
        }

        public object EndWork()
        {
            while (!waitingCommandFinished) ;
            waitingCommandFinished = false;
            return waitingCommandResult;
        }

        private void RunWorkerThread()
        {
            while(true)
            {
                while (waitingCommand == null) ;
                waitingCommandResult = waitingCommand();
                waitingCommand = null;
                waitingCommandFinished = true;
            }
        }
    }
}
