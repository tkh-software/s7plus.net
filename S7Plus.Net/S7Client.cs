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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using S7Plus.Net.Constants;
using S7Plus.Net.Requests;
using S7Plus.Net.Responses;
using S7Plus.Net.S7Variables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace S7Plus.Net
{
    public class S7Client : IDisposable
    {
        private const int MAX_CONNECTION_ATTEMPTS = 5;
        private const UInt32 SESSION_CLIENT_RID = 0x80c3c901;

        private TcpClient _client;
        private CotpNetworkStream _stream;
        private readonly ConcurrentDictionary<UInt64, TaskCompletionSource<byte[]>> _requests;
        private UInt16 _sequenceNumber;
        private UInt32 _integrityId = 0;
        private UInt32 _integrityIdSet = 0;
        private CancellationTokenSource _cts;
        private Task _receiveTask;
        private string _host;
        private int _port;
        private TimeSpan _timeout;

        private readonly ILogger<S7Client> _logger;

        private uint _sessionId = S7Ids.ObjectNullServerSession;
        private uint _sessionId2 = S7Ids.ObjectNullServerSession;

        public S7Client(ILogger<S7Client> logger = null)
        {
            _requests = new ConcurrentDictionary<UInt64, TaskCompletionSource<byte[]>>();
            _sequenceNumber = 0;
            _logger = logger ?? new NullLogger<S7Client>();
        }

        public void SetTimeout(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout is too large");

            if (timeout.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout is negative");
        }

        public async Task Connect(string host, int port, TimeSpan timeout)
        {
            _host = host;
            _port = port;
            _timeout = timeout;

            await AttemptConnection();
        }

        private async Task AttemptConnection()
        {
            if (_stream != null)
                return;

            _logger.LogInformation($"Attempting to connect to {_host}:{_port}...");

            using (var cts = new CancellationTokenSource(_timeout))
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(_host, _port).WaitAsync(cts.Token);
                    _stream = new CotpNetworkStream(_client.GetStream());
                    _cts = new CancellationTokenSource();

                    int srcTsap = ((ushort)0x0600) & 0x0000FFFF;
                    byte[] dstTsap = Encoding.ASCII.GetBytes("SIMATIC-ROOT-HMI");

                    _logger.LogDebug("Sending COTP connection request...");
                    await _stream.SendConnectionRequest(srcTsap, dstTsap, _timeout);

                    _receiveTask = Task.Run(() => ReceiveLoop(_cts.Token));

                    _logger.LogDebug("Initializing S7 session...");
                    await InitializeS7Session();

                    _logger.LogInformation("Connected successfully with session ID {0} and session ID2 {1}", _sessionId, _sessionId2);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException("The connection attempt timed out.");
                }
            }
        }

        private async Task InitializeS7Session()
        {
            CreateObjectRequest request = new CreateObjectRequest(ProtocolVersion.V1, false);
            request.SessionId = S7Ids.ObjectNullServerSession;
            request.RequestId = S7Ids.ObjectServerSessionContainer;
            request.RequestValue = new S7VariableUDInt(0);
            request.RequestObject = new S7Object(S7Ids.GetNewRIDOnServer, S7Ids.ClassServerSession, S7Ids.None);
            request.RequestObject.Attributes.Add(S7Ids.ServerSessionClientRID, new S7VariableRID(SESSION_CLIENT_RID));
            request.RequestObject.AddObject(new S7Object(S7Ids.GetNewRIDOnServer, S7Ids.ClassSubscriptions, S7Ids.None));

            MemoryStream writeBuffer = new MemoryStream();
            request.Serialize(writeBuffer);
            var response = await SendInternal(writeBuffer.ToArray(), 0, ProtocolVersion.V1);

            Stream buffer = new MemoryStream(response);
            CreateObjectResponse objResponse = CreateObjectResponse.Deserialize(buffer);

            if (objResponse.ObjectIds.Count < 2)
                throw new Exception("Failed to initialize S7 session");

            _sessionId = objResponse.ObjectIds[0];
            _sessionId2 = objResponse.ObjectIds[1];

            if (objResponse.Object == null)
                throw new Exception("Failed to initialize S7 session. Session object missing.");

            if (!objResponse.Object.Attributes.ContainsKey(S7Ids.ServerSessionVersion))
                throw new Exception("Failed to initialize S7 session. Session version missing.");

            SetMultiVariablesRequest setupSessionReq = new SetMultiVariablesRequest(
                ProtocolVersion.V2, _sessionId, [S7Ids.ServerSessionVersion], [objResponse.Object.Attributes[S7Ids.ServerSessionVersion]]);

            response = await Send(setupSessionReq);
            buffer = new MemoryStream(response);
            SetMultiVariablesResponse setupSessionResp = SetMultiVariablesResponse.Deserialize(buffer);
            if (setupSessionResp.ErrorValues.Count > 0)
                throw new Exception("Failed to initialize S7 session. Error setting session version.");
        }

        private async Task Reconnect()
        {
            int attempt = 0;

            foreach (var tcs in _requests.Values)
            {
                tcs.TrySetException(new IOException("Connection reset."));
            }

            while (attempt < MAX_CONNECTION_ATTEMPTS)
            {
                try
                {
                    _logger.LogDebug($"Attempting to reconnect... (Attempt {attempt + 1}/{MAX_CONNECTION_ATTEMPTS})");

                    _client.Close();
                    _requests.Clear();
                    _stream?.Dispose();
                    _stream = null;

                    await AttemptConnection();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Reconnect attempt {attempt + 1} failed: {ex.Message}");
                    attempt++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
                }
            }

            _logger.LogError("Failed to reconnect after {0} attempts", MAX_CONNECTION_ATTEMPTS);
        }

        public async Task Disconnect()
        {
            if (_stream == null)
                return;

            _cts.Cancel();
            try
            {
                await _receiveTask;
            }
            catch (OperationCanceledException) { }

            _stream.Dispose();
            _client.Close();
            _requests.Clear();
            _stream = null;
        }

        public async Task<byte[]> Send(IS7Request request)
        {
            _sequenceNumber++;
            if (_sequenceNumber == UInt16.MaxValue)
                _sequenceNumber = 1;

            request.SequenceNumber = _sequenceNumber;
            request.SessionId = _sessionId;

            if (request.WithIntegrityId)
                request.IntegrityId = GetNextIntegrityId(request.FunctionCode);

            var buffer = new MemoryStream();
            request.Serialize(buffer);
            return await SendInternal(buffer.ToArray(), _sequenceNumber, request.ProtocolVersion);
        }

        private async Task<byte[]> SendInternal(byte[] request, UInt16 sequenceNumber, byte protocolVersion)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            _requests.TryAdd(sequenceNumber, tcs);

            int curSize;
            int sourcePos = 0;
            int sendLen;
            int NegotiatedIsoPduSize = 1024;// TODO: Respect the negotiated TPDU size
            int bytesToSend = request.Length;

            // 4 Byte TPKT Header
            // 3 Byte ISO-Header
            // 5 Byte TLS Header + 17 Bytes addition from TLS
            // 4 Byte S7CommPlus Header
            // 4 Byte S7CommPlus Trailer (must fit into last PDU)
            int MaxSize = NegotiatedIsoPduSize - 4 - 3 - 5 - 17 - 4 - 4;
            byte[] packet = new byte[MaxSize + 4]; //max packet size is always MaxSize + PDU Header

            while (bytesToSend > 0)
            {
                if (bytesToSend > MaxSize)
                {
                    curSize = MaxSize;
                    bytesToSend -= MaxSize;
                }
                else
                {
                    curSize = bytesToSend;
                    bytesToSend -= curSize;
                }
                // Header
                packet[0] = 0x72;
                packet[1] = protocolVersion;
                packet[2] = (byte)(curSize >> 8);
                packet[3] = (byte)(curSize & 0x00FF);
                // Data part
                Array.Copy(request, sourcePos, packet, 4, curSize);
                sourcePos += curSize;
                sendLen = 4 + curSize;

                // Trailer only in last packet
                if (bytesToSend == 0)
                {
                    Array.Resize(ref packet, sendLen + 4); //resize only the last package to sendLen + TrailerLen
                    packet[sendLen] = 0x72;
                    sendLen++;
                    packet[sendLen] = protocolVersion;
                    sendLen++;
                    packet[sendLen] = 0;
                    sendLen++;
                    packet[sendLen] = 0;
                }
                await _stream.SendPacket(packet);
            }

            return await tcs.Task;
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (_client.Client.Poll(0, SelectMode.SelectRead) && _client.Client.Available == 0)
                            throw new IOException("Connection reset.");

                        byte[] buffer = await _stream.ReceivePacket();
                        if (buffer.Length == 0)
                        {
                            await Task.Delay(TimeSpan.FromMicroseconds(10), token);
                            continue;
                        }

                        memoryStream.Write(buffer, 0, buffer.Length);

                        if (memoryStream.Length < 4)
                        {
                            continue;
                        }

                        memoryStream.Position = 0;
                        byte[] header = new byte[4];
                        memoryStream.Read(header, 0, 4);

                        if (header[0] != 0x72)
                            throw new InvalidDataException("Invalid S7CommPlus header");

                        if (header[1] != ProtocolVersion.V1 && header[1] != ProtocolVersion.V2 && header[1] != ProtocolVersion.V3)
                            throw new InvalidDataException("Invalid protocol version");

                        UInt16 s7HeaderLength = (UInt16)((header[2] << 8) | header[3]);
                        if (memoryStream.Length < s7HeaderLength + 4)
                        {
                            memoryStream.Position = memoryStream.Length; // Move to the end to append more data
                            continue;
                        }

                        byte[] fullPacket = new byte[s7HeaderLength];
                        memoryStream.Position = 4;
                        memoryStream.Read(fullPacket, 0, fullPacket.Length);

                        // Remove the processed packet from the stream
                        memoryStream.SetLength(0);
                        memoryStream.Position = 0;

                        // Process the packet
                        S7ResponseBase s7Response = new S7ResponseBase();
                        s7Response.DeserializeBase(new MemoryStream(fullPacket));

                        if (_requests.TryRemove(s7Response.SequenceNumber, out var tcs))
                        {
                            tcs.SetResult(fullPacket);
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, $"ReceiveLoop exception occurred attempting to reconnect...");
                await Reconnect();
            }
        }

        private UInt32 GetNextIntegrityId(ushort functioncode)
        {
            switch (functioncode)
            {
                case Functioncode.SetMultiVariables:
                case Functioncode.SetVariable:
                case Functioncode.SetVarSubStreamed:
                case Functioncode.DeleteObject:
                case Functioncode.CreateObject:
                    _integrityIdSet++;
                    if (_integrityIdSet == UInt32.MaxValue)
                        _integrityIdSet = 1;

                    return _integrityIdSet;
                default:
                    _integrityId++;
                    if (_integrityId == UInt32.MaxValue)
                        _integrityId = 1;

                    return _integrityId;
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}