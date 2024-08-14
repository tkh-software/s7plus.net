#region License
/******************************************************************************
 * S7Plus.Net
 * 
 * Copyright (C) 2024 TKH Software GmbH, www.tkh-software.com
 * Copyright (C) 2023 Thomas Wiens, th.wiens@gmx.de
 *
 * This file is part of the S7Plus.Net project, which is based on the
 * S7CommPlusDriver project by Thomas Wiens
 * (https://github.com/thomas-v2/S7CommPlusDriver).
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 /****************************************************************************/
#endregion

using System;
using System.IO;

namespace TKH.S7Plus.Net.Helpers
{
    public static class S7VlqValueEncoder
    {
        public static int EncodeUInt32Vlq(Stream buffer, UInt32 value)
        {
            byte[] bytes = new byte[5];
            int i, j;
            for (i = 4; i > 0; i--)
            {
                if ((value & (0x7f << (i * 7))) > 0)
                {
                    break;
                }
            }
            for (j = 0; j <= i; j++)
            {
                bytes[j] = (byte)(((value >> ((i - j) * 7)) & 0x7f) | 0x80);
            }
            bytes[i] ^= 0x80;
            buffer.Write(bytes, 0, i + 1);
            return i + 1;
        }

        public static int EncodeInt32Vlq(Stream buffer, Int32 value)
        {
            byte[] bytes = new byte[5];
            uint absValue = (value == int.MinValue) ? (uint)int.MaxValue + 1 : (uint)Math.Abs(value);
            bytes[0] = (byte)(value & 0x7f);
            int length = 1;
            for (int i = 1; i < 5; i++)
            {
                if (absValue >= 0x40)
                {
                    length++;
                    absValue >>= 7;
                    value >>= 7;
                    bytes[i] = (byte)((value & 0x7f) + 0x80);
                }
                else
                {
                    break;
                }
            }

            for (int i = length - 1; i >= 0; i--)
            {
                buffer.Write(bytes, i, 1);
            }
            return length;
        }

        public static int EncodeUInt64Vlq(Stream buffer, UInt64 value)
        {
            byte[] bytes = new byte[9];
            bool special = value > 0x00ffffffffffffff;
            if (special)
            {
                bytes[0] = (byte)(value & 0xff);
            }
            else
            {
                bytes[0] = (byte)(value & 0x7f);
            }

            int length = 1;
            for (int i = 1; i < 9; i++)
            {
                if (value >= 0x80)
                {
                    length++;
                    if (i == 1 && special)
                    {
                        value >>= 8;
                    }
                    else
                    {
                        value >>= 7;
                    }
                    bytes[i] = (byte)((value & 0x7f) + 0x80);
                }
                else
                {
                    break;
                }
            }

            if (special && length == 8)
            {
                length++;
                bytes[8] = 0x80;
            }

            for (int i = length - 1; i >= 0; i--)
            {
                buffer.Write(bytes, i, 1);
            }
            return length;
        }

        public static int EncodeInt64Vlq(Stream buffer, Int64 value)
        {
            byte[] bytes = new byte[9];
            UInt64 absValue = (value == Int64.MinValue) ? (UInt64)Int64.MaxValue + 1 : (UInt64)Math.Abs(value);
            bool special = absValue > 0x007fffffffffffff;
            if (special)
            {
                bytes[0] = (byte)(value & 0xff);
            }
            else
            {
                bytes[0] = (byte)(value & 0x7f);
            }

            int length = 1;
            for (int i = 1; i < 9; i++)
            {
                if (absValue >= 0x40)
                {
                    length++;
                    if (i == 1 && special)
                    {
                        absValue >>= 8;
                        value >>= 8;
                    }
                    else
                    {
                        absValue >>= 7;
                        value >>= 7;
                    }
                    bytes[i] = (byte)((value & 0x7f) + 0x80);
                }
                else
                {
                    break;
                }
            }

            if (special && length == 8)
            {
                length++;
                bytes[8] = (value >= 0) ? (byte)0x80 : (byte)0xff;
            }

            for (int i = length - 1; i >= 0; i--)
            {
                buffer.Write(bytes, i, 1);
            }
            return length;
        }
    }
}