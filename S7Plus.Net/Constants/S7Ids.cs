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
    public static class S7Ids
    {
        public const int None = 0;
        public const int NativeObjectsThePLCProgramRid = 3;
        public const int NativeObjectsTheCPUExecUnitRid = 52;
        public const int NativeObjectsTheIArea_Rid = 80;
        public const int NativeObjectsTheQAreaRid = 81;
        public const int NativeObjectsTheMAreaRid = 82;
        public const int NativeObjectsTheS7CountersRid = 83;
        public const int NativeObjectsTheS7TimersRid = 84;
        public const int ObjectRoot = 201;
        public const int GetNewRIDOnServer = 211;
        public const int ObjectVariableTypeParentObject = 229;
        public const int ObjectVariableTypeName = 233;
        public const int ClassSubscriptions = 255;
        public const int ClassServerSessionContainer = 284;
        public const int ObjectServerSessionContainer = 285;
        public const int ClassServerSession = 287;
        public const int ObjectNullServerSession = 288;
        public const int ServerSessionClientRID = 300;
        public const int ServerSessionVersion = 306;
        public const int ClassTypeInfo = 511;
        public const int ClassOMSTypeInfoContainer = 534;
        public const int ObjectOMSTypeInfoContainer = 537;
        public const int TextLibraryClassRID = 606;
        public const int TextLibraryOffsetArea = 608;
        public const int TextLibraryStringArea = 609;
        public const int ClassSubscription = 1001;
        public const int SubscriptionMissedSendings = 1002;
        public const int SubscriptionSubsystemError = 1003;
        public const int SubscriptionReferenceTriggerAndTransmitMode = 1005;
        public const int SystemLimits = 1037;
        public const int SubscriptionRouteMode = 1040;
        public const int SubscriptionActive = 1041;
        public const int SubscriptionReferenceList = 1048;
        public const int SubscriptionCycleTime = 1049;
        public const int SubscriptionDelayTime = 1050;
        public const int SubscriptionDisabled = 1051;
        public const int SubscriptionCount = 1052;
        public const int SubscriptionCreditLimit = 1053;
        public const int SubscriptionTicks = 1054;
        public const int FreeItems = 1081;
        public const int SubscriptionFunctionClassId = 1082;
        public const int Filter = 1246;
        public const int FilterOperation = 1247;
        public const int AddressCount = 1249;
        public const int Address = 1250;
        public const int FilterValue = 1251;
        public const int ObjectQualifier = 1256;
        public const int ParentRID = 1257;
        public const int CompositionAID = 1258;
        public const int KeyQualifier = 1259;
        public const int EffectiveProtectionLevel = 1842;
        public const int ActiveProtectionLevel = 1843;
        public const int CPUexecUnitOperatingStateReq = 2167;
        public const int PLCProgramClassRid = 2520;
        public const int BlockNumber = 2521;
        public const int DBValueInitial = 2548;
        public const int DBValueActual = 2550;
        public const int DBInitialChanged = 2551;
        public const int DBClass_Rid = 2574;
        public const int AlarmSubscriptionRefAlarmDomain = 2659;
        public const int AlarmSubscriptionRefItsAlarmSubsystem = 2660;
        public const int AlarmSubscriptionRefClassRid = 2662;
        public const int DAICPUAlarmID = 2670;
        public const int DAIAllStatesInfo = 2671;
        public const int DAIAlarmDomain = 2672;
        public const int DAIComing = 2673;
        public const int DAIGoing = 2677;
        public const int DAIClassRid = 2681;
        public const int DAIAlarmTextsRid = 2715;
        public const int ASCGSAllStatesInfo = 3474;
        public const int ASCGSTimestamp = 3475;
        public const int ASCGSAssociatedValues = 3476;
        public const int ASCGSAckTimestamp = 3646;
        public const int ControllerArea_ValueInitial = 3735;
        public const int ControllerArea_ValueActual = 3736;
        public const int ControllerArea_RuntimeModified = 3737;
        public const int DAIMessageType = 4079;
        public const int ASObjectESComment = 4288;
        public const int AlarmSubscriptionRefAlarmDomain2 = 7731;
        public const int DAIHmiInfo = 7813;
        public const int MultipleSTAIClassRid = 7854;
        public const int MultipleSTAISTAIs = 7859;
        public const int DAISequenceCounter = 7917;
        public const int AlarmSubscriptionRefAlarmTextLanguagesRid = 8181;
        public const int AlarmSubscriptionRefSendAlarmTextsRid = 8173;
        public const int ReturnValue = 40305;

        public const int TI_BOOL = 0x02000000 + 1;
        public const int TI_BYTE = 0x02000000 + 2;
        public const int TI_CHAR = 0x02000000 + 3;
        public const int TI_WORD = 0x02000000 + 4;
        public const int TI_INT = 0x02000000 + 5;
        public const int TI_DWORD = 0x02000000 + 6;
        public const int TI_DINT = 0x02000000 + 7;
        public const int TI_REAL = 0x02000000 + 8;
        public const int TI_STRING = 0x02000000 + 19;
        public const int TI_LREAL = 0x02000000 + 48;
        public const int TI_USINT = 0x02000000 + 52;
        public const int TI_UINT = 0x02000000 + 53;
        public const int TI_UDINT = 0x02000000 + 54;
        public const int TI_SINT = 0x02000000 + 55;
        public const int TI_WCHAR = 0x02000000 + 61;
        public const int TI_WSTRING = 0x02000000 + 62;
        public const int TI_STRING_START = 0x020a0000;  // Start for String[0]
        public const int TI_STRING_END = 0x020affff;    // End (String[65535])
        public const int TI_WSTRING_START = 0x020b0000; // Start for WString[0]
        public const int TI_WSTRING_END = 0x020bffff;   // End (WString[65535])
    }
}