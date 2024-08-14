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
using System.IO;

namespace TKH.S7Plus.Net.Helpers
{
    public static class S7VlqValueDecoder
    {
        public static UInt32 DecodeUInt32Vlq(Stream buffer)
        {
            UInt32 value = 0;

            for (int counter = 1; counter <= 5; counter++)
            {
                byte octet = (byte)buffer.ReadByte();
                value <<= 7;
                byte cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                value += octet;
                if (cont == 0)
                {
                    break;
                }
            }

            return value;
        }

        public static Int32 DecodeInt32Vlq(Stream buffer)
        {
            Int32 value = 0;

            for (int counter = 1; counter <= 5; counter++)
            {
                byte octet = (byte)buffer.ReadByte();

                if ((counter == 1) && ((octet & 0x40) != 0))
                {
                    octet &= 0xbf;
                    value = -64;
                }
                else
                {
                    value <<= 7;
                }

                byte cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                value += octet;

                if (cont == 0)
                    break;
            }

            return value;
        }

        public static UInt64 DecodeUInt64Vlq(Stream buffer)
        {
            UInt64 value = 0;
            byte cont = 0;
            for (int counter = 1; counter <= 8; counter++)
            {
                byte octet = (byte)buffer.ReadByte();
                value <<= 7;
                cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                value += octet;

                if (cont == 0)
                    break;
            }

            if (cont > 0)
            {
                byte octet = (byte)buffer.ReadByte();
                value <<= 8;
                value += octet;
            }

            return value;
        }

        public static Int64 DecodeInt64Vlq(Stream buffer)
        {
            Int64 value = 0;
            byte cont = 0;

            for (int counter = 1; counter <= 8; counter++)
            {
                byte octet = (byte)buffer.ReadByte();

                if ((counter == 1) && ((octet & 0x40) != 0))
                {
                    octet &= 0xbf;
                    value = -64;
                }
                else
                {
                    value <<= 7;
                }
                cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                value += octet;
                if (cont == 0)
                {
                    break;
                }
            }

            if (cont > 0)
            {
                byte octet = (byte)buffer.ReadByte();

                value <<= 8;
                value += octet;
            }

            return value;
        }
    }
}