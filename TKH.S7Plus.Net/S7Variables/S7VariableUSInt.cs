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
using System.Linq;
using System.Text;
using TKH.S7Plus.Net.Helpers;

namespace TKH.S7Plus.Net.S7Variables
{
    public class S7VariableUSIntArray : S7VariableArray<S7VariableUSInt, byte>
    {
        public S7VariableUSIntArray(byte[] value) : base(value) { }
        public S7VariableUSIntArray(byte[] value, byte flags) : base(value, flags) { }
        public static S7VariableUSIntArray Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            var baseArray = DeserializeBase(buffer, flags, disableVlq);
            return new S7VariableUSIntArray(baseArray.Value);
        }
        public static S7VariableUSIntArray FromString(string str, int arraySize)
        {
            if (arraySize <= 2)
                throw new ArgumentOutOfRangeException(nameof(arraySize), "array size must be more than 2 (check PLC for correct size)");

            Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
            byte[] bytes = encoding.GetBytes(str);

            byte[] value = new byte[arraySize];
            value[0] = (byte)(arraySize - 2);
            value[1] = (byte)(bytes.Length);

            for (int i = 0; i < bytes.Length && i < arraySize - 2; i++)
                value[i + 2] = (byte)(bytes[i]);

            if (bytes.Length < arraySize - 3)
                value[bytes.Length + 2] = 0;

            return new S7VariableUSIntArray(value);
        }

        public string ConvertToString()
        {
            if (Value?.Length <= 3)
                return string.Empty;

            Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
            return encoding.GetString(Value!.Skip(2).Take(Value![1]).TakeWhile(v => v != 0).ToArray());
        }
    }

    public class S7VariableUSInt : S7VariableBase
    {
        public byte Value { get; private set; }

        public S7VariableUSInt() : this(default, 0)
        {
        }

        public S7VariableUSInt(byte value) : this(value, 0)
        {
        }

        public S7VariableUSInt(byte value, byte flags)
        {
            Datatype = Constants.Datatype.USInt;
            _datatypeFlags = flags;
            Value = value;
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);
            length += S7ValueEncoder.EncodeByte(buffer, Value);
            return length;
        }

        public static S7VariableUSInt Deserialize(Stream buffer, byte flags)
        {
            byte value = S7ValueDecoder.DecodeByte(buffer);
            return new S7VariableUSInt(value, flags);
        }
    }
}