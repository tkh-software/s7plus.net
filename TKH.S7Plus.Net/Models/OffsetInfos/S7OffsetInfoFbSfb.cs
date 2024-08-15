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

namespace TKH.S7Plus.Net.Models.OffsetInfos
{
    public class S7OffsetInfoFbSfb : S7OffsetInfo, IOffsetInfo, IOffsetInfoRelation
    {
        public override bool HasRelation => true;
        public override bool IsOneDimensional => false;
        public override bool IsMultiDimensional => false;

        public UInt16 Info1 { get; private set; }
        public UInt16 Info2 { get; private set; }
        public UInt32 Info3 { get; private set; }
        public UInt32 Info4 { get; private set; }
        public UInt32 Info5 { get; private set; }
        public UInt32 Info6 { get; private set; }
        public UInt32 OptimizedAddress { get; private set; }
        public UInt32 NonOptimizedAddress { get; private set; }
        public UInt32 RelationId { get; private set; }

        public UInt32 RetainSectionOffset { get; private set; }
        public UInt32 VolatileSectionOffset { get; private set; }

        public static S7OffsetInfoFbSfb Deserialize(Stream buffer)
        {
            S7OffsetInfoFbSfb result = new S7OffsetInfoFbSfb();

            result.Info1 = ReadUInt16LE(buffer);
            result.Info2 = ReadUInt16LE(buffer);

            result.OptimizedAddress = ReadUInt32LE(buffer);
            result.NonOptimizedAddress = ReadUInt32LE(buffer);
            result.RelationId = ReadUInt32LE(buffer);

            result.Info3 = ReadUInt32LE(buffer);
            result.Info4 = ReadUInt32LE(buffer);
            result.Info5 = ReadUInt32LE(buffer);
            result.Info6 = ReadUInt32LE(buffer);

            result.RetainSectionOffset = ReadUInt32LE(buffer);
            result.VolatileSectionOffset = ReadUInt32LE(buffer);

            return result;
        }
    }
}