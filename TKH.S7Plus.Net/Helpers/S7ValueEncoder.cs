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

using TKH.S7Plus.Net.Constants;
using TKH.S7Plus.Net.S7Variables;
using System;
using System.IO;

namespace TKH.S7Plus.Net.Helpers
{
    public static class S7ValueEncoder
    {
        private static int Encode(Stream buffer, ulong value, int byteCount)
        {
            for (int i = (byteCount - 1) * 8; i >= 0; i -= 8)
            {
                buffer.WriteByte((byte)((value >> i) & 0xFF));
            }
            return byteCount;
        }

        public static int EncodeByte(Stream buffer, byte value)
        {
            return Encode(buffer, value, 1);
        }

        public static int EncodeUInt16(Stream buffer, UInt16 value)
        {
            return Encode(buffer, value, 2);
        }

        public static int EncodeInt16(Stream buffer, Int16 value)
        {
            return Encode(buffer, (ulong)value, 2);
        }

        public static int EncodeUInt32(Stream buffer, UInt32 value)
        {
            return Encode(buffer, value, 4);
        }

        public static int EncodeInt32(Stream buffer, Int32 value)
        {
            return Encode(buffer, (ulong)value, 4);
        }

        public static int EncodeUInt64(Stream buffer, UInt64 value)
        {
            return Encode(buffer, value, 8);
        }

        public static int EncodeInt64(Stream buffer, Int64 value)
        {
            return Encode(buffer, (ulong)value, 8);
        }

        public static int EncodeFloat(Stream buffer, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            buffer.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        public static int EncodeDouble(Stream buffer, double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            buffer.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        public static int EncodeOctets(Stream buffer, byte[] value)
        {
            buffer.Write(value, 0, value.Length);
            return value.Length;
        }

        public static int EncodeString(Stream buffer, string value)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            buffer.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        public static int EncodeObjectQualifier(Stream buffer)
        {
            int length = 0;

            length += EncodeUInt32(buffer, S7Ids.ObjectQualifier);

            var parentRID = new S7VariableRID(0);
            var compositionAID = new S7VariableAID(0);
            var keyQualifier = new S7VariableUDInt(0);

            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, S7Ids.ParentRID);
            length += parentRID.Serialize(buffer);

            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, S7Ids.CompositionAID);
            length += compositionAID.Serialize(buffer);

            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, S7Ids.KeyQualifier);
            length += keyQualifier.Serialize(buffer);

            length += EncodeByte(buffer, 0);

            return length;
        }
    }
}