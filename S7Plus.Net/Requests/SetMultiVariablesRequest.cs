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
using System.Collections.Generic;
using System.IO;
using S7Plus.Net.Constants;
using S7Plus.Net.Helpers;
using S7Plus.Net.Models;
using S7Plus.Net.S7Variables;

namespace S7Plus.Net.Requests
{
    public class SetMultiVariablesRequest : S7RequestBase
    {
        private const byte TRANSPORT_FLAGS = 0x34;

        private readonly List<IS7Address> _addresses = new List<IS7Address>();
        private readonly List<S7VariableBase> _values = new List<S7VariableBase>();
        private readonly List<UInt32> _varIds = new List<UInt32>();
        private readonly UInt32 _objectId = 0;

        public override UInt16 FunctionCode => Functioncode.SetMultiVariables;

        public SetMultiVariablesRequest(byte protocolVersion, List<IS7Address> addresses, List<S7VariableBase> values) : base(protocolVersion)
        {
            _addresses = addresses ?? throw new ArgumentNullException(nameof(addresses));
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public SetMultiVariablesRequest(byte protocolVersion, UInt32 objectId, List<UInt32> varIds, List<S7VariableBase> values) : base(protocolVersion)
        {
            _objectId = objectId;
            _varIds = varIds ?? throw new ArgumentNullException(nameof(varIds));
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);

            length += S7ValueEncoder.EncodeByte(buffer, TRANSPORT_FLAGS);
            length += S7ValueEncoder.EncodeUInt32(buffer, _objectId);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)_values.Count);

            if (_objectId > 0)
            {
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)_varIds.Count);
                foreach (UInt32 varId in _varIds)
                {
                    length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, varId);
                }
            }
            else
            {
                UInt32 fieldCount = 0;
                foreach (IS7Address adr in _addresses)
                {
                    fieldCount += adr.FieldCount;
                }
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, fieldCount);

                foreach (IS7Address adr in _addresses)
                {
                    length += adr.Serialize(buffer);
                }
            }

            for (int i = 1; i <= _values.Count; i++)
            {
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, (UInt32)i);
                length += _values[i - 1].Serialize(buffer);
            }

            length += S7ValueEncoder.EncodeByte(buffer, 0); // List Terminator
            length += S7ValueEncoder.EncodeObjectQualifier(buffer);

            if (WithIntegrityId)
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, IntegrityId);

            length += S7ValueEncoder.EncodeUInt32(buffer, 0);

            return length;
        }
    }
}
