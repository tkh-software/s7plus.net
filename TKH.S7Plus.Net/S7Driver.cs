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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TKH.S7Plus.Net.Models;
using TKH.S7Plus.Net.Requests;
using TKH.S7Plus.Net.Responses;
using TKH.S7Plus.Net.S7Variables;

namespace TKH.S7Plus.Net
{
    public class S7Driver : IS7Driver, IDisposable
    {
        private readonly IS7Client _client;
        private readonly ILogger _logger;
        private bool _customClient;
        private SystemInfo _systemInfo = new SystemInfo();

        public S7Driver(ILogger? logger = null)
        {
            _logger = logger ?? new NullLogger<S7Driver>();
            _client = new S7Client(logger);
            _customClient = false;
        }

        public S7Driver(IS7Client customClient, ILogger? logger = null)
        {
            _logger = logger ?? new NullLogger<S7Driver>();
            _client = customClient;
            _customClient = true;
        }

        public bool IsConnected => _client.IsConnected;
        public bool IsConnecting => _client.IsConnecting;

        public SystemInfo SystemInfo => _systemInfo;

        public void SetTimeout(TimeSpan timeout)
        {
            _client.SetTimeout(timeout);
        }

        public void EnableAutoReconnect(bool enable, uint maxAttempts = 0, TimeSpan delay = default)
        {
            _client.EnableAutoReconnect(enable, maxAttempts, delay);
        }

        public async Task Connect(string host, int port)
        {
            await _client.Connect(host, port);

            try
            {
                await ReadSystemInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read system info");
            }
        }

        public void Disconnect()
        {
            _client.Disconnect();
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

        private async Task ReadSystemInfo()
        {
            GetMultiVariablesRequest request = new GetMultiVariablesRequest(new List<IS7Address>
            {
                _systemInfo.MaxReadSystemInfoAddress,
                _systemInfo.MaxWriteSystemInfoAddress
            });

            GetMultiVariablesResponse response = await GetMultiVariables(request);

            int maxRead = ((S7VariableDInt)response.Values.First().Value).Value;
            int maxWrite = ((S7VariableDInt)response.Values.Skip(1).First().Value).Value;

            _systemInfo = new SystemInfo(maxRead, maxWrite);
        }

        public void Dispose()
        {
            if (_customClient)
                return;

            if (_client is IDisposable disposable)
                disposable.Dispose();
        }
    }
}