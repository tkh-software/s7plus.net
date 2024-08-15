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
using System;
using System.IO;

namespace TKH.S7Plus.Net.Models.OffsetInfos
{
    public abstract class S7OffsetInfo
    {
        public abstract bool HasRelation { get; }
        public abstract bool IsOneDimensional { get; }
        public abstract bool IsMultiDimensional { get; }

        public static S7OffsetInfo Deserialize(Stream buffer, int type)
        {
            switch (type)
            {
                case OffsetInfoType.FbArray:
                    return S7OffsetInfoFbArray.Deserialize(buffer);
                case OffsetInfoType.StructElemStd:
                case OffsetInfoType.Std:
                    return S7OffsetInfoStd.Deserialize(buffer, type);
                case OffsetInfoType.StructElemString:
                case OffsetInfoType.String:
                    return S7OffsetInfoString.Deserialize(buffer);
                case OffsetInfoType.StructElemArray1Dim:
                case OffsetInfoType.Array1Dim:
                    return S7OffsetInfoArray1Dim.Deserialize(buffer);
                case OffsetInfoType.StructElemArrayMDim:
                case OffsetInfoType.ArrayMDim:
                    return S7OffsetInfoArrayMDim.Deserialize(buffer);
                case OffsetInfoType.StructElemStruct:
                case OffsetInfoType.Struct:
                    return S7OffsetInfoStruct.Deserialize(buffer);
                case OffsetInfoType.StructElemStruct1Dim:
                case OffsetInfoType.Struct1Dim:
                    return S7OffsetInfoStruct1Dim.Deserialize(buffer);
                case OffsetInfoType.StructElemStructMDim:
                case OffsetInfoType.StructMDim:
                    return S7OffsetInfoStructMDim.Deserialize(buffer);
                case OffsetInfoType.FbSfb:
                    return S7OffsetInfoFbSfb.Deserialize(buffer);
                default:
                    throw new Exception("Unknown offset info type");
            }
        }

        protected static UInt16 ReadUInt16LE(Stream buffer)
        {
            byte[] data = new byte[2];
            buffer.Read(data, 0, 2);
            return BitConverter.ToUInt16(data, 0);
        }

        protected static UInt32 ReadUInt32LE(Stream buffer)
        {
            byte[] data = new byte[4];
            buffer.Read(data, 0, 4);
            return BitConverter.ToUInt32(data, 0);
        }

        protected static Int32 ReadInt32LE(Stream buffer)
        {
            byte[] data = new byte[4];
            buffer.Read(data, 0, 4);
            return BitConverter.ToInt32(data, 0);
        }
    }
}