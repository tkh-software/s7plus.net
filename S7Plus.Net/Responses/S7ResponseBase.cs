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

using S7Plus.Net.Helpers;
using System;
using System.IO;

namespace S7Plus.Net.Responses
{
    public class S7ResponseBase
    {

        public UInt16 SequenceNumber { get; private set; }

        public UInt16 FunctionCode { get; private set; }
        public byte OpCode { get; private set; }

        public void DeserializeBase(Stream buffer)
        {
            OpCode = S7ValueDecoder.DecodeByte(buffer);
            if (OpCode != Constants.OpCode.Response)
                return;

            S7ValueDecoder.DecodeUInt16(buffer); // reserved

            FunctionCode = S7ValueDecoder.DecodeUInt16(buffer);
            S7ValueDecoder.DecodeUInt16(buffer); // reserved
            SequenceNumber = S7ValueDecoder.DecodeUInt16(buffer);
        }
    }
}