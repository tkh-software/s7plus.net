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
    public static class Datatype
    {
        public const byte Null = 0x00;
        public const byte Bool = 0x01;
        public const byte USInt = 0x02;
        public const byte UInt = 0x03;
        public const byte UDInt = 0x04;
        public const byte ULInt = 0x05;
        public const byte SInt = 0x06;
        public const byte Int = 0x07;
        public const byte DInt = 0x08;
        public const byte LInt = 0x09;
        public const byte Byte = 0x0a;
        public const byte Word = 0x0b;
        public const byte DWord = 0x0c;
        public const byte LWord = 0x0d;
        public const byte Real = 0x0e;
        public const byte LReal = 0x0f;
        public const byte Timestamp = 0x10;
        public const byte Timespan = 0x11;
        public const byte RID = 0x12;
        public const byte AID = 0x13;
        public const byte Blob = 0x14;
        public const byte WString = 0x15;
        public const byte Variant = 0x16;
        public const byte Struct = 0x17;
        public const byte S7String = 0x19;
    }
}