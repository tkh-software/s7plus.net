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
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace S7Plus.Net
{
    public class CotpNetworkStream : IDisposable
    {
        private readonly NetworkStream _networkStream;

        public CotpNetworkStream(NetworkStream networkStream)
        {
            _networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
        }

        public async Task SendConnectionRequest(int sourceTsap, byte[] destinationTsap, TimeSpan timeout)
        {
            // ISO-on-TCP connection request
            byte[] isoConnRequest = {
            	// TPKT (RFC1006 Header)
            	0x03, // RFC 1006 ID (3) 
            	0x00, // Reserved, always 0
            	0x00, // High part of packet lenght (entire frame, payload and TPDU included)
            	0x24, // Low part of packet lenght (entire frame, payload and TPDU included)
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

        public async Task SendPacket(byte[] data)
        {
            // COTP Data Packet
            byte[] cotpHeader = new byte[]
            {
                0x03, 0x00, // TPKT version and reserved
                (byte)((data.Length + 7) >> 8), (byte)((data.Length + 7) & 0xFF), // TPKT length
                0x02, 0xF0, 0x80 // COTP header
            };

            byte[] packet = new byte[cotpHeader.Length + data.Length];
            Array.Copy(cotpHeader, 0, packet, 0, cotpHeader.Length);
            Array.Copy(data, 0, packet, cotpHeader.Length, data.Length);

            await _networkStream.WriteAsync(packet, 0, packet.Length);
        }

        public async Task<byte[]> ReceivePacket()
        {
            if (!_networkStream.DataAvailable)
            {
                return Array.Empty<byte>();
            }

            byte[] tpktHeader = new byte[4];
            await Read(tpktHeader);

            int tpktLength = (tpktHeader[2] << 8) | tpktHeader[3];
            byte[] cotpHeader = new byte[3];
            await Read(cotpHeader);

            if (cotpHeader[1] != 0xF0 || cotpHeader[2] != 0x80)
            {
                throw new InvalidOperationException("Invalid COTP header");
            }

            int cotpLength = tpktLength - tpktHeader.Length - cotpHeader.Length;
            byte[] cotpData = new byte[cotpLength];

            int bytesRead = 0;
            while (bytesRead < cotpLength)
            {
                int read = await _networkStream.ReadAsync(cotpData, bytesRead, cotpLength - bytesRead);
                if (read == 0)
                {
                    throw new IOException("Disconnected");
                }
                bytesRead += read;
            }

            return cotpData;
        }

        private async Task Read(byte[] buffer)
        {
            int bytesRead = 0;
            while (bytesRead < buffer.Length)
            {
                int read = await _networkStream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead);
                if (read == 0)
                {
                    throw new IOException("Disconnected");
                }
                bytesRead += read;
            }
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
        }
    }
}