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
using TKH.S7Plus.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace TKH.S7Plus.Net.Requests
{
    public class GetMultiVariablesRequest : S7RequestBase
    {
        private const byte TRANSPORT_FLAGS = 0x34;
        private const UInt32 LINK_ID = 0;

        private readonly List<IS7Address> _addresses = new List<IS7Address>();

        public GetMultiVariablesRequest(byte protocolVersion, List<IS7Address> addresses) : base(protocolVersion)
        {
            _addresses = addresses ?? throw new ArgumentNullException(nameof(addresses));
            WithIntegrityId = true;
        }

        public override UInt16 FunctionCode => Functioncode.GetMultiVariables;

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);

            length += S7ValueEncoder.EncodeByte(buffer, TRANSPORT_FLAGS);
            length += S7ValueEncoder.EncodeUInt32(buffer, LINK_ID);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)_addresses.Count);

            UInt32 fieldCount = 0;
            foreach (IS7Address adr in _addresses)
            {
                fieldCount += adr.FieldCount;
            }
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, fieldCount);

            foreach (IS7Address adr in _addresses)
            {
                length += adr.Serialize(buffer);
            }
            length += S7ValueEncoder.EncodeObjectQualifier(buffer);

            if (WithIntegrityId)
            {
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, IntegrityId);
            }

            length += S7ValueEncoder.EncodeUInt32(buffer, 0);

            return length;
        }
    }
}