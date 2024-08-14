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
    public abstract class S7RequestBase : IS7Request
    {
        public S7RequestBase(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        public UInt32 SessionId { get; set; }

        public byte ProtocolVersion { get; }

        public UInt16 SequenceNumber { get; set; }

        public UInt32 IntegrityId { get; set; }

        public bool WithIntegrityId { get; set; }

        public abstract UInt16 FunctionCode { get; }

        public virtual int Serialize(Stream buffer)
        {
            int length = 0;
            length += S7ValueEncoder.EncodeByte(buffer, OpCode.Request);
            length += S7ValueEncoder.EncodeUInt16(buffer, 0);
            length += S7ValueEncoder.EncodeUInt16(buffer, FunctionCode);
            length += S7ValueEncoder.EncodeUInt16(buffer, 0);
            length += S7ValueEncoder.EncodeUInt16(buffer, SequenceNumber);
            length += S7ValueEncoder.EncodeUInt32(buffer, SessionId);

            return length;
        }
    }
}