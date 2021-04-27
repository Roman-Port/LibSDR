using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Components.General
{
    public delegate void BackgroundWorker_Task();

    public class BackgroundWorker : IDisposable
    {
        public BackgroundWorker(string label = "Unlabeled")
        {
            worker = new Thread(WorkerThread);
            worker.IsBackground = true;
            worker.Name = $"Background Worker \"{label}\"";
            worker.Start();
        }

        private Thread worker;
        private volatile bool isWorking;
        private volatile bool isExiting;
        private volatile AutoResetEvent inputEvent = new AutoResetEvent(false);
        private volatile AutoResetEvent outputEvent = new AutoResetEvent(false);
        private volatile BackgroundWorker_Task task;

        private void WorkerThread()
        {
            while(true)
            {
                //Wait for input task
                inputEvent.WaitOne();

                //Check if we should exit
                if (isExiting)
                    break;

                //Run
                task();

                //Set
                outputEvent.Set();
            }
        }

        /// <summary>
        /// Starts a process in the background worker
        /// </summary>
        /// <param name="task"></param>
        public void StartWork(BackgroundWorker_Task task)
        {
            //Validate
            if (isWorking)
                throw new Exception("Attempted to schedule work while the worker is already active! You'll need to make a new worker.");

            //Add
            isWorking = true;
            this.task = task;
            inputEvent.Set();
        }

        //Waits for the started process to finish
        public void EndWork()
        {
            //Validate
            if (!isWorking)
                throw new Exception("No work has been scheduled yet.");

            //Wait
            outputEvent.WaitOne();

            //Update
            isWorking = false;
        }

        public void Dispose()
        {
            isExiting = true;
            StartWork(null);
        }
    }
}
