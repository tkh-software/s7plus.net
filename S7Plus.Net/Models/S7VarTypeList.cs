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
using System.Collections.Generic;
using System.IO;
using S7Plus.Net.Helpers;

namespace S7Plus.Net.Models
{
    public class S7VarTypeList
    {
        public List<S7VarType> Types { get; } = new List<S7VarType>();
        public UInt32 FirstId { get; private set; }

        public static S7VarTypeList Deserialize(Stream buffer)
        {
            S7VarTypeList result = new S7VarTypeList();

            int length = 0;
            int maxLength;

            UInt16 blockLength = S7ValueDecoder.DecodeUInt16(buffer);
            length += sizeof(UInt16);

            result.FirstId = S7ValueDecoder.DecodeUInt32LE(buffer);
            length += sizeof(UInt32);

            maxLength = length + blockLength;
            while (blockLength > 0)
            {
                do
                {
                    int startPos = (int)buffer.Position;
                    S7VarType type = S7VarType.Deserialize(buffer);
                    length += (int)(buffer.Position - startPos);
                    result.Types.Add(type);

                } while (length < maxLength);

                blockLength = S7ValueDecoder.DecodeUInt16(buffer);
                length += sizeof(UInt16);
                maxLength = length + blockLength;
            }

            return result;
        }
    }
}