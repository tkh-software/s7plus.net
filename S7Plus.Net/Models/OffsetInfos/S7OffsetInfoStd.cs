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
using System;
using System.IO;

namespace S7Plus.Net.Models.OffsetInfos
{
    public class S7OffsetInfoStd : S7OffsetInfo, IOffsetInfo
    {
        public override bool HasRelation => false;
        public override bool IsOneDimensional => false;
        public override bool IsMultiDimensional => false;

        public UInt32 OptimizedAddress { get; private set; }
        public UInt32 NonOptimizedAddress { get; private set; }

        new public static S7OffsetInfoStd Deserialize(Stream buffer, int type)
        {
            S7OffsetInfoStd result = new S7OffsetInfoStd();

            if (type == OffsetInfoType.Std)
            {
                result.OptimizedAddress = ReadUInt32LE(buffer);
                result.NonOptimizedAddress = ReadUInt32LE(buffer);
            }
            else
            {
                result.NonOptimizedAddress = ReadUInt32LE(buffer);
                result.OptimizedAddress = ReadUInt32LE(buffer);
            }

            return result;
        }
    }
}