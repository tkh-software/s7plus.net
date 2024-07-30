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

namespace S7Plus.Net.Helpers
{
    public static class ConvertGeneric
    {
        private static readonly Dictionary<Type, Delegate> serializers = new()
        {
            { typeof(byte), new Func<Stream, byte, int>(S7ValueEncoder.EncodeByte) },
            { typeof(sbyte), new Func<Stream, sbyte, int>((buffer, value) => S7ValueEncoder.EncodeByte(buffer, Convert.ToByte(value))) },
            { typeof(bool), new Func<Stream, bool, int>((buffer, value) => S7ValueEncoder.EncodeByte(buffer, Convert.ToByte(value))) },
            { typeof(UInt16), new Func<Stream, UInt16, int>(S7ValueEncoder.EncodeUInt16) },
            { typeof(Int16), new Func<Stream, Int16, int>(S7ValueEncoder.EncodeInt16) },
            { typeof(UInt32), new Func<Stream, UInt32, int>(S7ValueEncoder.EncodeUInt32) },
            { typeof(Int32), new Func<Stream, Int32, int>(S7ValueEncoder.EncodeInt32) },
            { typeof(UInt64), new Func<Stream, UInt64, int>(S7ValueEncoder.EncodeUInt64) },
            { typeof(Int64), new Func<Stream, Int64, int>(S7ValueEncoder.EncodeInt64) },
            { typeof(float), new Func<Stream, float, int>(S7ValueEncoder.EncodeFloat) },
            { typeof(double), new Func<Stream, double, int>(S7ValueEncoder.EncodeDouble) },
        };

        private static readonly Dictionary<Type, Delegate> deserializers = new()
        {
            { typeof(byte), new Func<Stream, byte>(S7ValueDecoder.DecodeByte) },
            { typeof(sbyte), new Func<Stream, sbyte>(buffer => Convert.ToSByte(S7ValueDecoder.DecodeByte(buffer))) },
            { typeof(bool), new Func<Stream, bool>(buffer => Convert.ToBoolean(S7ValueDecoder.DecodeByte(buffer))) },
            { typeof(UInt16), new Func<Stream, UInt16>(S7ValueDecoder.DecodeUInt16) },
            { typeof(Int16), new Func<Stream, Int16>(S7ValueDecoder.DecodeInt16) },
            { typeof(UInt32), new Func<Stream, UInt32>(S7ValueDecoder.DecodeUInt32) },
            { typeof(Int32), new Func<Stream, Int32>(S7ValueDecoder.DecodeInt32) },
            { typeof(UInt64), new Func<Stream, UInt64>(S7ValueDecoder.DecodeUInt64) },
            { typeof(Int64), new Func<Stream, Int64>(S7ValueDecoder.DecodeInt64) },
            { typeof(float), new Func<Stream, float>(S7ValueDecoder.DecodeFloat) },
            { typeof(double), new Func<Stream, double>(S7ValueDecoder.DecodeDouble) },
        };

        private static readonly Dictionary<Type, Delegate> vlqDeserializers = new()
        {
            { typeof(byte), new Func<Stream, byte>(S7ValueDecoder.DecodeByte) },
            { typeof(sbyte), new Func<Stream, sbyte>(buffer => Convert.ToSByte(S7ValueDecoder.DecodeByte(buffer))) },
            { typeof(bool), new Func<Stream, bool>(buffer => Convert.ToBoolean(S7ValueDecoder.DecodeByte(buffer))) },
            { typeof(UInt16), new Func<Stream, UInt16>(S7ValueDecoder.DecodeUInt16) },
            { typeof(Int16), new Func<Stream, Int16>(S7ValueDecoder.DecodeInt16) },
            { typeof(UInt32), new Func<Stream, UInt32>(S7VlqValueDecoder.DecodeUInt32Vlq) },
            { typeof(Int32), new Func<Stream, Int32>(S7VlqValueDecoder.DecodeInt32Vlq) },
            { typeof(UInt64), new Func<Stream, UInt64>(S7VlqValueDecoder.DecodeUInt64Vlq) },
            { typeof(Int64), new Func<Stream, Int64>(S7VlqValueDecoder.DecodeInt64Vlq) },
            { typeof(float), new Func<Stream, float>(S7ValueDecoder.DecodeFloat) },
            { typeof(double), new Func<Stream, double>(S7ValueDecoder.DecodeDouble) },
        };

        public static int Serialize<T>(Stream buffer, T value) where T : struct
        {
            if (serializers.TryGetValue(typeof(T), out var serializer))
            {
                return ((Func<Stream, T, int>)serializer)(buffer, value);
            }
            throw new ArgumentException("Unsupported type");
        }

        public static T Deserialize<T>(Stream buffer, bool disableVlq) where T : struct
        {
            if (disableVlq)
            {
                if (deserializers.TryGetValue(typeof(T), out var deserializer))
                {
                    return ((Func<Stream, T>)deserializer)(buffer);
                }
            }
            else
            {
                if (vlqDeserializers.TryGetValue(typeof(T), out var deserializer))
                {
                    return ((Func<Stream, T>)deserializer)(buffer);
                }
            }

            throw new ArgumentException("Unsupported type");
        }
    }
}