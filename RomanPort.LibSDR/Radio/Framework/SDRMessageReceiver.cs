using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Framework
{
    /// <summary>
    /// A base class for threaded messages to be exchanged
    /// </summary>
    public abstract class SDRMessageReceiver
    {
        private ConcurrentQueue<ISDRMessage> messageQueue = new ConcurrentQueue<ISDRMessage>();

        /// <summary>
        /// A public interface for adding the messages
        /// </summary>
        /// <param name="msg"></param>
        protected void SendThreadedMessage(ISDRMessage msg)
        {
            messageQueue.Enqueue(msg);
        }

        /// <summary>
        /// Call this when it's safe to do so from the worker thread.
        /// </summary>
        protected void HandleQueuedMessages()
        {
            while (messageQueue.TryDequeue(out ISDRMessage msg))
                msg.Process();
        }
    }
}
