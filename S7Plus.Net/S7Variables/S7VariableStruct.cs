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

namespace S7Plus.Net.S7Variables
{
    public class S7VariableStruct : S7VariableBase
    {
        private Dictionary<UInt32, S7VariableBase> _members = new Dictionary<UInt32, S7VariableBase>();

        public UInt32 Value { get; private set; }
        public UInt64 PackedStructInterfaceTimestamp { get; set; }
        public UInt32 PackedStructTransportFlags { get; set; } = (UInt32)Constants.PackedStructTransportFlag.AlwaysSet;

        public S7VariableStruct() : this(default, 0)
        {
        }

        public S7VariableStruct(UInt32 value) : this(value, 0)
        {
        }

        public S7VariableStruct(UInt32 value, byte flags)
        {
            Datatype = Constants.Datatype.Struct;
            _datatypeFlags = flags;
            Value = value;
        }

        public void AddMember(UInt32 id, S7VariableBase member)
        {
            if (_members.ContainsKey(id))
                _members.Remove(id);

            _members.Add(id, member);
        }

        public S7VariableBase GetMember(UInt32 id)
        {
            return _members[id];
        }

        public override int Serialize(Stream buffer)
        {
            int length = base.Serialize(buffer);
            length += S7ValueEncoder.EncodeUInt32(buffer, Value);

            // Packed Struct, see comment in Deserialize
            if ((Value > 0x90000000 && Value < 0x9fffffff) || (Value > 0x02000000 && Value < 0x02ffffff))
            {
                // There should be only one Element? The key from the dictionary element is not used.
                // It's somewhat all hacked into the Struct variant...
                foreach (var elem in _members)
                {
                    // The timestamp must be exactly the same as from browsing the Plc, otherwise we
                    // get an Error "InvalidTimestampInTypeSafeBlob"
                    length += S7ValueEncoder.EncodeUInt64(buffer, PackedStructInterfaceTimestamp);

                    length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, PackedStructTransportFlags);

                    if (elem.Value.GetType() == typeof(S7VariableByteArray))
                    {
                        var barr = ((S7VariableByteArray)elem.Value).Value;
                        UInt32 elementcount = (UInt32)barr.Length;
                        length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, elementcount);

                        // Don't use the Serialize method of S7VariableByteArray, because there is an additional header we don't want here.
                        for (int i = 0; i < barr.Length; i++)
                        {
                            length += S7ValueEncoder.EncodeByte(buffer, barr[i]);
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("Only byte array is supported in first element of PackedStruct");
                    }
                }
            }
            else
            {
                foreach (var elem in _members)
                {
                    length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, elem.Key);
                    length += elem.Value.Serialize(buffer);
                }

                length += S7ValueEncoder.EncodeByte(buffer, 0); // List Terminator
            }

            return length;
        }

        public static S7VariableStruct Deserialize(Stream buffer, byte flags, bool disableVlq)
        {
            UInt32 value = S7ValueDecoder.DecodeUInt32(buffer);
            S7VariableStruct obj;

            // Special handling of datatype struct and some specific ID ranges:
            // Some struct elements aren't transmitted as single elements. Instead they are packed (e.g. DTL-Struct).
            // The ID number range where this is used is only guessed (Type Info).
            if ((value > 0x90000000 && value < 0x9fffffff) || (value > 0x02000000 && value < 0x02ffffff))
            {
                // Packed Struct
                // These are system datatypes. Either the information about them must be read out of the CPU before,
                // or must be known before. As the data are transmitted as Bytearrays, return them in this type. Interpretation must be done later.
                obj = new S7VariableStruct(value, flags);
                obj.PackedStructInterfaceTimestamp = S7ValueDecoder.DecodeUInt64(buffer);

                UInt32 elementcount;

                if (!disableVlq)
                {
                    obj.PackedStructTransportFlags = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                    elementcount = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);

                    if ((obj.PackedStructTransportFlags & (uint)PackedStructTransportFlag.Count2Present) != 0)
                    {
                        elementcount = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                    }
                }
                else
                {
                    obj.PackedStructTransportFlags = S7ValueDecoder.DecodeUInt32(buffer);
                    elementcount = S7ValueDecoder.DecodeUInt32(buffer);

                    if ((obj.PackedStructTransportFlags & (uint)PackedStructTransportFlag.Count2Present) != 0)
                    {
                        elementcount = S7ValueDecoder.DecodeUInt32(buffer);
                    }
                }

                byte[] barr = new byte[elementcount];
                for (int i = 0; i < elementcount; i++)
                {
                    barr[i] = S7ValueDecoder.DecodeByte(buffer);
                }

                S7VariableByteArray elem = new S7VariableByteArray(barr);
                obj.AddMember(value, elem);
            }
            else
            {
                S7VariableBase elem;
                obj = new S7VariableStruct(value, flags);

                UInt32 memberId = disableVlq ? S7ValueDecoder.DecodeUInt32(buffer) : S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                while (memberId > 0)
                {
                    elem = S7VariableBase.Deserialize(buffer, disableVlq);
                    obj.AddMember(value, elem);
                    memberId = disableVlq ? S7ValueDecoder.DecodeUInt32(buffer) : S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                }
            }

            return obj;
        }
    }
}