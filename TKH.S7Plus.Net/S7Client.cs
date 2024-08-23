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
using TKH.S7Plus.Net.Constants;
using TKH.S7Plus.Net.Requests;
using TKH.S7Plus.Net.Responses;
using TKH.S7Plus.Net.S7Variables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TKH.S7Plus.Net
{
    public class S7Client : IDisposable
    {
        private const int MAX_CONNECTION_ATTEMPTS = 5;
        private const UInt32 SESSION_CLIENT_RID = 0x80c3c901;
        private const int S7_HEADER_SIZE = 4;
        private const int S7_TRAILER_SIZE = 4;

        private TcpClient _client;
        private CotpNetworkStream _stream;
        private readonly ConcurrentDictionary<UInt64, Tuple<DateTime, TaskCompletionSource<byte[]>>> _requests;
        private UInt16 _sequenceNumber = 0;
        private UInt32 _integrityId = 0;
        private UInt32 _integrityIdSet = 0;
        private CancellationTokenSource _cts;
        private Task _receiveTask;
        private string _host;
        private int _port;
        private TimeSpan _timeout;
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private readonly ILogger _logger;

        private uint _sessionId = S7Ids.ObjectNullServerSession;
        private uint _sessionId2 = S7Ids.ObjectNullServerSession;

        public S7Client(ILogger? logger = null)
        {
            _requests = new ConcurrentDictionary<UInt64, Tuple<DateTime, TaskCompletionSource<byte[]>>>();
            _logger = logger ?? new NullLogger<S7Client>();
            _timeout = TimeSpan.FromSeconds(5);
        }

        public bool IsConnected { get; private set; }

        public void SetTimeout(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout is too large");

            if (timeout.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout is negative");

            _lock.Wait();
            _timeout = timeout;
            _lock.Release();
        }

        public async Task Connect(string host, int port)
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected.");

            _lock.Wait();

            _host = host;
            _port = port;

            _lock.Release();

            await AttemptConnection();
            IsConnected = true;
        }

        private async Task AttemptConnection()
        {
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
                    await InitSslSession();
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
            CreateObjectRequest request = new CreateObjectRequest(ProtocolVersion.V1, false)
            {
                RequestValue = new S7VariableUDInt(0),
                RequestObject = new S7Object(S7Ids.GetNewRIDOnServer, S7Ids.ClassServerSession, S7Ids.None),
                SessionId = S7Ids.ObjectNullServerSession,
                RequestId = S7Ids.ObjectServerSessionContainer
            };

            request.RequestObject.Attributes.Add(S7Ids.ServerSessionClientRID, new S7VariableRID(SESSION_CLIENT_RID));
            request.RequestObject.AddObject(new S7Object(S7Ids.GetNewRIDOnServer, S7Ids.ClassSubscriptions, S7Ids.None));

            MemoryStream writeBuffer = new MemoryStream();
            request.Serialize(writeBuffer);
            var response = await SendInternal(request);

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
            setupSessionReq.WithIntegrityId = false;

            response = await SendInternal(setupSessionReq);

            buffer = new MemoryStream(response);
            SetMultiVariablesResponse setupSessionResp = SetMultiVariablesResponse.Deserialize(buffer);
            if (setupSessionResp.ErrorValues.Count > 0)
                throw new Exception("Failed to initialize S7 session. Error setting session version.");

            GetVarSubStreamedRequest getAccessLevel = new GetVarSubStreamedRequest(ProtocolVersion.V2, _sessionId, S7Ids.EffectiveProtectionLevel);

            response = await SendInternal(getAccessLevel);

            buffer = new MemoryStream(response);
            GetVarSubStreamedResponse getAccessLevelResp = GetVarSubStreamedResponse.Deserialize(buffer);

            if (getAccessLevelResp.Value is S7VariableUDInt accessLevel)
            {
                if (accessLevel.Value >= AccessLevel.NoAccess)
                    throw new Exception("Failed to initialize S7 session. Access level not sufficient.");

                _logger.LogDebug("Access level: {0}", accessLevel.Value);
            }
            else
            {
                throw new Exception("Failed to initialize S7 session. Access level not found.");
            }
        }

        private async Task InitSslSession()
        {
            InitSslRequest initSslRequest = new InitSslRequest(ProtocolVersion.V1);
            MemoryStream initSslBuffer = new MemoryStream();
            initSslRequest.Serialize(initSslBuffer);

            var initSslResponse = await SendS7Packet(initSslBuffer.ToArray(), 0, ProtocolVersion.V1);

            Stream buffer = new MemoryStream(initSslResponse);
            InitSslResponse initSslResp = InitSslResponse.Deserialize(buffer);

            if (initSslResp.Error)
                throw new Exception("Failed to initialize SSL session.");

            await _stream.EnableSsl(_timeout);
        }

        private async Task Reconnect()
        {
            IsConnected = false;

            await Task.Yield(); // Ensure the current method is async

            try
            {
                foreach (var tcs in _requests.Values)
                {
                    tcs.Item2.TrySetException(new IOException("Connection reset."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset requests.");
            }

            int attempt = 0;

            while (attempt < MAX_CONNECTION_ATTEMPTS)
            {
                try
                {
                    _logger.LogDebug($"Attempting to reconnect... (Attempt {attempt + 1}/{MAX_CONNECTION_ATTEMPTS})");

                    CleanupResources();

                    await AttemptConnection();
                    IsConnected = true;
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

        private void CleanupResources()
        {
            try
            {
                _cts.Cancel();
                _receiveTask?.Wait(); // Ensure the receive loop is finished
            }
            catch (Exception) { }

            _stream?.Dispose();
            _client?.Close();
            _client?.Dispose();
            _requests.Clear();
            _cts.Dispose();
            _integrityId = 0;
            _integrityIdSet = 0;
            _sequenceNumber = 0;
            _sessionId = S7Ids.ObjectNullServerSession;
            _sessionId2 = S7Ids.ObjectNullServerSession;
        }

        public void Disconnect()
        {
            IsConnected = false;
            CleanupResources();
        }

        public Task<byte[]> Send(IS7Request request)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected.");

            return SendInternal(request);
        }

        private Task<byte[]> SendInternal(IS7Request request)
        {
            _lock.Wait();

            _sequenceNumber++;
            if (_sequenceNumber == UInt16.MaxValue)
                _sequenceNumber = 1;

            request.SequenceNumber = _sequenceNumber;
            request.SessionId = _sessionId;

            if (request.WithIntegrityId)
                request.IntegrityId = GetNextIntegrityId(request.FunctionCode);

            _lock.Release();

            var buffer = new MemoryStream();
            request.Serialize(buffer);
            return SendS7Packet(buffer.ToArray(), _sequenceNumber, request.ProtocolVersion);
        }

        private async Task<byte[]> SendS7Packet(byte[] request, UInt16 sequenceNumber, byte protocolVersion)
        {
            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            _requests.TryAdd(sequenceNumber, new Tuple<DateTime, TaskCompletionSource<byte[]>>(DateTime.Now, tcs));

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
            int MaxSize = NegotiatedIsoPduSize - 4 - 3 - 5 - 17 - S7_HEADER_SIZE - S7_TRAILER_SIZE;
            byte[] packet = new byte[MaxSize + S7_HEADER_SIZE]; //max packet size is always MaxSize + PDU Header

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
                Array.Copy(request, sourcePos, packet, S7_HEADER_SIZE, curSize);
                sourcePos += curSize;
                sendLen = S7_HEADER_SIZE + curSize;

                // Trailer only in last packet
                if (bytesToSend == 0)
                {
                    Array.Resize(ref packet, sendLen + S7_TRAILER_SIZE); //resize only the last package to sendLen + TrailerLen
                    packet[sendLen] = 0x72;
                    sendLen++;
                    packet[sendLen] = protocolVersion;
                    sendLen++;
                    packet[sendLen] = 0;
                    sendLen++;
                    packet[sendLen] = 0;
                }

                _stream.SendPacket(packet);
            }

            return await tcs.Task;
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                long lastPacketPos = 0;
                long totalS7Size = 0;
                List<long> headerPositions = new List<long>();

                using (var memoryStream = new MemoryStream())
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!_client.Client.Connected)
                            throw new IOException("Connection reset.");

                        _stream.Update();

                        byte[] buffer = _stream.ReceivePacket();
                        if (buffer.Length == 0)
                        {
                            var itemsToRemove = new List<UInt64>();

                            foreach (var reqTask in _requests)
                            {
                                if (DateTime.Now - reqTask.Value.Item1 > _timeout)
                                {
                                    itemsToRemove.Add(reqTask.Key);
                                }
                            }

                            foreach (var key in itemsToRemove)
                            {
                                if (_requests.TryRemove(key, out var timoutTask))
                                    timoutTask.Item2.TrySetException(new TimeoutException("Request timed out."));
                            }

                            await Task.Delay(TimeSpan.FromMicroseconds(10), token);
                            continue;
                        }

                        memoryStream.Write(buffer, 0, buffer.Length);

                        if (memoryStream.Length < S7_HEADER_SIZE)
                        {
                            continue;
                        }

                        memoryStream.Position = lastPacketPos;
                        headerPositions.Add(memoryStream.Position);
                        byte[] header = new byte[S7_HEADER_SIZE];
                        memoryStream.Read(header, 0, S7_HEADER_SIZE);

                        if (header[0] != 0x72)
                            throw new InvalidDataException("Invalid S7CommPlus header");

                        if (header[1] != ProtocolVersion.V1 && header[1] != ProtocolVersion.V2 && header[1] != ProtocolVersion.V3)
                            throw new InvalidDataException("Invalid protocol version");

                        UInt16 s7HeaderLength = (UInt16)((header[2] << 8) | header[3]);
                        totalS7Size += s7HeaderLength;
                        if (memoryStream.Length < totalS7Size + S7_TRAILER_SIZE + (headerPositions.Count * S7_HEADER_SIZE))
                        {
                            lastPacketPos = memoryStream.Length;
                            memoryStream.Position = memoryStream.Length; // Move to the end to append more data
                            continue;
                        }

                        byte[] fullPacket = new byte[totalS7Size];
                        int fullPacketPos = 0;
                        for (int i = 0; i < headerPositions.Count; i++)
                        {
                            memoryStream.Position = headerPositions[i] + S7_HEADER_SIZE;
                            int lengthToRead = (i < headerPositions.Count - 1) ? (int)((headerPositions[i + 1] - headerPositions[i]) - S7_HEADER_SIZE) :
                                (int)(totalS7Size - headerPositions[i]);
                            memoryStream.Read(fullPacket, fullPacketPos, lengthToRead);
                            fullPacketPos += lengthToRead;
                        }

                        // Remove the processed packet from the stream
                        memoryStream.SetLength(0);
                        memoryStream.Position = 0;
                        lastPacketPos = 0;
                        totalS7Size = 0;
                        headerPositions.Clear();

                        // Process the packet
                        S7ResponseBase s7Response = new S7ResponseBase();
                        s7Response.DeserializeBase(new MemoryStream(fullPacket));

                        if (_requests.TryRemove(s7Response.SequenceNumber, out var tcs))
                        {
                            tcs.Item2.SetResult(fullPacket);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (IsConnected)
                {
                    _logger.LogError(ex, $"ReceiveLoop exception occurred attempting to reconnect...");
                    _ = Reconnect();
                }
                else
                {
                    _logger.LogError(ex, "ReceiveLoop exception occurred.");
                }
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
                    if (_integrityIdSet == UInt32.MaxValue)
                        _integrityIdSet = 0;
                    else
                        _integrityIdSet++;

                    return _integrityIdSet;
                default:
                    if (_integrityId == UInt32.MaxValue)
                        _integrityId = 0;
                    else
                        _integrityId++;

                    return _integrityId;
            }
        }

        public void Dispose()
        {
            CleanupResources();
            _lock.Dispose();
        }
    }
}
