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
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace S7Plus.Net
{
    public class S7TlsClientProtocol : TlsClientProtocol
    {
        public S7TlsClientProtocol(Stream stream)
            : base(stream)
        {
        }

        public override void Connect(TlsClient tlsClient)
        {
            if (tlsClient == null)
            {
                throw new ArgumentNullException("tlsClient");
            }

            if (m_tlsClient != null)
            {
                throw new InvalidOperationException("'Connect' can only be called once");
            }

            m_tlsClient = tlsClient;
            tlsClient.NotifyCloseHandle(this);
        }

        public void PerformCustomHandshake()
        {
            // Perform custom handshake steps here
            SendCustomHandshakeMessage();
            ReceiveCustomHandshakeResponse();
        }

        private void SendCustomHandshakeMessage()
        {
            // Example: Send a custom handshake message
            byte[] customHandshakeMessage = CreateCustomHandshakeMessage();
            Stream.Write(customHandshakeMessage, 0, customHandshakeMessage.Length);
            Stream.Flush();
        }

        private void ReceiveCustomHandshakeResponse()
        {
            // Example: Read the response from the server
            byte[] buffer = new byte[1024];
            int bytesRead = Stream.Read(buffer, 0, buffer.Length);
            ProcessServerResponse(buffer, bytesRead);
        }

        private byte[] CreateCustomHandshakeMessage()
        {
            // Create your custom handshake message here
            return new byte[] { /* your custom handshake data */ };
        }

        private void ProcessServerResponse(byte[] buffer, int bytesRead)
        {
            // Process the response from the server
        }
    }
}