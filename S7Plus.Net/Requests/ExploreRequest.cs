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

using S7Plus.Net.Constants;
using S7Plus.Net.Helpers;
using S7Plus.Net.S7Variables;
using System;
using System.Collections.Generic;
using System.IO;

namespace S7Plus.Net.Requests
{
    public class ExploreRequest : S7RequestBase
    {
        private const byte TRANSPORT_FLAGS = 0x34;
        private readonly List<UInt32> _attributes = new List<UInt32>();

        public ExploreRequest(byte protocolVersion) : base(protocolVersion)
        {
            WithIntegrityId = true;
        }

        public override UInt16 FunctionCode => Functioncode.Explore;
        public UInt32 ExploreId { get; set; }
        public UInt32 ExploreRequestId { get; set; } = S7Ids.None;
        public bool ExploreChildren { get; set; }
        public bool ExploreParents { get; set; }
        public S7VariableStruct? FilterData { get; set; }
        public List<UInt32> Attributes => _attributes;

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);

            length += S7ValueEncoder.EncodeByte(buffer, TRANSPORT_FLAGS);
            length += S7ValueEncoder.EncodeUInt32(buffer, ExploreId);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, ExploreRequestId);

            length += S7ValueEncoder.EncodeByte(buffer, ExploreChildren ? (byte)1 : (byte)0);
            length += S7ValueEncoder.EncodeByte(buffer, 1); // unknown
            length += S7ValueEncoder.EncodeByte(buffer, ExploreParents ? (byte)1 : (byte)0);

            if (FilterData != null)
            {
                length += S7ValueEncoder.EncodeByte(buffer, 1); // unknown
                length += FilterData.Serialize(buffer);
            }

            length += S7ValueEncoder.EncodeByte(buffer, 0); // unknown

            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)_attributes.Count);
            foreach (UInt32 attribute in _attributes)
            {
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, attribute);
            }

            if (WithIntegrityId)
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, IntegrityId);

            // Fill 5 bytes with 0
            length += S7ValueEncoder.EncodeUInt32(buffer, 0);
            length += S7ValueEncoder.EncodeByte(buffer, 0);

            return length;
        }
    }
}