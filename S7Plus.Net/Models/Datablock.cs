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

using System;

namespace S7Plus.Net.Models
{
    public record Datablock
    {
        public Datablock()
        {
        }

        public Datablock(UInt32 blockNumber, UInt32 blockRelId, UInt32 blockTiRelId, string blockName)
        {
            BlockNumber = blockNumber;
            BlockRelId = blockRelId;
            BlockTiRelId = blockTiRelId;
            BlockName = blockName;
        }

        public UInt32 BlockNumber { get; set; }
        public UInt32 BlockRelId { get; set; }
        public UInt32 BlockTiRelId { get; set; }
        public string BlockName { get; set; } = string.Empty;
    }
}