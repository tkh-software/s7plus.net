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

using TKH.S7Plus.Net.Helpers;
using System;
using System.IO;

namespace TKH.S7Plus.Net.S7Variables
{
    public class S7VariableTimestampArray : S7VariableArray<S7VariableTimestamp, UInt64>
    {
        public S7VariableTimestampArray(UInt64[] value) : base(value) { }
        public S7VariableTimestampArray(UInt64[] value, byte flags) : base(value, flags) { }
        public static S7VariableTimestampArray Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            var baseArray = DeserializeBase(buffer, flags, disableVlq);
            return new S7VariableTimestampArray(baseArray.Value);
        }
    }

    public class S7VariableTimestamp : S7VariableBase
    {
        public UInt64 Value { get; private set; }

        public S7VariableTimestamp() : this(default, 0)
        {
        }

        public S7VariableTimestamp(UInt64 value) : this(value, 0)
        {
        }

        public S7VariableTimestamp(UInt64 value, byte flags)
        {
            Datatype = Constants.Datatype.Timestamp;
            _datatypeFlags = flags;
            Value = value;
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);
            length += S7ValueEncoder.EncodeUInt64(buffer, Value);
            return length;
        }

        public static S7VariableTimestamp Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            UInt64 value = disableVlq ? S7ValueDecoder.DecodeUInt64(buffer) : S7VlqValueDecoder.DecodeUInt64Vlq(buffer);
            return new S7VariableTimestamp(value, flags);
        }
    }
}