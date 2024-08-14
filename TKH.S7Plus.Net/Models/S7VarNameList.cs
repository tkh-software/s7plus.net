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

using TKH.S7Plus.Net.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace TKH.S7Plus.Net.Models
{
    public class S7VarNameList
    {
        public List<string> Names { get; } = new List<string>();

        public static S7VarNameList Deserialize(Stream buffer)
        {
            S7VarNameList result = new S7VarNameList();

            int length = 0;
            int maxLength;

            UInt16 blockLength = S7ValueDecoder.DecodeUInt16(buffer);
            length += sizeof(UInt16);

            maxLength = length + blockLength;
            while (blockLength > 0)
            {
                do
                {
                    // Length of a name is max. 128 chars
                    byte nameLength = S7ValueDecoder.DecodeByte(buffer);
                    length += sizeof(byte);

                    string name = S7ValueDecoder.DecodeString(buffer, nameLength);
                    length += nameLength;

                    result.Names.Add(name);

                    // Additional 1 Byte with 0 termination of the string
                    S7ValueDecoder.DecodeByte(buffer);
                    length += sizeof(byte);

                } while (length < maxLength);

                blockLength = S7ValueDecoder.DecodeUInt16(buffer);
                length += sizeof(UInt16);
                maxLength = length + blockLength;
            }

            return result;
        }
    }
}