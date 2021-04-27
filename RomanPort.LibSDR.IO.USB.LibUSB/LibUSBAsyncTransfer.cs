using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.IO.USB;
using RomanPort.LibSDR.IO.USB.LibUSB.Native;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB
{
    public unsafe class LibUSBAsyncTransfer : IUsbAsyncTransfer
    {
        public LibUSBAsyncTransfer(IntPtr device, byte endpoint, int bufferSize)
        {
            //Configure
            this.device = device;
            this.bufferSize = bufferSize;

            //Create buffer
            buffer = UnsafeBuffer.Create(bufferSize, out bufferPtr);

            //Allocate transfer
            transfer = (LibUSBTransfer*)LibUSBNative.libusb_alloc_transfer(0);

            //Get the GCHandle for ourself
            handle = GCHandle.Alloc(this);

            //Populate transfer (same as libusb_fill_bulk_transfer)
            transfer->dev_handle = device;
            transfer->endpoint = endpoint;
            transfer->type = LibUSBTransferType.LIBUSB_TRANSFER_TYPE_BULK;
            transfer->timeout = 0;
            transfer->buffer = bufferPtr;
            transfer->length = bufferSize;
            transfer->user_data = (IntPtr)handle;
            transfer->callback = Marshal.GetFunctionPointerForDelegate(TransferCallbackDelegate);
        }

        static readonly LibUSBNative.libusb_transfer_cb_fn TransferCallbackDelegate = new LibUSBNative.libusb_transfer_cb_fn(TransferCallback);

        private static void TransferCallback(LibUSBTransfer* transfer)
        {
            //Get the async transfer object
            LibUSBAsyncTransfer ctx = (LibUSBAsyncTransfer)GCHandle.FromIntPtr(transfer->user_data).Target;

            //Send events
            if (transfer->status == LibUSBTransferStatus.LIBUSB_TRANSFER_COMPLETED)
                ctx.OnTransferCompleted?.Invoke(ctx, transfer->buffer, transfer->length);
            else
                ctx.OnTransferFailed?.Invoke(ctx);
        }

        public void SubmitTransfer()
        {
            LibUSBNative.ThrowIfError(LibUSBNative.libusb_submit_transfer(transfer));
        }

        private IntPtr device;
        private LibUSBTransfer* transfer;
        private GCHandle handle;

        private UnsafeBuffer buffer;
        private byte* bufferPtr;
        private int bufferSize;

        public event IUsbAsyncTransfer_TransferCompleted OnTransferCompleted;
        public event IUsbAsyncTransfer_TransferFailed OnTransferFailed;
    }
}
