#region License
/******************************************************************************
 * S7Plus.Net
 * 
 * Copyright (C) 2024 TKH Software GmbH, www.tkh-software.com
 *
 * This file is part of the S7Plus.Net project.
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
using System.Threading.Tasks;
using TKH.S7Plus.Net.Requests;

namespace TKH.S7Plus.Net
{
    public interface IS7Client
    {
        bool IsConnected { get; }
        bool IsConnecting { get; }

        Task Connect(string host, int port);
        void Disconnect();
        void EnableAutoReconnect(bool enable, uint maxAttempts = 0, TimeSpan delay = default);
        Task<byte[]> Send(IS7Request request);
        void SetTimeout(TimeSpan timeout);
    }
}