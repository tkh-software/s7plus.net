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
using S7Plus.Net.Models;
using S7Plus.Net.Models.OffsetInfos;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace S7Plus.Net.Helpers
{
    public static class AccessSeqCalculator
    {
        public static string GetAccessSeqFor1DimArray(string symbol, S7VarType type)
        {
            Regex re = new Regex(@"^\[(\d+)\]");
            Match m = re.Match(symbol);
            if (!m.Success)
                throw new Exception("Symbol syntax error");

            int arrayIndex = int.Parse(m.Groups[1].Value);

            IOffsetInfo1Dim offsetInfo = (IOffsetInfo1Dim)type.OffsetInfo;

            if (arrayIndex - offsetInfo.ArrayLowerBounds > offsetInfo.ArrayElementCount)
                throw new Exception("Out of bounds");
            if (arrayIndex < offsetInfo.ArrayLowerBounds)
                throw new Exception("Out of bounds");

            string result = "." + String.Format("{0:X}", arrayIndex - offsetInfo.ArrayLowerBounds);
            if (offsetInfo.HasRelation)
                result += ".1"; // additional ".1" for array of struct

            return result;
        }

        public static string GetAccessSeqForMDimArray(string symbol, S7VarType type)
        {
            Regex re = new Regex(@"^\[([0-9, ]+)\]");
            Match m = re.Match(symbol);
            if (!m.Success)
                throw new Exception("Symbol syntax error");

            string idxs = m.Groups[1].Value.Replace(" ", "");

            uint[] indexes = Array.ConvertAll(idxs.Split(','), e => uint.Parse(e));
            IOffsetInfoMDim offsetInfo = (IOffsetInfoMDim)type.OffsetInfo;

            uint[] mDimArrayElementCount = (uint[])offsetInfo.MDimElementCounts.Clone();
            int[] mDimArrayLowerBounds = offsetInfo.MDimLowerBounds;

            // check dim count
            int dimCount = mDimArrayElementCount.Aggregate(0, (acc, act) => acc += (act > 0) ? 1 : 0);
            if (dimCount != indexes.Count())
                throw new Exception("Out of bounds");

            // check bounds
            for (int i = 0; i < dimCount; ++i)
            {
                indexes[i] = (uint)(indexes[i] - mDimArrayLowerBounds[dimCount - i - 1]);

                if (indexes[i] > mDimArrayElementCount[dimCount - i - 1])
                    throw new Exception("Out of bounds");

                if (indexes[i] < 0)
                    throw new Exception("Out of bounds");
            }

            // calc dim size
            if (type.SoftDatatype == SoftDatatype.BBool)
            {
                mDimArrayElementCount[0] += 8 - mDimArrayElementCount[0] % 8; // for bool must be a mutiple of 8!
            }

            uint[] dimSize = new uint[dimCount];
            uint g = 1;
            for (int i = 0; i < dimCount - 1; ++i)
            {
                dimSize[i] = g;
                g *= mDimArrayElementCount[i];
            }
            dimSize[dimCount - 1] = g;

            // calc id
            uint arrayIndex = 0;
            for (int i = 0; i < dimCount; ++i)
            {
                arrayIndex += indexes[i] * dimSize[dimCount - i - 1];
            }

            string result = "." + arrayIndex.ToString("X");
            if (offsetInfo.HasRelation)
                result += ".1"; // additional ".1" for array of struct

            return result;
        }
    }
}