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
    public static class ElementId
    {
        public const byte StartOfObject = 0xa1;
        public const byte TerminatingObject = 0xa2;
        public const byte Attribute = 0xa3;
        public const byte Relation = 0xa4;
        public const byte StartOfTagDescription = 0xa7;
        public const byte TerminatingTagDescription = 0xa8;
        public const byte VartypeList = 0xab;
        public const byte VarnameList = 0xac;
    }
}