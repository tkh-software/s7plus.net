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

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace S7Plus.Net
{
    internal class S7Client : IDisposable
    {
        private readonly TcpClient _client;
        private readonly SslStream _stream;
        private readonly ConcurrentDictionary<UInt64, TaskCompletionSource<byte[]>> _requests;
        private UInt64 _sequenceNumber;
        private Thread _receiveThread;
        private bool _running;

        public S7Client()
        {
            _client = new TcpClient();
            _stream = new SslStream(_client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => true);
            _requests = new ConcurrentDictionary<UInt64, TaskCompletionSource<byte[]>>();
            _sequenceNumber = 0;
        }

        public async Task ConnectAsync(string host, int port)
        {
            if (_running)
                return;

            await _client.ConnectAsync(host, port);
            _stream.AuthenticateAsClient(host);
            _running = true;
            _receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true
            };
            _receiveThread.Start();
        }

        public async Task DisconnectAsync()
        {
            if (!_running)
                return;

            _running = false;
            _receiveThread.Join();
            await _stream.FlushAsync();
            _client.Close();
            _requests.Clear();
        }

        public async Task<byte[]> SendAsync(byte[] request)
        {
            var sequenceNumber = _sequenceNumber++;
            var tcs = new TaskCompletionSource<byte[]>();
            _requests.TryAdd(sequenceNumber, tcs);

            var buffer = new MemoryStream();
            AddIsoOnTcpHeader(buffer, (uint)request.Length + 4, sequenceNumber);
            buffer.Write(request, 0, request.Length);
            await _stream.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length);

            return await tcs.Task;
        }

        private void ReceiveLoop()
        {
            try
            {
                while (_running)
                {
                    var header = new byte[7];
                    _stream.Read(header, 0, header.Length);

                    var length = (header[5] << 8) | header[6];
                    var buffer = new byte[length - 7];
                    _stream.Read(buffer, 0, buffer.Length);

                    var sequenceNumber = BitConverter.ToUInt64(buffer, 0);

                    if (_requests.TryRemove(sequenceNumber, out var tcs))
                    {
                        var response = new byte[buffer.Length - 8];
                        Array.Copy(buffer, 8, response, 0, response.Length);
                        tcs.SetResult(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReceiveLoop exception: {ex.Message}");
            }
        }

        private void AddIsoOnTcpHeader(Stream stream, uint length, UInt64 sequenceNumber)
        {
            stream.WriteByte(0x03); // TPKT version
            stream.WriteByte(0x00); // Reserved
            stream.WriteByte((byte)((length + 7) >> 8)); // Length high byte
            stream.WriteByte((byte)((length + 7) & 0xFF)); // Length low byte
            stream.WriteByte(0x02); // ISO 8073:1986 COTP Header Length
            stream.WriteByte(0xF0); // ISO 8073:1986 PDU Type (Data)
            stream.WriteByte(0x80); // Last byte of COTP header

            var sequenceBytes = BitConverter.GetBytes(sequenceNumber);
            stream.Write(sequenceBytes, 0, sequenceBytes.Length);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}