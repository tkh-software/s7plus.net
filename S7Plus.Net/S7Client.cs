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
    public class S7Client : IDisposable
    {
        private readonly TcpClient _client;
        private NetworkStream _stream;
        private readonly ConcurrentDictionary<UInt64, TaskCompletionSource<byte[]>> _requests;
        private UInt64 _sequenceNumber;
        private CancellationTokenSource _cts;
        private Task _receiveTask;

        public S7Client()
        {
            _client = new TcpClient();

            _requests = new ConcurrentDictionary<UInt64, TaskCompletionSource<byte[]>>();
            _sequenceNumber = 0;
        }

        public void SetTimeout(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout is too large");

            if (timeout.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout is negative");
        }

        public async Task ConnectAsync(string host, int port, TimeSpan timeout)
        {
            if (_stream != null && _stream.CanRead)
                return;

            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    await _client.ConnectAsync(host, port).WaitAsync(cts.Token);
                    _stream = _client.GetStream();
                    _cts = new CancellationTokenSource();

                    // Start the receive loop
                    _receiveTask = Task.Run(() => ReceiveLoop(_cts.Token));
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException("The connection attempt timed out.");
                }
            }
        }

        private async Task SendIsoOnTcpConnectionRequestAsync()
        {
            // ISO-on-TCP connection request
            byte[] isoConnectionRequest = new byte[]
            {
                0x03, 0x00, 0x00, 0x16, // TPKT Header
                0x11, 0xE0, 0x00, 0x00, // COTP Connection Request
                0x00, 0x01, 0x00, 0xC1, 0x02, 0x01, 0x00, // Source TSAP
                0xC2, 0x02, 0x01, 0x02, // Destination TSAP
                0x01, 0x00 // 2 bytes for TPDU size
            };

            await _stream.WriteAsync(isoConnectionRequest, 0, isoConnectionRequest.Length);
        }

        public async Task DisconnectAsync()
        {
            if (_stream == null || !_stream.CanRead)
                return;

            _cts.Cancel();
            try
            {
                await _receiveTask;
            }
            catch (OperationCanceledException) { }

            _stream.Close();
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

        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_stream.DataAvailable)
                    {
                        var header = new byte[7];
                        await _stream.ReadAsync(header, 0, header.Length, token);

                        var length = (header[5] << 8) | header[6];
                        var buffer = new byte[length - 7];
                        await _stream.ReadAsync(buffer, 0, buffer.Length, token);

                        var sequenceNumber = BitConverter.ToUInt64(buffer, 0);

                        if (_requests.TryRemove(sequenceNumber, out var tcs))
                        {
                            var response = new byte[buffer.Length - 8];
                            Array.Copy(buffer, 8, response, 0, response.Length);
                            tcs.SetResult(response);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMicroseconds(10), token);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
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