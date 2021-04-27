using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB.Native
{
    enum LibUSBTransferType : byte
    {
		/** Control transfer */
		LIBUSB_TRANSFER_TYPE_CONTROL = 0,

		/** Isochronous transfer */
		LIBUSB_TRANSFER_TYPE_ISOCHRONOUS = 1,

		/** Bulk transfer */
		LIBUSB_TRANSFER_TYPE_BULK = 2,

		/** Interrupt transfer */
		LIBUSB_TRANSFER_TYPE_INTERRUPT = 3,

		/** Bulk stream transfer */
		LIBUSB_TRANSFER_TYPE_BULK_STREAM = 4
	}
}
