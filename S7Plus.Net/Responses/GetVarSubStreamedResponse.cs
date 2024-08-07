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

namespace S7Plus.Net.Responses
{
    public class GetVarSubStreamedResponse : S7ResponseBase, IS7Response
    {
        public byte ProtocolVersion { get; private set; }

        public UInt32 IntegrityId { get; private set; }

        public bool WithIntegrityId => true;
        public byte TransportFlags { get; private set; }
        public UInt64 ReturnValue { get; private set; }
        public S7VariableBase? Value { get; private set; }

        public static GetVarSubStreamedResponse Deserialize(Stream buffer)
        {
            GetVarSubStreamedResponse response = new GetVarSubStreamedResponse();
            response.DeserializeBase(buffer);

            if (response.FunctionCode != Functioncode.GetVarSubStreamed)
                throw new InvalidDataException($"Invalid function code in response to GetVarSubStreamed request: {response.FunctionCode}");

            response.TransportFlags = S7ValueDecoder.DecodeByte(buffer);
            response.ReturnValue = S7VlqValueDecoder.DecodeUInt64Vlq(buffer);

            S7ValueDecoder.DecodeByte(buffer); //unknown

            response.Value = S7VariableBase.Deserialize(buffer);
            response.IntegrityId = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);

            return response;
        }
    }
}
