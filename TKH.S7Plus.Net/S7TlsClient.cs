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
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System.Collections.Generic;
using System.Net.Security;

namespace TKH.S7Plus.Net
{
    public class S7TlsClient : AbstractTlsClient
    {
        private readonly S7TlsAuthentication _authentication;

        public S7TlsClient() : base(new BcTlsCrypto())
        {
            _authentication = new S7TlsAuthentication();
        }

        public bool IsHandshakeComplete { get; private set; }
        public bool IsHandshaking { get; private set; }

        protected override int[] GetSupportedCipherSuites()
        {
            return new[]
            {
                (int)TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                (int)TlsCipherSuite.TLS_AES_128_GCM_SHA256
            };
        }

        public override ProtocolVersion[] GetProtocolVersions()
        {
            return new[] { ProtocolVersion.TLSv13 };
        }

        public override TlsAuthentication GetAuthentication()
        {
            return _authentication;
        }

        public override TlsSession GetSessionToResume()
        {
            return null;
        }

        public override IDictionary<int, byte[]> GetClientExtensions()
        {
            return base.GetClientExtensions();
        }

        public override void NotifySessionID(byte[] sessionID)
        {
            base.NotifySessionID(sessionID);
        }

        public override void NotifyHandshakeBeginning()
        {
            IsHandshaking = true;
        }

        public override void NotifyHandshakeComplete()
        {
            IsHandshaking = false;
            IsHandshakeComplete = true;
        }

        public override void NotifyConnectionClosed()
        {
            IsHandshakeComplete = false;
            IsHandshaking = false;
        }
    }

    public class S7TlsAuthentication : TlsAuthentication
    {
        public void NotifyServerCertificate(TlsServerCertificate serverCertificate)
        {

        }

        public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
        {
            return null; // No client certificate
        }
    }
}