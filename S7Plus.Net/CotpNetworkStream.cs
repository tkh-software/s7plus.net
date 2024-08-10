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
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace S7Plus.Net
{
    public class CotpNetworkStream : Stream
    {
        private readonly NetworkStream _networkStream;
        private readonly string _targetHost;
        private TlsClientProtocol _sslClientProtocol;
        private readonly MemoryStream _buffer = new MemoryStream();

        public CotpNetworkStream(NetworkStream networkStream, string targetHost)
        {
            _networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
            _targetHost = targetHost ?? throw new ArgumentNullException(nameof(targetHost));
        }

        public override bool CanRead => _networkStream.CanRead;
        public override bool CanSeek => _networkStream.CanSeek;
        public override bool CanWrite => _networkStream.CanWrite;
        public override long Length => _buffer.Length;
        public override long Position { get => _buffer.Position; set => _networkStream.Position = value; }

        public void EnableSsl()
        {
            _sslClientProtocol = new TlsClientProtocol(this);
            var tlsClient = new S7TlsClient();
            _sslClientProtocol.Connect(tlsClient);
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Implement server certificate validation
            return true;
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
            if (_sslClientProtocol != null)
                _sslClientProtocol.WriteApplicationData(data, 0, data.Length);
            else
                Write(data, 0, data.Length);

            Flush();
        }

        public byte[] ReceivePacket()
        {
            if (!_networkStream.DataAvailable)
            {
                return Array.Empty<byte>();
            }

            byte[] buffer = new byte[1024];
            int count = _sslClientProtocol == null ? Read(buffer, 0, buffer.Length)
                : _sslClientProtocol.ReadApplicationData(buffer);

            if(_sslClientProtocol != null)
            {
                byte[] data = new byte[count];
                Array.Copy(buffer, data, count);
                return data;
            }

            if(Length > 0)
            {
                byte[] data = new byte[Length];
                Read(data, 0, data.Length);
                
                //append to buffer
                byte[] result = new byte[buffer.Length + data.Length];
                Array.Copy(buffer, 0, result, 0, buffer.Length);
                Array.Copy(data, 0, result, buffer.Length, data.Length);
                return result;
            }

            //resize buffer
            Array.Resize(ref buffer, count);

            return buffer;
        }

        public override void Flush()
        {
            _networkStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(_buffer.Length > 0)
            {
                int read = _buffer.Read(buffer, offset, count);
                if (_buffer.Position == _buffer.Length)
                {
                    _buffer.SetLength(0);
                }
                return read;
            }

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
            _buffer.Position = 0;
            _buffer.SetLength(0);

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

            _buffer.Write(cotpData, 0, cotpData.Length);
            _buffer.Seek(0, SeekOrigin.Begin);
            return Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _networkStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _networkStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Wrap the buffer in a COTP header before sending
            byte[] cotpHeader = new byte[]
            {
            0x03, 0x00, 0x00, (byte)(count + 7), // COTP Header (3 bytes of COTP header + length)
            0x02, 0xF0, 0x80,                   // COTP TPDU (indicates data packet)
            };

            byte[] cotpWrappedMessage = new byte[cotpHeader.Length + count];
            Array.Copy(cotpHeader, 0, cotpWrappedMessage, 0, cotpHeader.Length);
            Array.Copy(buffer, offset, cotpWrappedMessage, cotpHeader.Length, count);

            _networkStream.Write(cotpWrappedMessage, 0, cotpWrappedMessage.Length);
        }
    }
}