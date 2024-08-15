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
using TKH.S7Plus.Net.Helpers;
using System;
using System.IO;

namespace TKH.S7Plus.Net.Requests
{
    public class GetVarSubStreamedRequest : S7RequestBase
    {
        private const byte TRANSPORT_FLAGS = 0x34;

        public GetVarSubStreamedRequest(byte protocolVersion, UInt32 objectId, UInt32 address) : base(protocolVersion)
        {
            ObjectId = objectId;
            Address = address;
            WithIntegrityId = true;
        }

        public UInt32 ObjectId { get; }
        public UInt32 Address { get; }
        public override UInt16 FunctionCode => Functioncode.GetVarSubStreamed;

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);

            length += S7ValueEncoder.EncodeByte(buffer, TRANSPORT_FLAGS);

            length += S7ValueEncoder.EncodeUInt32(buffer, ObjectId);
            length += S7ValueEncoder.EncodeByte(buffer, 0x20); // address array
            length += S7ValueEncoder.EncodeByte(buffer, Datatype.UDInt);
            length += S7ValueEncoder.EncodeByte(buffer, 1); // number of elements
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, Address);

            length += S7ValueEncoder.EncodeObjectQualifier(buffer);
            length += S7ValueEncoder.EncodeUInt16(buffer, 1); // unknown

            if (WithIntegrityId)
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, IntegrityId);

            length += S7ValueEncoder.EncodeUInt32(buffer, 0); // placeholder

            return length;
        }
    }
}