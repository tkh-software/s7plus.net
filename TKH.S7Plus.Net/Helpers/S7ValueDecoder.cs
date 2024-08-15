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
    public static class S7ValueDecoder
    {
        private static ulong Decode(Stream buffer, int byteCount)
        {
            if (buffer.Position >= buffer.Length)
                return 0;

            ulong value = 0;
            for (int i = 0; i < byteCount; i++)
            {
                value = (value << 8) | (ulong)(byte)buffer.ReadByte();
            }
            return value;
        }

        public static byte DecodeByte(Stream buffer)
        {
            return (byte)Decode(buffer, 1);
        }

        public static UInt16 DecodeUInt16(Stream buffer)
        {
            return (UInt16)Decode(buffer, 2);
        }

        public static Int16 DecodeInt16(Stream buffer)
        {
            return (Int16)Decode(buffer, 2);
        }

        public static UInt32 DecodeUInt32(Stream buffer)
        {
            return (UInt32)Decode(buffer, 4);
        }

        public static Int32 DecodeInt32(Stream buffer)
        {
            return (Int32)Decode(buffer, 4);
        }

        public static UInt64 DecodeUInt64(Stream buffer)
        {
            return Decode(buffer, 8);
        }

        public static Int64 DecodeInt64(Stream buffer)
        {
            return (Int64)Decode(buffer, 8);
        }

        public static UInt32 DecodeUInt32LE(Stream buffer)
        {
            byte[] bytes = new byte[4];
            buffer.Read(bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static float DecodeFloat(Stream buffer)
        {
            byte[] bytes = new byte[4];
            buffer.Read(bytes, 0, 4);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static double DecodeDouble(Stream buffer)
        {
            byte[] bytes = new byte[8];
            buffer.Read(bytes, 0, 8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static byte[] DecodeOctets(Stream buffer, int length)
        {
            byte[] bytes = new byte[length];
            buffer.Read(bytes, 0, length);
            return bytes;
        }

        public static string DecodeString(Stream buffer, int length)
        {
            byte[] bytes = new byte[length];
            buffer.Read(bytes, 0, length);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}