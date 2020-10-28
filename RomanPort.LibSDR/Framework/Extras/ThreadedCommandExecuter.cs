using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Framework.Extras
{
    public abstract class ThreadedCommandExecuter<CommandType, ReturnType>
    {
        private Thread worker;
        private bool working;
        private ConcurrentQueue<CommandType> incomingQueue;
        private ConcurrentQueue<ReturnType> outgoingQueue;

        public ThreadedCommandExecuter()
        {
            incomingQueue = new ConcurrentQueue<CommandType>();
            outgoingQueue = new ConcurrentQueue<ReturnType>();
            working = true;
            worker = new Thread(WorkerThread);
            worker.Name = "ThreadedCommandExecuter Worker";
            worker.IsBackground = true;
            worker.Start();
        }

        public void Stop()
        {
            working = false;
        }

        public void StartExecute(CommandType cmd)
        {
            incomingQueue.Enqueue(cmd);
        }

        public ReturnType EndExecute()
        {
            ReturnType c;
            while (!outgoingQueue.TryDequeue(out c)) ;
            return c;
        }

        private void WorkerThread()
        {
            CommandType cmd;
            while(true)
            {
                while (!incomingQueue.TryDequeue(out cmd) && working) ;
                if (!working)
                    return;
                ReturnType r = Compute(cmd);
                outgoingQueue.Enqueue(r);
            }
        }

        public abstract ReturnType Compute(CommandType cmd);
    }
}
