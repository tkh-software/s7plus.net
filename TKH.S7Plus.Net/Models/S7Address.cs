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
using System.Linq;

namespace TKH.S7Plus.Net.Models
{
    public class S7Address : IS7Address
    {
        private const UInt32 SYMBOL_CRC = 0;
        private const UInt32 DB_ACCESS_AREA = 0x8A0E0000;

        public S7Address()
        {
        }

        public S7Address(UInt32 area, UInt32 subArea)
        {
            AccessArea = area;
            AccessSubArea = subArea;
        }

        public S7Address(string accessString)
        {
            List<UInt32> ids = new List<UInt32>();
            foreach (string p in accessString.Split('.'))
            {
                ids.Add(UInt32.Parse(p, System.Globalization.NumberStyles.HexNumber));
            }

            if (ids.Count < 2)
            {
                throw new ArgumentException("Invalid access string");
            }

            AccessArea = ids[0];
            if (AccessArea >= DB_ACCESS_AREA)
            {
                AccessSubArea = S7Ids.DBValueActual;
            }
            else if (
                AccessArea == S7Ids.NativeObjectsTheS7TimersRid ||
                AccessArea == S7Ids.NativeObjectsTheS7CountersRid ||
                AccessArea == S7Ids.NativeObjectsTheIAreaRid ||
                AccessArea == S7Ids.NativeObjectsTheQAreaRid ||
                AccessArea == S7Ids.NativeObjectsTheMAreaRid)
            {
                AccessSubArea = S7Ids.ControllerArea_ValueActual;
            }

            foreach (var i in ids.Skip(1))
            {
                Offsets.Add(i);
            }
        }

        public UInt32 AccessArea { get; set; }
        public UInt32 AccessSubArea { get; set; }
        public uint FieldCount => (uint)Offsets.Count + 4;
        public List<UInt32> Offsets { get; } = new List<UInt32>();

        public int Serialize(Stream buffer)
        {
            int length = 0;
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, SYMBOL_CRC);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, AccessArea);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)Offsets.Count + 1); // offset count + 1 (subarea)
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, AccessSubArea);

            foreach (UInt32 offset in Offsets)
            {
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, offset);
            }

            return length;
        }
    }
}