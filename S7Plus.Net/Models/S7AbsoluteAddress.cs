#region License
/******************************************************************************
 * S7Plus.Net
 * 
 * Copyright (C) 2024 TKH Software GmbH, www.tkh-software.com
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
using System.Collections.Generic;
using System.IO;

namespace S7Plus.Net.Models
{
    public class S7AbsoluteAddress : IS7Address
    {
        private const UInt32 SYMBOL_CRC = 0;
        private const UInt32 ACCESS_AREA_OFFSET = 0x8a0e0000; // Data block access area offset
        private const UInt32 ACCESS_SUBAREA = S7Ids.DBValueActual;

        public UInt32 Datablock { get; }
        public List<UInt32> Offsets { get; } = new List<UInt32>();

        public S7AbsoluteAddress(UInt32 datablock, UInt32 offset)
        {
            Datablock = datablock;
            Offsets.Add(offset);
        }

        public uint FieldCount => (uint)Offsets.Count + 4;
        public uint AccessArea => Datablock + ACCESS_AREA_OFFSET;
        public uint AccessSubArea => ACCESS_SUBAREA;

        public int Serialize(Stream buffer)
        {
            int length = 0;
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, SYMBOL_CRC);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, Datablock + ACCESS_AREA_OFFSET);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)Offsets.Count + 1); // offset count + 1 (subarea)
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, ACCESS_SUBAREA);

            foreach (UInt32 offset in Offsets)
            {
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, offset);
            }

            return length;
        }
    }
}