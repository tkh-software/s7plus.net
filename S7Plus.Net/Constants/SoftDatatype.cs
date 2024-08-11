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
    public static class SoftDatatype
    {
        public const uint Void = 0;
        public const uint Bool = 1;
        public const uint Byte = 2;
        public const uint Char = 3;
        public const uint Word = 4;
        public const uint Int = 5;
        public const uint Dword = 6;
        public const uint Dint = 7;
        public const uint Real = 8;
        public const uint Date = 9;
        public const uint TimeOfDay = 10;
        public const uint Time = 11;
        public const uint S5Time = 12;
        public const uint S5Count = 13;
        public const uint DateAndTime = 14;
        public const uint InternetTime = 15;
        public const uint Array = 16;
        public const uint Struct = 17;
        public const uint EndStruct = 18;
        public const uint String = 19;
        public const uint Pointer = 20;
        public const uint MultiFb = 21;
        public const uint Any = 22;
        public const uint BlockFb = 23;
        public const uint BlockFc = 24;
        public const uint BlockDb = 25;
        public const uint BlockSdb = 26;
        public const uint MultiSfb = 27;
        public const uint Counter = 28;
        public const uint Timer = 29;
        public const uint IecCounter = 30;
        public const uint IecTimer = 31;
        public const uint BlockSfb = 32;
        public const uint BlockSfc = 33;
        public const uint BlockCb = 34;
        public const uint BlockScb = 35;
        public const uint BlockOb = 36;
        public const uint BlockUdt = 37;
        public const uint Offset = 38;
        public const uint BlockSdt = 39;
        public const uint BBool = 40;
        public const uint BlockExt = 41;
        public const uint LReal = 48;
        public const uint Ulint = 49;
        public const uint Lint = 50;
        public const uint Lword = 51;
        public const uint Usint = 52;
        public const uint Uint = 53;
        public const uint Udint = 54;
        public const uint Sint = 55;
        public const uint Bcd8 = 56;
        public const uint Bcd16 = 57;
        public const uint Bcd32 = 58;
        public const uint Bcd64 = 59;
        public const uint Aref = 60;
        public const uint Wchar = 61;
        public const uint Wstring = 62;
        public const uint Variant = 63;
        public const uint LTime = 64;
        public const uint LTod = 65;
        public const uint Ldt = 66;
        public const uint Dtl = 67;
        public const uint IecLTimer = 68;
        public const uint IecSCounter = 69;
        public const uint IecDCounter = 70;
        public const uint IecLCounter = 71;
        public const uint IecUCounter = 72;
        public const uint IecUSCounter = 73;
        public const uint IecUDCounter = 74;
        public const uint IecULCounter = 75;
        public const uint Remote = 96;
        public const uint ErrorStruct = 97;
        public const uint Nref = 98;
        public const uint Vref = 99;
        public const uint FbtRef = 100;
        public const uint Cref = 101;
        public const uint VaRef = 102;
        public const uint AomIdent = 128;
        public const uint EventAny = 129;
        public const uint EventAtt = 130;
        public const uint EventHwInt = 131;
        public const uint Folder = 132;
        public const uint AomAid = 133;
        public const uint AomLink = 134;
        public const uint HwAny = 144;
        public const uint HwIoSystem = 145;
        public const uint HwDpMaster = 146;
        public const uint HwDevice = 147;
        public const uint HwDpSlave = 148;
        public const uint HwIo = 149;
        public const uint HwModule = 150;
        public const uint HwSubModule = 151;
        public const uint HwHsc = 152;
        public const uint HwPwm = 153;
        public const uint HwPto = 154;
        public const uint HwInterface = 155;
        public const uint HwIePort = 156;
        public const uint ObAny = 160;
        public const uint ObDelay = 161;
        public const uint ObTod = 162;
        public const uint ObCyclic = 163;
        public const uint ObAtt = 164;
        public const uint ConnAny = 168;
        public const uint ConnPrg = 169;
        public const uint ConnOuc = 170;
        public const uint ConnRid = 171;
        public const uint HwNr = 172;
        public const uint Port = 173;
        public const uint Rtm = 174;
        public const uint Pip = 175;
        public const uint CAlarm = 176;
        public const uint CAlarms = 177;
        public const uint CAlarm8 = 178;
        public const uint CAlarm8P = 179;
        public const uint CAlarmT = 180;
        public const uint CarSend = 181;
        public const uint CNotify = 182;
        public const uint CNotify8P = 183;
        public const uint ObPCycle = 192;
        public const uint ObHwInt = 193;
        public const uint ObComm = 194;
        public const uint ObDiag = 195;
        public const uint ObTimeError = 196;
        public const uint ObStartup = 197;
        public const uint OpcUaLocTxtEncM = 200;
        public const uint OpcUaStrActLen = 201;
        public const uint DbAny = 208;
        public const uint DbWww = 209;
        public const uint DbDyn = 210;
        public const uint Para = 253;
        public const uint Label = 254;
        public const uint UDefined = 255;
        public const uint NotChosen = 256;
    }
}