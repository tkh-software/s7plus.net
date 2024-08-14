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

namespace TKH.S7Plus.Net.Constants
{
    [Flags]
    public enum PackedStructTransportFlag : UInt32
    {
        None = 0,
        ClassicNonoptimizedOffsets = 1 << 0,    // Is set when a struct is read from non-optimized datablock
        AlwaysSet = 1 << 1,                     // Is (so far) always set
        Count2Present = 1 << 10                 // If this bit is set, then there's 2nd counter present. Which if for a rare case you can read an array of struct, if the complete size, the 1st for one element.
    }
}