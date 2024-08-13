﻿#region License
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using S7Plus.Net.Constants;
using S7Plus.Net.Models;
using S7Plus.Net.Requests;
using S7Plus.Net.Responses;
using S7Plus.Net.S7Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S7Plus.Net
{
    public class S7Driver : IS7Driver
    {
        private readonly ILogger _logger;
        private readonly S7Client _client;

        public S7Driver(ILogger logger = null)
        {
            _client = new S7Client(logger);
            _logger = logger ?? new NullLogger<S7Driver>();
        }

        public ILogger Logger => _logger;

        public void SetTimeout(TimeSpan timeout)
        {
            _client.SetTimeout(timeout);
        }

        public Task Connect(string host, int port)
        {
            return _client.Connect(host, port);
        }

        public Task Disconnect()
        {
            return _client.Disconnect();
        }

        public async Task<GetMultiVariablesResponse> GetMultiVariables(GetMultiVariablesRequest request)
        {
            byte[] buffer = await _client.Send(request);
            using MemoryStream stream = new MemoryStream(buffer);
            return GetMultiVariablesResponse.Deserialize(stream);
        }

        public async Task<SetMultiVariablesResponse> SetMultiVariables(SetMultiVariablesRequest request)
        {
            byte[] buffer = await _client.Send(request);
            using MemoryStream stream = new MemoryStream(buffer);
            return SetMultiVariablesResponse.Deserialize(stream);
        }

        public async Task<ExploreResponse> Explore(ExploreRequest request)
        {
            byte[] buffer = await _client.Send(request);
            using MemoryStream stream = new MemoryStream(buffer);
            return ExploreResponse.Deserialize(stream);
        }
    }
}