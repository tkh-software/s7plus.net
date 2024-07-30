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

namespace S7Plus.Net.Helpers
{
    public static class S7VlqValueEncoder
    {
        private static int EncodeVlq(Stream buffer, ulong value)
        {
            byte[] bytes = new byte[10]; // Maximum 10 bytes for 64-bit VLQ
            int i, j;
            for (i = 9; i > 0; i--)
            {
                if ((value & (0x7fUL << (i * 7))) > 0)
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

        private static int EncodeSignedVlq(Stream buffer, long value, int maxBytes)
        {
            byte[] b = new byte[maxBytes];
            ulong abs_v;
            if (value == long.MinValue) // Handle this, otherwise Math.Abs() will fail
            {
                abs_v = (ulong)(1L << (maxBytes * 7 - 1));
            }
            else
            {
                abs_v = (ulong)Math.Abs(value);
            }

            b[0] = (byte)(value & 0x7f);
            int length = 1;
            for (int i = 1; i < maxBytes; i++)
            {
                if (abs_v >= 0x40)
                {
                    length++;
                    abs_v >>= 7;
                    value >>= 7;
                    b[i] = (byte)((value & 0x7f) + 0x80);
                }
                else
                {
                    break;
                }
            }
            buffer.Write(b, 0, length);
            return length;
        }

        public static int EncodeUInt32Vlq(Stream buffer, UInt32 value)
        {
            return EncodeVlq(buffer, value);
        }

        public static int EncodeInt32Vlq(Stream buffer, Int32 value)
        {
            return EncodeSignedVlq(buffer, value, 5);
        }

        public static int EncodeUInt64Vlq(Stream buffer, UInt64 value)
        {
            return EncodeVlq(buffer, value);
        }

        public static int EncodeInt64Vlq(Stream buffer, Int64 value)
        {
            return EncodeSignedVlq(buffer, value, 10);
        }
    }
}