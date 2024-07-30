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
    public static class S7VlqValueDecoder
    {
        private static ulong DecodeUnsignedVlq(Stream buffer)
        {
            ulong value = 0;
            byte b;
            int shift = 0;
            do
            {
                b = (byte)buffer.ReadByte();
                value |= (ulong)(b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return value;
        }

        private static long DecodeSignedVlq(Stream buffer, int maxBytes)
        {
            long value = 0;
            byte b;
            int shift = 0;
            do
            {
                b = (byte)buffer.ReadByte();
                value |= (long)(b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            // Sign extension if the sign bit is set
            if (shift < maxBytes * 7 && (b & 0x40) != 0)
            {
                value |= -(1L << shift);
            }

            return value;
        }

        public static UInt32 DecodeUInt32Vlq(Stream buffer)
        {
            return (UInt32)DecodeUnsignedVlq(buffer);
        }

        public static Int32 DecodeInt32Vlq(Stream buffer)
        {
            return (Int32)DecodeSignedVlq(buffer, 5);
        }

        public static UInt64 DecodeUInt64Vlq(Stream buffer)
        {
            return DecodeUnsignedVlq(buffer);
        }

        public static Int64 DecodeInt64Vlq(Stream buffer)
        {
            return DecodeSignedVlq(buffer, 10);
        }
    }
}