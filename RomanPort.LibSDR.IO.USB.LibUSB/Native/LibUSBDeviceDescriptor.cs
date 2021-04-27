using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB.Native
{
    struct LibUSBDeviceDescriptor
    {
		/** Size of this descriptor (in bytes) */
		public byte bLength;

		/** Descriptor type. Will have value
		 * \ref libusb_descriptor_type::LIBUSB_DT_DEVICE LIBUSB_DT_DEVICE in this
		 * context. */
		public byte bDescriptorType;

		/** USB specification release number in binary-coded decimal. A value of
		 * 0x0200 indicates USB 2.0, 0x0110 indicates USB 1.1, etc. */
		public ushort bcdUSB;

		/** USB-IF class code for the device. See \ref libusb_class_code. */
		public byte bDeviceClass;

		/** USB-IF subclass code for the device, qualified by the bDeviceClass
		 * value */
		public byte bDeviceSubClass;

		/** USB-IF protocol code for the device, qualified by the bDeviceClass and
		 * bDeviceSubClass values */
		public byte bDeviceProtocol;

		/** Maximum packet size for endpoint 0 */
		public byte bMaxPacketSize0;

		/** USB-IF vendor ID */
		public ushort idVendor;

		/** USB-IF product ID */
		public ushort idProduct;

		/** Device release number in binary-coded decimal */
		public ushort bcdDevice;

		/** Index of string descriptor describing manufacturer */
		public byte iManufacturer;

		/** Index of string descriptor describing product */
		public byte iProduct;

		/** Index of string descriptor containing device serial number */
		public byte iSerialNumber;

		/** Number of possible configurations */
		public byte bNumConfigurations;
	}
}
