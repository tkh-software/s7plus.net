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
using TKH.S7Plus.Net.Models.OffsetInfos;
using System;
using System.IO;

namespace TKH.S7Plus.Net.Models
{
    public class S7VarType
    {
        //flags in tag description for S7-1500
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_OFFSETINFOTYPE = 0xf000;      // Bits 13..16
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_HMIVISIBLE = 0x0800;          // Bit 12
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_HMIREADONLY = 0x0400;         // Bit 11
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_HMIACCESSIBLE = 0x0200;       // Bit 10
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_BIT09 = 0x0100;               // Bit 09
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_OPTIMIZEDACCESS = 0x0080;     // Bit 08
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_SECTION = 0x0070;             // Bits 05..07
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_BIT04 = 0x0008;               // Bit 04
        private const int S7COMMP_TAGDESCR_ATTRIBUTE2_BITOFFSET = 0x0007;           // Bits 01..03

        const int S7COMMP_TAGDESCR_BITOFFSETINFO_RETAIN = 0x80;
        const int S7COMMP_TAGDESCR_BITOFFSETINFO_NONOPTBITOFFSET = 0x70;
        const int S7COMMP_TAGDESCR_BITOFFSETINFO_CLASSIC = 0x08;
        const int S7COMMP_TAGDESCR_BITOFFSETINFO_OPTBITOFFSET = 0x07;

        public UInt32 LID { get; private set; }
        public UInt32 SymbolCrc { get; private set; }
        public byte SoftDatatype { get; private set; }
        public UInt16 AttributeFlags { get; private set; }
        public byte BitOffsetInfoFlags { get; private set; }

        public int Section => (AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_SECTION) >> 4;
        public int BitOffset => AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_BITOFFSET;
        public bool HmiVisible => (AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_HMIVISIBLE) != 0;
        public bool HmiAccessible => (AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_HMIACCESSIBLE) != 0;
        public bool OptimizedAccess => (AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_OPTIMIZEDACCESS) != 0;
        public bool HmiReadOnly => (AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_HMIREADONLY) != 0;
        public bool BitOffsetInfoRetain => (BitOffsetInfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_RETAIN) != 0;
        public bool BitOffsetInfoClassic => (BitOffsetInfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_CLASSIC) != 0;
        public int BitOffsetInfoNonOptimizedBitOffset => (BitOffsetInfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_NONOPTBITOFFSET) >> 4;
        public int BitOffsetInfoOptimizedBitOffset => BitOffsetInfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_OPTBITOFFSET;
        public int OffsetInfoType => (AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_OFFSETINFOTYPE) >> 12;

        public S7OffsetInfo OffsetInfo { get; private set; }

        public static S7VarType Deserialize(Stream buffer)
        {
            S7VarType result = new S7VarType
            {
                LID = S7ValueDecoder.DecodeUInt32LE(buffer),
                SymbolCrc = S7ValueDecoder.DecodeUInt32LE(buffer),
                SoftDatatype = S7ValueDecoder.DecodeByte(buffer),
                AttributeFlags = S7ValueDecoder.DecodeUInt16(buffer),
                BitOffsetInfoFlags = S7ValueDecoder.DecodeByte(buffer)
            };

            result.OffsetInfo = S7OffsetInfo.Deserialize(buffer, result.OffsetInfoType);

            return result;
        }
    }
}