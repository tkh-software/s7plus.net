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
using System.IO;

namespace TKH.S7Plus.Net.S7Variables
{
    public class S7VariableRealArray : S7VariableArray<S7VariableReal, float>
    {
        public S7VariableRealArray(float[] value) : base(value) { }
        public S7VariableRealArray(float[] value, byte flags) : base(value, flags) { }
        public static S7VariableRealArray Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            var baseArray = DeserializeBase(buffer, flags, disableVlq);
            return new S7VariableRealArray(baseArray.Value);
        }
    }

    public class S7VariableReal : S7VariableBase
    {
        public float Value { get; private set; }

        public S7VariableReal() : this(default, 0)
        {
        }

        public S7VariableReal(float value) : this(value, 0)
        {
        }

        public S7VariableReal(float value, byte flags)
        {
            Datatype = Constants.Datatype.Real;
            _datatypeFlags = flags;
            Value = value;
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);
            length += S7ValueEncoder.EncodeFloat(buffer, Value);
            return length;
        }

        public static S7VariableReal Deserialize(Stream buffer, byte flags)
        {
            float value = S7ValueDecoder.DecodeFloat(buffer);
            return new S7VariableReal(value, flags);
        }
    }
}