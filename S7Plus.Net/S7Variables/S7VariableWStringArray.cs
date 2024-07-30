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
using S7Plus.Net.Helpers;

namespace S7Plus.Net.S7Variables
{
    public class S7VariableWStringArray : S7VariableBase
    {
        public string[] Value { get; private set; }

        public S7VariableWStringArray() : this(Array.Empty<string>(), FLAGS_ADDRESSARRAY)
        {
        }

        public S7VariableWStringArray(string[] value) : this(value, FLAGS_ADDRESSARRAY)
        {
        }

        public S7VariableWStringArray(string[] value, byte flags)
        {
            Datatype = Constants.Datatype.WString;
            _datatypeFlags = flags;

            Value = new string[value.Length];
            Array.Copy(value, Value, value.Length);
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (uint)Value.Length);

            foreach (string item in Value)
            {
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (uint)item.Length);
                length += S7ValueEncoder.EncodeString(buffer, item);
            }

            return length;
        }

        public static S7VariableWStringArray Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            UInt32 size = disableVlq ? S7ValueDecoder.DecodeUInt32(buffer) : S7VlqValueDecoder.DecodeUInt32Vlq(buffer);

            string[] value = new string[size];
            for (int i = 0; i < size; i++)
            {
                UInt32 itemSize = disableVlq ? S7ValueDecoder.DecodeUInt32(buffer) : S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                value[i] = S7ValueDecoder.DecodeString(buffer, (int)itemSize);
            }

            return new S7VariableWStringArray(value, flags);
        }
    }
}