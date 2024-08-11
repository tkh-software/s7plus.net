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

namespace S7Plus.Net.Responses
{
    public class CreateObjectResponse : S7ResponseBase, IS7Response
    {
        public UInt32 IntegrityId { get; private set; }

        public bool WithIntegrityId => false;
        public byte TransportFlags { get; private set; }
        public UInt64 ReturnValue { get; private set; }
        public byte ObjectIdCount { get; private set; }
        public List<UInt32> ObjectIds { get; } = new List<UInt32>();
        public S7Object? Object { get; private set; }

        public static CreateObjectResponse Deserialize(Stream buffer)
        {
            CreateObjectResponse response = new CreateObjectResponse();
            response.DeserializeBase(buffer);

            if (response.FunctionCode != Functioncode.CreateObject)
                throw new InvalidDataException($"Invalid function code in response to CreateObject request: {response.FunctionCode}");

            response.TransportFlags = S7ValueDecoder.DecodeByte(buffer);
            response.ReturnValue = S7VlqValueDecoder.DecodeUInt64Vlq(buffer);
            response.ObjectIdCount = S7ValueDecoder.DecodeByte(buffer);

            for (byte i = 0; i < response.ObjectIdCount; i++)
            {
                response.ObjectIds.Add(S7VlqValueDecoder.DecodeUInt32Vlq(buffer));
            }

            response.Object = S7Object.Deserialize(buffer);
            return response;
        }
    }
}