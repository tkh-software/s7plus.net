﻿#region License
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
using TKH.S7Plus.Net.Helpers;
using TKH.S7Plus.Net.Models;
using TKH.S7Plus.Net.Models.OffsetInfos;
using TKH.S7Plus.Net.Requests;
using TKH.S7Plus.Net.Responses;
using TKH.S7Plus.Net.S7Variables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace TKH.S7Plus.Net.DriverExtensions
{
    public static class DriverExploreExtensions
    {
        private const UInt32 MARKER_TI_RID = 0x90030000;
        private const UInt32 OUTPUT_TI_RID = 0x90020000;
        private const UInt32 INPUT_TI_RID = 0x90010000;
        private const UInt32 TIMER_TI_RID = 0x90050000;
        private const UInt32 COUNTER_TI_RID = 0x90060000;

        public static async Task<List<Datablock>> GetDatablocks(this IS7Driver driver)
        {
            ExploreRequest request = new ExploreRequest(ProtocolVersion.V2)
            {
                ExploreId = S7Ids.NativeObjectsThePLCProgramRid,
                ExploreChildren = true,
                Attributes = { S7Ids.ObjectVariableTypeName }
            };

            S7VariableStruct filterData = new S7VariableStruct(S7Ids.Filter);
            filterData.AddMember(S7Ids.FilterOperation, new S7VariableDInt(8)); // InstanceOf
            filterData.AddMember(S7Ids.AddressCount, new S7VariableUDInt(0));
            UInt32[] filterAddress = new UInt32[32];
            filterData.AddMember(S7Ids.Address, new S7VariableUDIntArray(filterAddress));
            filterData.AddMember(S7Ids.FilterValue, new S7VariableRID(S7Ids.DBClass_Rid));
            request.FilterData = filterData;

            ExploreResponse response = await driver.Explore(request);

            List<Datablock> result = new List<Datablock>();
            List<IS7Address> addresses = new List<IS7Address>();

            foreach (S7Object obj in response.Objects)
            {
                if (obj.ClassId != S7Ids.DBClass_Rid)
                    continue;

                UInt32 area = obj.RelationId >> 16;
                if (area != Datablock.Area)
                    continue;

                if (!obj.Attributes.ContainsKey(S7Ids.ObjectVariableTypeName))
                    continue;

                S7VariableWString name = (S7VariableWString)obj.Attributes[S7Ids.ObjectVariableTypeName];
                Datablock db = new Datablock(obj.RelationId, obj.RelationId, 0, name.Value);
                result.Add(db);

                S7Address address = new S7Address(db.BlockRelId, S7Ids.DBValueActual);
                address.Offsets.Add(1);
                addresses.Add(address);
            }

            GetMultiVariablesRequest getMultiVariablesRequest = new GetMultiVariablesRequest(ProtocolVersion.V2, addresses);
            GetMultiVariablesResponse getMultiVariablesResponse = await driver.GetMultiVariables(getMultiVariablesRequest);

            foreach (var value in getMultiVariablesResponse.Values)
            {
                if (getMultiVariablesResponse.ErrorValues.ContainsKey(value.Key))
                    continue;

                Datablock? db = result.Find(d => d.BlockRelId == value.Key);
                if (db != null)
                    db.BlockTypeInfoRelId = ((S7VariableRID)value.Value).Value;
            }

            result.RemoveAll(db => db.BlockTypeInfoRelId == 0);

            return result;
        }

        public static async Task<VariableInfo?> GetVariableInfoBySymbol(this IS7Driver driver, string symbol, List<Datablock> datablocks)
        {
            string symbolRoot = SymbolParser.GetRootLevel(symbol);

            Datablock? db = datablocks.Find(d => d.BlockName == symbolRoot);
            if (db != null)
            {
                string accessSequence = db.BlockRelId.ToString("X");
                return await BrowseVariableInfoInternal(driver, db.BlockTypeInfoRelId, symbol, SymbolParser.GetNextLevel(symbol), accessSequence);
            }
            else
            {
                string accessSequence = S7Ids.NativeObjectsTheMAreaRid.ToString("X"); // Marker area
                VariableInfo? varInfo = await BrowseVariableInfoInternal(driver, MARKER_TI_RID, symbol, symbol, accessSequence);
                if (varInfo != null)
                    return varInfo;

                accessSequence = S7Ids.NativeObjectsTheQAreaRid.ToString("X"); // Output area
                varInfo = await BrowseVariableInfoInternal(driver, OUTPUT_TI_RID, symbol, symbol, accessSequence);
                if (varInfo != null)
                    return varInfo;

                accessSequence = S7Ids.NativeObjectsTheIAreaRid.ToString("X"); // Input area
                varInfo = await BrowseVariableInfoInternal(driver, INPUT_TI_RID, symbol, symbol, accessSequence);

                return varInfo;
            }
        }

        public static async Task<S7Object?> GetTypeInfoByRelId(this IS7Driver driver, UInt32 relId)
        {
            ExploreRequest request = new ExploreRequest(ProtocolVersion.V2)
            {
                ExploreId = relId,
                ExploreChildren = true,
            };

            ExploreResponse response = await driver.Explore(request);
            return response.Objects.Find(v => v.RelationId == relId);
        }

        public static async Task<List<VariableInfo>> BrowseAllVariables(this IS7Driver driver)
        {
            List<Datablock> dbs = await driver.GetDatablocks();

            ExploreResponse exploreResponse = await driver.Explore(new ExploreRequest(ProtocolVersion.V2)
            {
                ExploreId = S7Ids.ObjectOMSTypeInfoContainer,
                ExploreChildren = true
            });

            if (!exploreResponse.Objects.Any(v => v.ClassId == S7Ids.ClassOMSTypeInfoContainer))
                return new List<VariableInfo>();

            var typeInfos = exploreResponse.Objects.First(v => v.ClassId == S7Ids.ClassOMSTypeInfoContainer).Objects;

            VariableBrowser browser = new VariableBrowser(typeInfos.ToList());
            foreach (Datablock db in dbs)
            {
                browser.AddRootNode(db.BlockName, db.BlockRelId, db.BlockTypeInfoRelId);
            }

            browser.AddRootNode("I", S7Ids.NativeObjectsTheIAreaRid, INPUT_TI_RID);
            browser.AddRootNode("Q", S7Ids.NativeObjectsTheQAreaRid, OUTPUT_TI_RID);
            browser.AddRootNode("M", S7Ids.NativeObjectsTheMAreaRid, MARKER_TI_RID);
            browser.AddRootNode("S7Timers", S7Ids.NativeObjectsTheS7TimersRid, TIMER_TI_RID);
            browser.AddRootNode("S7Counters", S7Ids.NativeObjectsTheS7CountersRid, COUNTER_TI_RID);

            return browser.GetAllVariables();
        }

        private static async Task<VariableInfo?> BrowseVariableInfoInternal(IS7Driver driver, UInt32 relId, string fullSymbol, string symbol, string accessSequence,
            List<S7Object>? typeInfos = null)
        {
            if (typeInfos == null)
                typeInfos = new List<S7Object>();

            S7Object? typeInfo = typeInfos.Find(v => v.RelationId == relId);
            if (typeInfo == null)
                typeInfo = await driver.GetTypeInfoByRelId(relId);

            if (typeInfo == null)
                return null;

            typeInfos.Add(typeInfo);

            string rootLevel = SymbolParser.GetRootLevel(symbol);
            int varListIndex = typeInfo.VarNameList?.Names?.IndexOf(rootLevel) ?? -1;
            if (varListIndex < 0)
                return null;

            S7VarType varType = typeInfo.VarTypeList.Types[varListIndex];
            accessSequence += "." + varType.LID.ToString("X");

            if (varType.OffsetInfo.IsOneDimensional)
            {
                symbol = SymbolParser.GetNextLevel(symbol);
                accessSequence += AccessSeqCalculator.GetAccessSeqFor1DimArray(symbol, varType);
            }
            else if (varType.OffsetInfo.IsMultiDimensional)
            {
                symbol = SymbolParser.GetNextLevel(symbol);
                accessSequence += AccessSeqCalculator.GetAccessSeqForMDimArray(symbol, varType);
            }

            if (varType.OffsetInfo.HasRelation)
            {
                if (symbol.Length == 0)
                    return null;

                IOffsetInfoRelation offsetInfo = (IOffsetInfoRelation)varType.OffsetInfo;
                return await BrowseVariableInfoInternal(driver, offsetInfo.RelationId, fullSymbol, SymbolParser.GetNextLevel(symbol), accessSequence, typeInfos);
            }
            else
            {
                return new VariableInfo(fullSymbol, varType.SoftDatatype, new S7Address(accessSequence));
            }
        }
    }
}