using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.USB
{
    public unsafe delegate void IUsbAsyncTransfer_TransferCompleted(IUsbAsyncTransfer transfer, byte* bufferPtr, int count);
    public unsafe delegate void IUsbAsyncTransfer_TransferFailed(IUsbAsyncTransfer transfer);
    
    public interface IUsbAsyncTransfer
    {
        event IUsbAsyncTransfer_TransferCompleted OnTransferCompleted;
        event IUsbAsyncTransfer_TransferFailed OnTransferFailed;
        void SubmitTransfer();
    }
}
