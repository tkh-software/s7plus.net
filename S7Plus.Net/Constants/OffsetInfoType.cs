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

namespace S7Plus.Net.Constants
{
    public static class OffsetInfoType
    {
        public const int FbArray = 0;
        public const int StructElemStd = 1;
        public const int StructElemString = 2;
        public const int StructElemArray1Dim = 3;
        public const int StructElemArrayMDim = 4;
        public const int StructElemStruct = 5;
        public const int StructElemStruct1Dim = 6;
        public const int StructElemStructMDim = 7;
        public const int Std = 8;
        public const int String = 9;
        public const int Array1Dim = 10;
        public const int ArrayMDim = 11;
        public const int Struct = 12;
        public const int Struct1Dim = 13;
        public const int StructMDim = 14;
        public const int FbSfb = 15;
    }
}