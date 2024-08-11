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

namespace S7Plus.Net.Responses
{
    public class InitSslResponse : S7ResponseBase, IS7Response
    {
        public byte ProtocolVersion { get; private set; }

        public UInt32 IntegrityId { get; private set; }

        public bool WithIntegrityId => false;
        public byte TransportFlags { get; private set; }
        public UInt64 ReturnValue { get; private set; }
        public bool Error { get; private set; }

        public static InitSslResponse Deserialize(Stream buffer)
        {
            InitSslResponse response = new InitSslResponse();
            response.DeserializeBase(buffer);

            if (response.FunctionCode != Functioncode.InitSsl)
                throw new InvalidDataException($"Invalid function code in response to GetVarSubStreamed request: {response.FunctionCode}");

            response.TransportFlags = S7ValueDecoder.DecodeByte(buffer);
            response.ReturnValue = S7VlqValueDecoder.DecodeUInt64Vlq(buffer);

            if ((response.ReturnValue & 0x4000000000000000) > 0) // error extension
            {
                response.Error = true;
            }

            return response;
        }
    }
}