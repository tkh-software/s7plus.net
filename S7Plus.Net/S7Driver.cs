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
using System.Threading.Tasks;

namespace S7Plus.Net
{
    public class S7Driver
    {
        private readonly S7Client _client = new S7Client();

        public void SetTimeout(TimeSpan timeout)
        {
            _client.SetTimeout(timeout);
        }

        public Task Connect(string host, int port)
        {
            return _client.Connect(host, port, TimeSpan.FromSeconds(5));
        }

        public Task Disconnect()
        {
            return _client.Disconnect();
        }
    }
}