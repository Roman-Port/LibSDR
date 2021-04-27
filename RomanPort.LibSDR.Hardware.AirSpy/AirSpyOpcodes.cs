using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Hardware.AirSpy
{
    internal enum AirSpyOpcodes : byte
    {
        AIRSPY_INVALID = 0,
        AIRSPY_RECEIVER_MODE = 1,
        AIRSPY_SI5351C_WRITE = 2,
        AIRSPY_SI5351C_READ = 3,
        AIRSPY_R820T_WRITE = 4,
        AIRSPY_R820T_READ = 5,
        AIRSPY_SPIFLASH_ERASE = 6,
        AIRSPY_SPIFLASH_WRITE = 7,
        AIRSPY_SPIFLASH_READ = 8,
        AIRSPY_BOARD_ID_READ = 9,
        AIRSPY_VERSION_STRING_READ = 10,
        AIRSPY_BOARD_PARTID_SERIALNO_READ = 11,
        AIRSPY_SET_SAMPLERATE = 12,
        AIRSPY_SET_FREQ = 13,
        AIRSPY_SET_LNA_GAIN = 14,
        AIRSPY_SET_MIXER_GAIN = 15,
        AIRSPY_SET_VGA_GAIN = 16,
        AIRSPY_SET_LNA_AGC = 17,
        AIRSPY_SET_MIXER_AGC = 18,
        AIRSPY_MS_VENDOR_CMD = 19,
        AIRSPY_SET_RF_BIAS_CMD = 20,
        AIRSPY_GPIO_WRITE = 21,
        AIRSPY_GPIO_READ = 22,
        AIRSPY_GPIODIR_WRITE = 23,
        AIRSPY_GPIODIR_READ = 24,
        AIRSPY_GET_SAMPLERATES = 25,
        AIRSPY_SET_PACKING = 26
    }
}
