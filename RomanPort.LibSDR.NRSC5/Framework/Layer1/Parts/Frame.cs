using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    delegate void FrameAasEventArgs(FrameAas frame);
    delegate void FramePduEventArgs(FramePdu frame);

    unsafe class Frame : Nrsc5Layer1Part
    {
        public event FrameAasEventArgs OnAasFrame;
        public event FramePduEventArgs OnPduFrame;

        UnsafeBuffer bufferB;
        byte* buffer;
        UnsafeBuffer pduB;
        byte*[] pdu;
        /*unsigned*/ /*uint*/
        int[] pdu_idx = new int[MAX_PROGRAMS];
        /*unsigned*/ /*uint*/
        int pci;
        /*unsigned*/ /*uint*/
        int program;
        UnsafeBuffer psd_bufB;
        byte*[] psd_buf;
        UnsafeBuffer psd_idxB;
        int* psd_idx;

        /*unsigned*/ /*uint*/
        int sync_width;
        /*unsigned*/ /*uint*/
        int sync_count;
        UnsafeBuffer ccc_bufB;
        byte* ccc_buf;
        int ccc_idx;
        fixed_subchannel_t[] subchannel = new fixed_subchannel_t[4];
        int fixed_ready;
        void* rs_dec;

        public Frame()
        {
            bufferB = UnsafeBuffer.Create(MAX_PDU_LEN, out buffer);
            pduB = UnsafeBuffer.Create2D(MAX_PROGRAMS, 0x10000, out pdu);
            psd_bufB = UnsafeBuffer.Create2D(MAX_PROGRAMS, MAX_AAS_LEN, out psd_buf);
            psd_idxB = UnsafeBuffer.Create(MAX_PROGRAMS, out psd_idx);
            ccc_bufB = UnsafeBuffer.Create(CCM_BUF_LEN, out ccc_buf);
        }

        class fixed_subchannel_t
        {
            public ushort mode;
            public ushort length;
            public /*unsigned*/ /*uint*/ int block_idx;
            public UnsafeBuffer blocksB;
            public byte* blocks;
            public int idx;
            public UnsafeBuffer dataB;
            public byte* data;

            public fixed_subchannel_t()
            {
                blocksB = UnsafeBuffer.Create(255 + 4, out blocks);
                dataB = UnsafeBuffer.Create(MAX_AAS_LEN, out data);
            }
        }

        class frame_header_t
        {
            public /*unsigned*/ /*uint*/ int codec;
            public /*unsigned*/ /*uint*/ int stream_id;
            public /*unsigned*/ /*uint*/ int pdu_seq;
            public /*unsigned*/ /*uint*/ int blend_control;
            public /*unsigned*/ /*uint*/ int per_stream_delay;
            public /*unsigned*/ /*uint*/ int common_delay;
            public /*unsigned*/ /*uint*/ int latency;
            public /*unsigned*/ /*uint*/ bool pfirst;
            public /*unsigned*/ /*uint*/ bool plast;
            public /*unsigned*/ /*uint*/ int seq;
            public /*unsigned*/ /*uint*/ int nop;
            public /*unsigned*/ /*uint*/ int hef;
            public /*unsigned*/ /*uint*/ int la_location;
        }

        class hef_t
        {
            public /*unsigned*/ /*uint*/ int class_ind;
            public /*unsigned*/ /*uint*/ int prog_num;
            public /*unsigned*/ /*uint*/ int pdu_len;
            public /*unsigned*/ /*uint*/ int prog_type;
            public /*unsigned*/ /*uint*/ int access;
            public /*unsigned*/ /*uint*/ int applied_services;
            public /*unsigned*/ /*uint*/ int pdu_marker;
        }

        const int PCI_AUDIO = 0x38D8D3;
        const int PCI_AUDIO_FIXED = 0xE3634C;
        const int PCI_AUDIO_FIXED_OPP = 0x8D8D33;
        const int MAX_AUDIO_PACKETS = 64;
        const int MAX_AAS_LEN = 8212;
        const int RS_BLOCK_LEN = 255;
        const int RS_CODEWORD_LEN = 96;
        const int VALIDFCS16 = 0xf0b8;
        const int CCM_BUF_LEN = 32;

        byte crc8(byte* pkt, /*unsigned*/ /*uint*/ int cnt)
        {
            byte crc = 0xFF;
            for (int i = 0; i < cnt; ++i)
                crc = FrameConstants.crc8_tab[crc ^ pkt[i]];
            return crc;
        }

        ushort fcs16(byte* cp, int len)
        {
            ushort crc = 0xFFFF;
            while (len-- != 0)
                crc = (ushort)((crc >> 8) ^ FrameConstants.fcs_tab[(crc ^ *cp++) & 0xFF]);
            return (crc);
        }

        bool has_fixed()
        {
            return this.pci == PCI_AUDIO_FIXED || this.pci == PCI_AUDIO_FIXED_OPP;
        }

        int fix_header(byte* buf)
        {
            byte[] hdr = new byte[RS_BLOCK_LEN];
            int i, corrections;

            memset(hdr, 0, RS_BLOCK_LEN - RS_CODEWORD_LEN);
            for (i = 0; i < RS_CODEWORD_LEN; i++)
                hdr[RS_BLOCK_LEN - i - 1] = buf[i];

            /*corrections = decode_rs_char(this.rs_dec, hdr, NULL, 0);

            if (corrections == -1)
                return 0;*/
            corrections = 0;

            for (i = 0; i < RS_BLOCK_LEN - RS_CODEWORD_LEN; i++)
                if (hdr[i] != 0)
                    return 0;

            if (corrections > 0)
                Console.WriteLine("RS corrected %d symbols", corrections);

            for (i = 0; i < RS_CODEWORD_LEN; i++)
                buf[i] = hdr[RS_BLOCK_LEN - i - 1];
            return 1;
        }

        void parse_header(byte* buf, frame_header_t hdr)
        {
            hdr.codec = buf[8] & 0xf;
            hdr.stream_id = (buf[8] >> 4) & 0x3;
            hdr.pdu_seq = (buf[8] >> 6) | ((buf[9] & 1) << 2);
            hdr.blend_control = (buf[9] >> 1) & 0x3;
            hdr.per_stream_delay = buf[9] >> 3;
            hdr.common_delay = buf[10] & 0x3f;
            hdr.latency = (buf[10] >> 6) | ((buf[11] & 1) << 2);
            hdr.pfirst = ((buf[11] >> 1) & 1) != 0;
            hdr.plast = ((buf[11] >> 2) & 1) != 0;
            hdr.seq = (buf[11] >> 3) | ((buf[12] & 1) << 5);
            hdr.nop = (buf[12] >> 1) & 0x3f;
            hdr.hef = buf[12] >> 7;
            hdr.la_location = buf[13];
        }

        /*unsigned*/ /*uint*/
        int parse_hef(byte* buf, /*unsigned*/ /*uint*/ int length, hef_t hef)
        {
            byte* b = buf;
            byte* end = buf + length;

            do
            {
                if (b >= end) return length;

                switch ((*b >> 4) & 0x7)
                {
                    case 0:
                        hef.class_ind = *b & 0xf;
                        break;
                    case 1:
                        hef.prog_num = (*b >> 1) & 0x7;
                        if ((*b & 0x1) != 0)
                        {
                            if (b + 2 >= end) return length;
                            b++;
                            hef.pdu_len = (*b & 0x7f) << 7;
                            b++;
                            hef.pdu_len |= (*b & 0x7f);
                        }
                        break;
                    case 2:
                        if (b + 1 >= end) return length;
                        hef.access = (*b >> 3) & 0x1;
                        hef.prog_type = (*b & 0x1) << 7;
                        b++;
                        hef.prog_type |= (*b & 0x7f);
                        break;
                    case 3:
                        if ((*b & 0x8) != 0)
                        {
                            if (b + 4 >= end) return length;
                            b += 4;
                        }
                        else
                        {
                            if (b + 3 >= end) return length;
                            b += 3;
                        }
                        break;
                    case 4:
                        if ((*b & 0x8) != 0)
                        {
                            if (b + 3 >= end) return length;
                            hef.applied_services = (*b & 0x7);
                            b++;
                            hef.pdu_marker = (*b & 0x7f) << 14;
                            b++;
                            hef.pdu_marker |= (*b & 0x7f) << 7;
                            b++;
                            hef.pdu_marker |= (*b & 0x7f);
                        }
                        else
                        {
                            if (b + 1 >= end) return length;
                            b++;
                        }
                        break;
                    default:
                        throw new Exception("unknown header expansion ID");
                }
            } while ((*(b++) & 0x80) != 0);

            return (int)(b - buf);
        }

        /*unsigned*/ /*uint*/
        int calc_lc_bits(frame_header_t hdr)
        {
            switch (hdr.codec)
            {
                case 0:
                    return 16;
                case 1:
                case 2:
                case 3:
                    if (hdr.stream_id == 0)
                        return 12;
                    else
                        return 16;
                case 10:
                case 13:
                    return 12;
                default:
                    throw new Exception("unknown codec field " + hdr.codec);
                    return 16;
            }
        }

        /*unsigned*/ /*uint*/
        int parse_location(byte* buf, /*unsigned*/ /*uint*/ int lc_bits, /*unsigned*/ /*uint*/ int i)
        {
            if (lc_bits == 16)
                return (buf[2 * i + 1] << 8) | buf[2 * i];
            else
            {
                if (i % 2 == 0)
                    return ((buf[i / 2 * 3 + 1] & 0xf) << 8) | buf[i / 2 * 3];
                else
                    return (buf[i / 2 * 3 + 2] << 4) | (buf[i / 2 * 3 + 1] >> 4);
            }
        }

        int unescape_hdlc(byte* data, int length)
        {
            byte* p = data;

            for (int i = 0; i < length; i++)
            {
                if (data[i] == 0x7D)
                    *p++ = (byte)(data[++i] | 0x20);
                else
                    *p++ = data[i];
            }

            return (int)(p - data);
        }

        void aas_push(byte* psd, /*unsigned*/ /*uint*/ int length)
        {
            length = unescape_hdlc(psd, length);

            if (length == 0)
            {
                // empty frames are used as padding
            }
            else if (fcs16(psd, length) != VALIDFCS16)
            {
                Console.WriteLine("psd crc mismatch");
            }
            else if (psd[0] != 0x21)
            {
                Console.WriteLine("unknown AAS protocol %x", psd[0]);
            }
            else
            {
                // remove protocol and fcs fields
                input_aas_push(psd + 1, length - 3);
            }
        }

        delegate void parseCallback(byte* ptr, /*unsigned*/ /*uint*/ int len);
        void parse_hdlc(parseCallback process, byte* buffer, ref int bufidx, int bufsz, byte* input, int inlen)
        {
            for (int i = 0; i < inlen; i++)
            {
                byte b = input[i];
                if (b == 0x7E)
                {
                    if (bufidx >= 0)
                        process(buffer, bufidx);
                    bufidx = 0;
                }
                else if (bufidx >= 0)
                {
                    if (bufidx == bufsz)
                    {
                        throw new Exception("HDLC buffer overflow");
                        bufidx = -1;
                        continue;
                    }
                    buffer[bufidx++] = b;
                }
            }
        }

        void process_fixed_ccc(byte* buf, /*unsigned*/ /*uint*/ int buflen)
        {
            buflen = unescape_hdlc(buf, buflen);

            // padding
            if (buflen == 0)
                return;

            // ignore new CCC packets (XXX they shouldn't change)
            if (this.fixed_ready != 0)
                return;

            if (fcs16(buf, buflen) != VALIDFCS16)
            {
                Console.WriteLine("bad CCC checksum");
                return;
            }

            for (/*unsigned*/ /*uint*/ int i = 0; i < 4; i++)
            {
                if (this.subchannel[i] == null)
                    this.subchannel[i] = new fixed_subchannel_t();
                fixed_subchannel_t subch = this.subchannel[i];
                subch.mode = 0;
                subch.length = 0;

                if (5 + i * 4 <= buflen)
                {
                    ushort mode = (ushort)(buf[1 + i * 4] | (buf[2 + i * 4] << 8));
                    ushort length = (ushort)(buf[3 + i * 4] | (buf[4 + i * 4] << 8));

                    if (mode == 0)
                    {
                        subch.mode = mode;
                        subch.length = length;
                        subch.block_idx = 0;
                        subch.idx = -1;
                    }
                    else
                    {
                        throw new Exception("Subchannel mode " + mode + " not supported");
                    }
                }
            }

            this.fixed_ready = 1;
        }

        /* FIXME: We only support mode=0 (no FEC, no interleaving) */
        void process_fixed_block(int i)
        {
            fixed_subchannel_t subch = this.subchannel[i];
            parse_hdlc(aas_push, subch.data, ref subch.idx, MAX_AAS_LEN, &subch.blocks[4], 255);
        }

        int process_fixed_data(int length)
        {
            byte[] bbm = new byte[] { 0x7D, 0x3A, 0xE2, 0x42 };
            byte* p = &this.buffer[length - 1];

            if (this.sync_count < 2)
            {
                /*unsigned*/ /*uint*/
                int width = (*p & 0xF) * 2;
                if (this.sync_width == width)
                    this.sync_count++;
                else
                    this.sync_count = 0;
                this.sync_width = width;

                if (this.sync_count < 2)
                    return (int)(p - this.buffer);
            }

            p -= this.sync_width;
            parse_hdlc(process_fixed_ccc, this.ccc_buf, ref this.ccc_idx, CCM_BUF_LEN, p, this.sync_width);

            // wait until we have subchannel information
            if (this.fixed_ready == 0)
                return (int)(p - this.buffer);

            for (int i = 3; i >= 0; i--)
            {
                fixed_subchannel_t subch = this.subchannel[i];
                length = subch.length;

                if (length == 0)
                    continue;

                p -= length;
                for (int j = 0; j < length; j++)
                {
                    subch.blocks[subch.block_idx++] = p[j];
                    fixed(byte* bbmPtr = bbm)
                    {
                        if (subch.block_idx == 4 && memcmp(subch.blocks, bbmPtr, sizeof(byte)) != 0)
                        {
                            // mis-aligned, skip a byte
                            memmove(subch.blocks, subch.blocks + 1, 3);
                            subch.block_idx--;
                        }
                    }

                    if (subch.block_idx == 255 + 4)
                    {
                        // we have a complete block, deinterleave and process
                        process_fixed_block(i);
                        subch.block_idx = 0;
                    }
                }
            }

            return (int)(p - buffer);
        }

        public void frame_push(byte[] bits, int length)
        {
            /*unsigned*/ /*uint*/
            int start, offset;
            /*unsigned*/ /*uint*/
            int i, j = 0, h = 0, header = 0, val = 0;
            byte* ptr = this.buffer;

            switch (length)
            {
                case P1_FRAME_LEN:
                    start = P1_FRAME_LEN - 30000;
                    offset = 1248;
                    break;
                case P3_FRAME_LEN:
                    start = 120;
                    offset = 184;
                    break;
                default:
                    throw new Exception("Unknown frame length: " + length);
            }

            for (i = 0; i < length; ++i)
            {
                // swap bit order
                byte bit = bits[((i >> 3) << 3) + 7 - (i & 7)];
                if (i >= start && ((i - start) % offset) == 0 && h < PCI_LEN)
                {
                    header = (header << 1) | bit;
                    ++h;
                }
                else
                {
                    val |= bit << (7 - j);
                    if (++j == 8)
                    {
                        *ptr++ = (byte)val;
                        val = 0;
                        j = 0;
                    }
                }
            }

            this.pci = header;
            frame_process((int)(ptr - this.buffer));
        }

        void frame_process(int length)
        {
            /*unsigned*/ /*uint*/
            int offset = 0;
            /*unsigned*/ /*uint*/
            int audio_end = length;

            if (has_fixed())
                audio_end = process_fixed_data(length);

            while (offset < audio_end - RS_CODEWORD_LEN)
            {
                /*unsigned*/ /*uint*/
                int start = offset;
                /*unsigned*/ /*uint*/
                int j, lc_bits, loc_bytes, prog;
                /*unsigned*/
                short[] locations = new short[MAX_AUDIO_PACKETS];
                frame_header_t hdr = new frame_header_t();
                hef_t hef = new hef_t();

                if (fix_header(this.buffer + offset) == 0)
                {
                    // go back to coarse sync if we fail to decode any audio packets in a P1 frame
                    if (length == MAX_PDU_LEN && offset == 0)
                        sync.syncState = SYNC_STATE.SYNC_STATE_NONE;
                    return;
                }

                parse_header(this.buffer + offset, hdr);
                offset += 14;
                lc_bits = calc_lc_bits(hdr);
                loc_bytes = ((lc_bits * hdr.nop) + 4) / 8;
                if (start + hdr.la_location < offset + loc_bytes || start + hdr.la_location >= audio_end)
                    return;

                for (j = 0; j < hdr.nop; j++)
                {
                    locations[j] = (short)parse_location(this.buffer + offset, lc_bits, j);
                    if (j == 0 && locations[j] <= hdr.la_location) return;
                    if (j > 0 && locations[j] <= locations[j - 1]) return;
                    if (start + locations[j] >= audio_end) return;
                }
                offset += loc_bytes;

                if (hdr.hef != 0)
                    offset += parse_hef(this.buffer + offset, audio_end - offset, hef);
                prog = hef.prog_num;

                parse_hdlc(aas_push, this.psd_buf[prog], ref this.psd_idx[prog], MAX_AAS_LEN, this.buffer + offset, start + hdr.la_location + 1 - offset);
                offset = start + hdr.la_location + 1;

                for (j = 0; j < hdr.nop; ++j)
                {
                    /*unsigned*/ /*uint*/
                    int cnt = start + locations[j] - offset;
                    if (crc8(this.buffer + offset, cnt + 1) != 0)
                    {
                        Console.WriteLine("crc mismatch!");
                        offset += cnt + 1;
                        continue;
                    }

                    if (j == 0 && hdr.pfirst)
                    {
                        if (this.pdu_idx[prog] != 0)
                        {
                            Utils.Memcpy(this.pdu[prog] + this.pdu_idx[prog], this.buffer + offset, cnt);
                            input_pdu_push(this.pdu[prog], cnt + this.pdu_idx[prog], prog);
                        }
                        else
                        {
                            Console.WriteLine("ignoring partial pdu");
                        }
                    }
                    else if (j == hdr.nop - 1 && hdr.plast)
                    {
                        Utils.Memcpy(this.pdu[prog], this.buffer + offset, cnt);
                        this.pdu_idx[prog] = cnt;
                    }
                    else
                    {
                        input_pdu_push(this.buffer + offset, cnt, prog);
                    }

                    offset += cnt + 1;
                }
            }

            

        }

        void input_pdu_push(byte* pdu, int len, int program)
        {
            //Marshal the data into a byte array
            byte[] payload = new byte[len];
            fixed (byte* payloadPtr = payload)
                Utils.Memcpy(payloadPtr, pdu, len);

            //Make frame
            FramePdu frame = new FramePdu
            {
                payload = payload,
                program = program
            };

            //Dispatch
            OnPduFrame?.Invoke(frame);
        }

        void input_aas_push(byte* psd, int len)
        {
            //Read metadata header
            int port = psd[0] | (psd[1] << 8);
            int seq = psd[2] | (psd[3] << 8);

            //Offset data region
            psd += 4;
            len -= 4;

            //Marshal the data into a byte array
            byte[] payload = new byte[len];
            fixed (byte* payloadPtr = payload)
                Utils.Memcpy(payloadPtr, psd, len);

            //Make frame
            FrameAas frame = new FrameAas
            {
                payload = payload,
                port = (ushort)port,
                sequence = (ushort)seq
            };

            //Dispatch
            OnAasFrame?.Invoke(frame);
        }
    }
}