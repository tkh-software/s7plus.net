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
using System;
using System.IO;

namespace S7Plus.Net.Requests
{
    public class InitSslRequest : S7RequestBase
    {
        private const byte TRANSPORT_FLAGS = 0x30;

        public InitSslRequest(byte protocolVersion) : base(protocolVersion)
        {
            WithIntegrityId = false;
        }

        public override UInt16 FunctionCode => Functioncode.InitSsl;

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);

            length += S7ValueEncoder.EncodeByte(buffer, TRANSPORT_FLAGS);
            length += S7ValueEncoder.EncodeUInt32(buffer, 0); // placeholder

            return length;
        }
    }
}