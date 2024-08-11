#region License
/******************************************************************************
 * S7Plus.Net
 * 
 * Copyright (C) 2024 TKH Software GmbH, www.tkh-software.com
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

using S7Plus.Net.Helpers;
using System;
using System.IO;

namespace S7Plus.Net.S7Variables
{
    public class S7VariableArray<T, TValue> : S7VariableBase where T : S7VariableBase, IS7Variable, new() where TValue : struct
    {
        public TValue[] Value { get; private set; }

        public S7VariableArray(TValue[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public S7VariableArray(TValue[] value, byte flags)
        {
            Datatype = new T().Datatype;
            _datatypeFlags = flags;
            Value = value;
        }

        public override int Serialize(Stream buffer)
        {
            int length = 0;
            length += S7ValueEncoder.EncodeByte(buffer, _datatypeFlags);
            length += S7ValueEncoder.EncodeByte(buffer, Datatype);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)Value.Length);

            foreach (TValue item in Value)
            {
                length += ConvertGeneric.Serialize<TValue>(buffer, item);
            }

            return length;
        }

        public static S7VariableArray<T, TValue> Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            UInt32 length = disableVlq ? S7ValueDecoder.DecodeUInt32(buffer) : S7VlqValueDecoder.DecodeUInt32Vlq(buffer);

            TValue[] value = new TValue[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ConvertGeneric.Deserialize<TValue>(buffer, disableVlq);
            }

            return new S7VariableArray<T, TValue>(value, flags);
        }
    }
}