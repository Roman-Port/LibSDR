using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy
{
    public static class NativeMethods
    {
        private const string LibAirSpy = "airspy";

        [DllImport("airspy", EntryPoint = "airspy_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_init();

        [DllImport("airspy", EntryPoint = "airspy_list_devices", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int airspy_list_devices(ulong* serials, int count);

        [DllImport("airspy", EntryPoint = "airspy_board_partid_serialno_read", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe airspy_error airspy_board_partid_serialno_read(
          IntPtr dev,
          uint* read_partid_serialno);

        [DllImport("airspy", EntryPoint = "airspy_exit", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_exit();

        [DllImport("airspy", EntryPoint = "airspy_open", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_open(out IntPtr dev);

        [DllImport("airspy", EntryPoint = "airspy_open_sn", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_open_sn(out IntPtr dev, ulong serial);

        [DllImport("airspy", EntryPoint = "airspy_close", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_close(IntPtr dev);

        [DllImport("airspy", EntryPoint = "airspy_set_samplerate", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_samplerate(IntPtr dev, uint samplerate);

        [DllImport("airspy", EntryPoint = "airspy_set_conversion_filter_float32", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe airspy_error airspy_set_conversion_filter_float32(
          IntPtr dev,
          float* kernel,
          int len);

        [DllImport("airspy", EntryPoint = "airspy_set_conversion_filter_int16", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe airspy_error airspy_set_conversion_filter_int16(
          IntPtr dev,
          short* kernel,
          int len);

        [DllImport("airspy", EntryPoint = "airspy_get_samplerates", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe airspy_error airspy_get_samplerates(
          IntPtr dev,
          uint* buffer,
          uint len);

        [DllImport("airspy", EntryPoint = "airspy_start_rx", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_start_rx(
          IntPtr dev,
          airspy_sample_block_cb_fn cb,
          IntPtr rx_ctx);

        [DllImport("airspy", EntryPoint = "airspy_stop_rx", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_stop_rx(IntPtr dev);

        [DllImport("airspy", EntryPoint = "airspy_is_streaming", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_is_streaming(IntPtr dev);

        [DllImport("airspy", EntryPoint = "airspy_board_id_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr airspy_board_id_name_native(uint index);

        [DllImport("airspy", EntryPoint = "airspy_set_sample_type", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_sample_type(
          IntPtr dev,
          airspy_sample_type sample_type);

        [DllImport("airspy", EntryPoint = "airspy_set_freq", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_freq(IntPtr dev, uint freq_hz);

        [DllImport("airspy", EntryPoint = "airspy_set_packing", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_packing(IntPtr dev, [MarshalAs(UnmanagedType.U1)] bool value);

        [DllImport("airspy", EntryPoint = "airspy_set_lna_gain", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_lna_gain(IntPtr dev, byte value);

        [DllImport("airspy", EntryPoint = "airspy_set_mixer_gain", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_mixer_gain(IntPtr dev, byte value);

        [DllImport("airspy", EntryPoint = "airspy_set_vga_gain", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_vga_gain(IntPtr dev, byte value);

        [DllImport("airspy", EntryPoint = "airspy_set_lna_agc", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_lna_agc(IntPtr dev, [MarshalAs(UnmanagedType.U1)] bool value);

        [DllImport("airspy", EntryPoint = "airspy_set_mixer_agc", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_mixer_agc(IntPtr dev, [MarshalAs(UnmanagedType.U1)] bool value);

        [DllImport("airspy", EntryPoint = "airspy_set_linearity_gain", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_linearity_gain(IntPtr dev, byte value);

        [DllImport("airspy", EntryPoint = "airspy_set_sensitivity_gain", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_sensitivity_gain(
          IntPtr dev,
          byte value);

        [DllImport("airspy", EntryPoint = "airspy_r820t_write", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_r820t_write(
          IntPtr device,
          byte register_number,
          byte value);

        public static airspy_error airspy_r820t_write_mask(
          IntPtr device,
          byte reg,
          byte value,
          byte mask)
        {
            byte num;
            airspy_error airspyError = NativeMethods.airspy_r820t_read(device, reg, out num);
            if (airspyError < airspy_error.AIRSPY_SUCCESS)
                return airspyError;
            value = (byte)((int)num & (int)~mask | (int)value & (int)mask);
            return NativeMethods.airspy_r820t_write(device, reg, value);
        }

        [DllImport("airspy", EntryPoint = "airspy_r820t_read", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_r820t_read(
          IntPtr device,
          byte register_number,
          out byte value);

        [DllImport("airspy", EntryPoint = "airspy_si5351c_write", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_si5351c_write(
          IntPtr device,
          byte register_number,
          byte value);

        [DllImport("airspy", EntryPoint = "airspy_si5351c_read", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_si5351c_read(
          IntPtr device,
          byte register_number,
          out byte value);

        [DllImport("airspy", EntryPoint = "airspy_set_rf_bias", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_set_rf_bias(IntPtr dev, [MarshalAs(UnmanagedType.U1), In] bool value);

        [DllImport("airspy", EntryPoint = "airspy_spiflash_erase", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_spiflash_erase(IntPtr device);

        [DllImport("airspy", EntryPoint = "airspy_spiflash_write", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe airspy_error airspy_spiflash_write(
          IntPtr device,
          uint address,
          ushort length,
          byte* data);

        [DllImport("airspy", EntryPoint = "airspy_spiflash_read", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe airspy_error airspy_spiflash_read(
          IntPtr device,
          uint address,
          ushort length,
          byte* data);

        [DllImport("airspy", EntryPoint = "airspy_spiflash_erase_sector", CallingConvention = CallingConvention.Cdecl)]
        public static extern airspy_error airspy_spiflash_erase_sector(
          IntPtr device,
          ushort sector_num);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int airspy_sample_block_cb_fn(airspy_transfer* ptr);
}
