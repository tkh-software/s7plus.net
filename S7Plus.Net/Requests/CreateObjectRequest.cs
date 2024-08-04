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
using S7Plus.Net.Constants;
using S7Plus.Net.Helpers;
using S7Plus.Net.S7Variables;

namespace S7Plus.Net.Requests
{
    public class CreateObjectRequest : S7RequestBase
    {
        private const byte TRANSPORT_FLAGS = 0x36;

        public override UInt16 FunctionCode => Functioncode.CreateObject;
        public UInt32 RequestId { get; set; }
        public S7VariableBase RequestValue { get; set; }
        public S7Object RequestObject { get; set; }

        public CreateObjectRequest(byte protocolVersion, bool withIntegrityId) : base(protocolVersion)
        {
            WithIntegrityId = withIntegrityId;
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);
            length += S7ValueEncoder.EncodeByte(buffer, TRANSPORT_FLAGS);

            length += S7ValueEncoder.EncodeUInt32(buffer, RequestId);
            length += RequestValue.Serialize(buffer);
            length += S7ValueEncoder.EncodeUInt32(buffer, 0); // placeholder

            if (WithIntegrityId)
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, IntegrityId);

            length += RequestObject.Serialize(buffer);
            length += S7ValueEncoder.EncodeUInt32(buffer, 0); // placeholder

            return length;
        }
    }
}
