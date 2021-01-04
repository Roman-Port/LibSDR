using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy.Internal
{
    public enum airspy_error
    {
        AIRSPY_ERROR_OTHER = -9999, // 0xFFFFD8F1
        AIRSPY_ERROR_STREAMING_STOPPED = -1003, // 0xFFFFFC15
        AIRSPY_ERROR_STREAMING_THREAD_ERR = -1002, // 0xFFFFFC16
        AIRSPY_ERROR_THREAD = -1001, // 0xFFFFFC17
        AIRSPY_ERROR_LIBUSB = -1000, // 0xFFFFFC18
        AIRSPY_ERROR_NO_MEM = -11, // 0xFFFFFFF5
        AIRSPY_ERROR_BUSY = -6, // 0xFFFFFFFA
        AIRSPY_ERROR_NOT_FOUND = -5, // 0xFFFFFFFB
        AIRSPY_ERROR_INVALID_PARAM = -2, // 0xFFFFFFFE
        AIRSPY_SUCCESS = 0,
        AIRSPY_TRUE = 1,
    }
}
