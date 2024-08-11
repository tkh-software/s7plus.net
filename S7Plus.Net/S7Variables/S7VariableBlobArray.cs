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

using S7Plus.Net.Helpers;
using System;
using System.IO;

namespace S7Plus.Net.S7Variables
{
    public class S7VariableBlobArray : S7VariableBase
    {
        public S7VariableBlob[] Value { get; private set; }
        public UInt32 BlobRootId { get; set; }
        public byte Blobtype { get; }

        public S7VariableBlobArray() : this(Array.Empty<S7VariableBlob>(), FLAGS_ADDRESSARRAY)
        {
        }

        public S7VariableBlobArray(S7VariableBlob[] value) : this(value, FLAGS_ADDRESSARRAY)
        {
        }

        public S7VariableBlobArray(S7VariableBlob[] value, byte flags, byte blobtype = Constants.Blobtype.Unknown)
        {
            Datatype = Constants.Datatype.Blob;
            _datatypeFlags = flags;

            Value = new S7VariableBlob[value.Length];
            Array.Copy(value, Value, value.Length);

            Blobtype = blobtype;
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (uint)Value.Length);

            foreach (S7VariableBlob item in Value)
            {
                length += item.Serialize(buffer);
            }

            return length;
        }

        public static S7VariableBlobArray Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            UInt32 size = disableVlq ? S7ValueDecoder.DecodeUInt32(buffer) : S7VlqValueDecoder.DecodeUInt32Vlq(buffer);

            S7VariableBlob[] value = new S7VariableBlob[size];

            for (int i = 0; i < size; i++)
            {
                value[i] = S7VariableBlob.Deserialize(buffer, flags, disableVlq);
            }

            return new S7VariableBlobArray(value, flags);
        }
    }
}