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

using Org.BouncyCastle.Tls;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TKH.S7Plus.Net
{
    public class CotpNetworkStream : IDisposable
    {
        private readonly NetworkStream _networkStream;
        private readonly TlsClientProtocol _sslClientProtocol = new TlsClientProtocol();
        private readonly ConcurrentQueue<byte[]> _readBuffer = new ConcurrentQueue<byte[]>();
        private readonly ConcurrentQueue<byte[]> _writeBuffer = new ConcurrentQueue<byte[]>();
        private bool _ssl = false;
        private bool _handshaking = false;

        public CotpNetworkStream(NetworkStream networkStream)
        {
            _networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
        }

        public async Task EnableSsl(TimeSpan timeout)
        {
            if (_ssl)
                return;

            var tlsClient = new S7TlsClient();

            _handshaking = true;
            _sslClientProtocol.Connect(tlsClient);

            DateTime start = DateTime.Now;
            while (tlsClient.IsHandshaking)
            {
                await Task.Delay(1);
                if (DateTime.Now - start > timeout)
                {
                    _handshaking = false;
                    throw new TimeoutException("The SSL handshake timed out.");
                }
            }

            _handshaking = false;

            if (tlsClient.IsHandshakeComplete)
            {
                _ssl = true;
            }
            else
            {
                throw new IOException("SSL handshake failed.");
            }
        }

        public void Update()
        {
            if (_networkStream.DataAvailable)
            {
                MemoryStream buffer = new MemoryStream();
                int cotpLength = ReadCotp(buffer);

                byte[] data = new byte[cotpLength];
                buffer.Read(data, 0, cotpLength);

                if (_ssl || _handshaking)
                {
                    _sslClientProtocol.OfferInput(data);

                    if (!_handshaking)
                    {
                        byte[] sslData = new byte[_sslClientProtocol.GetAvailableInputBytes()];
                        _sslClientProtocol.ReadInput(sslData, 0, sslData.Length);
                        _readBuffer.Enqueue(sslData);
                    }
                }
                else
                {
                    _readBuffer.Enqueue(data);
                }
            }

            if (_writeBuffer.TryDequeue(out byte[] bufferData))
            {
                if (_ssl)
                {
                    _sslClientProtocol.WriteApplicationData(bufferData, 0, bufferData.Length);
                }
                else
                {
                    WriteCotp(bufferData, 0, bufferData.Length);
                }
            }

            if ((_ssl || _handshaking) && _sslClientProtocol.GetAvailableOutputBytes() > 0)
            {
                byte[] sslData = new byte[_sslClientProtocol.GetAvailableOutputBytes()];
                _sslClientProtocol.ReadOutput(sslData, 0, sslData.Length);
                WriteCotp(sslData, 0, sslData.Length);
            }
        }

        public async Task SendConnectionRequest(int sourceTsap, byte[] destinationTsap, TimeSpan timeout)
        {
            // ISO-on-TCP connection request
            byte[] isoConnRequest = {
            // TPKT (RFC1006 Header)
            0x03, // RFC 1006 ID (3) 
            0x00, // Reserved, always 0
            0x00, // High part of packet length (entire frame, payload and TPDU included)
            0x24, // Low part of packet length (entire frame, payload and TPDU included)
            // COTP (ISO 8073 Header)
            0x1f, // PDU Size Length
            0xE0, // CR - Connection Request ID
            0x00, // Dst Reference HI
            0x00, // Dst Reference LO
            0x00, // Src Reference HI
            0x01, // Src Reference LO
            0x00, // Class + Options Flags
            0xC0, // PDU Max Length ID
            0x01, // PDU Max Length HI
            0x0A, // PDU Max Length LO
            0xC1, // Src TSAP Identifier
            0x02, // Src TSAP Length (2 bytes)
            (byte)(sourceTsap >> 8), // Src TSAP HI
            (byte)(sourceTsap & 0x00FF), // Src TSAP LO
            0xC2, // Dst TSAP Identifier
            (byte)destinationTsap.Length, // Dst TSAP Length (16 bytes)
            // TSAP ID (String)
        };

            byte[] isoConnectionRequest = new byte[isoConnRequest.Length + destinationTsap.Length];
            Array.Copy(isoConnRequest, isoConnectionRequest, isoConnRequest.Length);
            Array.Copy(destinationTsap, 0, isoConnectionRequest, isoConnRequest.Length, destinationTsap.Length);

            await _networkStream.WriteAsync(isoConnectionRequest, 0, isoConnectionRequest.Length);
            await _networkStream.FlushAsync();

            bool connected = false;
            for (int i = 0; i < timeout.TotalMilliseconds; i++)
            {
                if (_networkStream.DataAvailable)
                {
                    byte[] buffer = new byte[36]; // 36 bytes is the minimum size of a connection confirm packet
                    await _networkStream.ReadAsync(buffer, 0, buffer.Length);

                    if (buffer[5] == 0xD0) // Connection Confirm ID
                    {
                        connected = true;
                        break;
                    }
                    else
                    {
                        throw new IOException("Connection request failed.");
                    }
                }
                else
                    await Task.Delay(1);
            }

            if (!connected)
                throw new TimeoutException("The connection attempt timed out.");
        }

        public void SendPacket(byte[] data)
        {
            _writeBuffer.Enqueue(data);
        }

        public byte[] ReceivePacket()
        {
            if (_readBuffer.TryDequeue(out byte[] buffer))
            {
                return buffer;
            }

            return Array.Empty<byte>();
        }

        private int ReadCotp(MemoryStream buffer)
        {
            byte[] tpktHeader = new byte[4];
            _networkStream.Read(tpktHeader, 0, tpktHeader.Length);

            int tpktLength = (tpktHeader[2] << 8) | tpktHeader[3];
            byte[] cotpHeader = new byte[3];
            _networkStream.Read(cotpHeader, 0, cotpHeader.Length);

            if (cotpHeader[1] != 0xF0 || cotpHeader[2] != 0x80)
            {
                throw new InvalidOperationException("Invalid COTP header");
            }

            int cotpLength = tpktLength - tpktHeader.Length - cotpHeader.Length;
            byte[] cotpData = new byte[cotpLength];

            buffer.Position = 0;
            buffer.SetLength(0);

            int bytesRead = 0;
            while (bytesRead < cotpLength)
            {
                int read = _networkStream.Read(cotpData, 0, cotpLength - bytesRead);
                if (read == 0)
                {
                    throw new IOException("Disconnected");
                }
                bytesRead += read;
            }

            buffer.Write(cotpData, 0, cotpData.Length);
            buffer.Seek(0, SeekOrigin.Begin);

            return cotpData.Length;
        }

        private void WriteCotp(byte[] buffer, int offset, int count)
        {
            int totalLength = count + 7;

            // Wrap the buffer in a COTP header before sending
            byte[] cotpHeader = new byte[]
            {
            0x03, 0x00, (byte)(totalLength >> 8), (byte)totalLength,    // COTP Header (3 bytes of COTP header + length)
            0x02, 0xF0, 0x80,                                           // COTP TPDU (indicates data packet)
            };

            byte[] cotpWrappedMessage = new byte[cotpHeader.Length + count];
            Array.Copy(cotpHeader, 0, cotpWrappedMessage, 0, cotpHeader.Length);
            Array.Copy(buffer, offset, cotpWrappedMessage, cotpHeader.Length, count);

            _networkStream.Write(cotpWrappedMessage, 0, cotpWrappedMessage.Length);
        }

        public void Dispose()
        {
            _networkStream.Dispose();
        }
    }
}