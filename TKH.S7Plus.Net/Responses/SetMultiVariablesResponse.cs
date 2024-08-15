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
using System.Collections.Generic;
using System.IO;

namespace TKH.S7Plus.Net.Responses
{
    public class SetMultiVariablesResponse : S7ResponseBase, IS7Response
    {
        public UInt32 IntegrityId { get; private set; }

        public bool WithIntegrityId => true;
        public byte TransportFlags { get; private set; }
        public UInt64 ReturnValue { get; private set; }
        public Dictionary<UInt32, UInt64> ErrorValues { get; } = new Dictionary<UInt32, UInt64>();

        public static SetMultiVariablesResponse Deserialize(Stream buffer)
        {
            SetMultiVariablesResponse response = new SetMultiVariablesResponse();
            response.DeserializeBase(buffer);

            if (response.FunctionCode != Functioncode.SetMultiVariables)
                throw new InvalidDataException($"Invalid function code in response to SetMultiVariables request: {response.FunctionCode}");

            response.TransportFlags = S7ValueDecoder.DecodeByte(buffer);
            response.ReturnValue = S7VlqValueDecoder.DecodeUInt64Vlq(buffer);

            UInt32 errorNr = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
            while (errorNr > 0)
            {
                UInt64 errorCode = S7VlqValueDecoder.DecodeUInt64Vlq(buffer);
                response.ErrorValues.Add(errorNr, errorCode);
                errorNr = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
            }

            response.IntegrityId = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
            return response;
        }
    }
}