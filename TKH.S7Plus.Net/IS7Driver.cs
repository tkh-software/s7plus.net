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

using TKH.S7Plus.Net.Requests;
using TKH.S7Plus.Net.Responses;
using System;
using System.Threading.Tasks;
using TKH.S7Plus.Net.Models;

namespace TKH.S7Plus.Net
{
    public interface IS7Driver
    {
        void SetTimeout(TimeSpan timeout);
        Task Connect(string host, int port);
        void Disconnect();

        bool IsConnected { get; }
        SystemInfo SystemInfo { get; }

        Task<GetMultiVariablesResponse> GetMultiVariables(GetMultiVariablesRequest request);
        Task<SetMultiVariablesResponse> SetMultiVariables(SetMultiVariablesRequest request);
        Task<ExploreResponse> Explore(ExploreRequest request);
    }
}