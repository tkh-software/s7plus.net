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
using System.IO;
using S7Plus.Net.Constants;
using S7Plus.Net.Helpers;

namespace S7Plus.Net.S7Variables
{
    public abstract class S7VariableBase : IS7Variable
    {
        protected const byte FLAGS_ARRAY = 0x10;
        protected const byte FLAGS_ADDRESSARRAY = 0x20;
        protected const byte FLAGS_SPARSEARRAY = 0x40;

        protected byte _datatypeFlags;

        public byte Datatype { get; protected set; }

        virtual public int Serialize(Stream buffer)
        {
            int length = 0;
            length += S7ValueEncoder.EncodeByte(buffer, _datatypeFlags);
            length += S7ValueEncoder.EncodeByte(buffer, Datatype);
            return length;
        }

        /// <summary>
        /// Deserializes the buffer to the protocol values
        /// </summary>
        /// <param name="buffer">Stream of bytes from the network</param>
        /// <param name="disableVlq">If true, the variable length encoding is disabled for all underlying values</param>
        /// <returns>The protocol value</returns>
        public static S7VariableBase Deserialize(Stream buffer, bool disableVlq = false)
        {
            byte flags;
            byte datatype;

            if (disableVlq)
            {
                // If VLQ is disabled, there are two additional bytes we just skip here.
                S7ValueDecoder.DecodeByte(buffer);
                flags = S7ValueDecoder.DecodeByte(buffer);
                S7ValueDecoder.DecodeByte(buffer);
                datatype = S7ValueDecoder.DecodeByte(buffer);
            }
            else
            {
                flags = S7ValueDecoder.DecodeByte(buffer);
                datatype = S7ValueDecoder.DecodeByte(buffer);
            }

            // Sparsearray and Adressarray of Struct are different
            if (flags == FLAGS_ARRAY || flags == FLAGS_ADDRESSARRAY)
            {
                return datatype switch
                {
                    Constants.Datatype.Bool => S7VariableBoolArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.USInt => S7VariableUSIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.UInt => S7VariableUIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.UDInt => S7VariableUDIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.ULInt => S7VariableULIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.SInt => S7VariableSIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Int => S7VariableIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.DInt => S7VariableDIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.LInt => S7VariableLIntArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Byte => S7VariableByteArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Word => S7VariableWordArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.DWord => S7VariableDWordArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.LWord => S7VariableLWordArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Real => S7VariableRealArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.LReal => S7VariableLRealArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Timestamp => S7VariableTimestampArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Timespan => S7VariableTimespanArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.RID => S7VariableRIDArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.AID => S7VariableAIDArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Blob => S7VariableBlobArray.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.WString => S7VariableWStringArray.Deserialize(buffer, flags, disableVlq),
                    _ => throw new NotImplementedException(),
                };
            }
            else if (flags == FLAGS_SPARSEARRAY)
            {
                throw new NotImplementedException();
            }
            else
            {
                return datatype switch
                {
                    Constants.Datatype.Null => S7VariableNull.Deserialize(flags),
                    Constants.Datatype.Bool => S7VariableBool.Deserialize(buffer, flags),
                    Constants.Datatype.USInt => S7VariableUSInt.Deserialize(buffer, flags),
                    Constants.Datatype.UInt => S7VariableUInt.Deserialize(buffer, flags),
                    Constants.Datatype.UDInt => S7VariableUDInt.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.ULInt => S7VariableULInt.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.SInt => S7VariableSInt.Deserialize(buffer, flags),
                    Constants.Datatype.Int => S7VariableInt.Deserialize(buffer, flags),
                    Constants.Datatype.DInt => S7VariableDInt.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.LInt => S7VariableLInt.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Byte => S7VariableByte.Deserialize(buffer, flags),
                    Constants.Datatype.Word => S7VariableWord.Deserialize(buffer, flags),
                    Constants.Datatype.DWord => S7VariableDWord.Deserialize(buffer, flags),
                    Constants.Datatype.LWord => S7VariableLWord.Deserialize(buffer, flags),
                    Constants.Datatype.Real => S7VariableReal.Deserialize(buffer, flags),
                    Constants.Datatype.LReal => S7VariableLReal.Deserialize(buffer, flags),
                    Constants.Datatype.Timestamp => S7VariableTimestamp.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Timespan => S7VariableTimespan.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.RID => S7VariableRID.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.AID => S7VariableAID.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Blob => S7VariableBlob.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.WString => S7VariableWString.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.Variant => throw new NotImplementedException(),
                    Constants.Datatype.Struct => S7VariableStruct.Deserialize(buffer, flags, disableVlq),
                    Constants.Datatype.S7String => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}