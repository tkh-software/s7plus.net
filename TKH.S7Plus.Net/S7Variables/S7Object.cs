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

using TKH.S7Plus.Net.Constants;
using TKH.S7Plus.Net.Helpers;
using TKH.S7Plus.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TKH.S7Plus.Net.S7Variables
{
    public class S7Object
    {
        private readonly Dictionary<Tuple<UInt32, UInt32>, S7Object> _objects = new Dictionary<Tuple<UInt32, UInt32>, S7Object>();

        public UInt32 RelationId { get; set; }
        public UInt32 ClassId { get; set; }
        public UInt32 ClassFlags { get; set; }
        public UInt32 AttributeId { get; set; }
        public Dictionary<UInt32, S7VariableBase> Attributes { get; } = new Dictionary<UInt32, S7VariableBase>();
        public Dictionary<UInt32, UInt32> Relations { get; } = new Dictionary<UInt32, UInt32>();
        public S7VarTypeList VarTypeList { get; set; }
        public S7VarNameList VarNameList { get; set; }
        public IReadOnlyList<S7Object> Objects => _objects.Values.ToList();

        public S7Object() : this(0, 0, 0)
        {
        }

        public S7Object(UInt32 relationId, UInt32 classId, UInt32 attributeId)
        {
            RelationId = relationId;
            ClassId = classId;
            AttributeId = attributeId;
        }

        public void AddObject(S7Object obj)
        {
            // Whether using the ClassId as Key makes sense, remains to be seen
            // TODO: The ClassId is not unique and may be occur more than once
            // (e.g. DB.Class_Rid and in RelId is the DB number as DB.1)
            var tuple = new Tuple<UInt32, UInt32>(obj.ClassId, obj.RelationId);
            _objects.Add(tuple, obj);
        }

        public int Serialize(Stream buffer)
        {
            int length = 0;
            length += S7ValueEncoder.EncodeByte(buffer, ElementId.StartOfObject);
            length += S7ValueEncoder.EncodeUInt32(buffer, RelationId);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, ClassId);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, ClassFlags);
            length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, AttributeId);

            foreach (var elem in Attributes)
            {
                length += S7ValueEncoder.EncodeByte(buffer, ElementId.Attribute);
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, elem.Key);
                length += elem.Value.Serialize(buffer);
            }

            foreach (var obj in Objects)
            {
                length += obj.Serialize(buffer);
            }

            foreach (var rel in Relations)
            {
                length += S7ValueEncoder.EncodeByte(buffer, ElementId.Relation);
                length += S7VlqValueEncoder.EncodeUInt32Vlq(buffer, rel.Key);
                length += S7ValueEncoder.EncodeUInt32(buffer, rel.Value);
            }

            length += S7ValueEncoder.EncodeByte(buffer, ElementId.TerminatingObject);

            return length;
        }

        public static S7Object? Deserialize(Stream buffer, S7Object? obj = null)
        {
            bool terminate = false;
            do
            {
                byte tagId = S7ValueDecoder.DecodeByte(buffer);
                switch (tagId)
                {
                    case ElementId.StartOfObject:
                        if (obj == null)
                        {
                            obj = new S7Object();
                            obj.RelationId = S7ValueDecoder.DecodeUInt32(buffer);
                            obj.ClassId = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                            obj.ClassFlags = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                            obj.AttributeId = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                        }
                        else
                        {
                            S7Object newobj = new S7Object();
                            newobj.RelationId = S7ValueDecoder.DecodeUInt32(buffer);
                            newobj.ClassId = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                            newobj.ClassFlags = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                            newobj.AttributeId = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                            S7Object? childObj = Deserialize(buffer, newobj);

                            if (childObj != null)
                                obj.AddObject(childObj);
                        }
                        break;
                    case ElementId.TerminatingObject:
                        terminate = true;
                        break;
                    case ElementId.Attribute:
                        UInt32 attrId = S7VlqValueDecoder.DecodeUInt32Vlq(buffer);
                        S7VariableBase attr = S7VariableBase.Deserialize(buffer);
                        if (obj != null && !obj.Attributes.ContainsKey(attrId))
                            obj.Attributes.Add(attrId, attr);
                        break;
                    case ElementId.StartOfTagDescription:
                        // Skip, only 1200 FW2 and maybe older
                        break;
                    case ElementId.VartypeList:
                        S7VarTypeList typelist = S7VarTypeList.Deserialize(buffer);
                        if (obj != null)
                            obj.VarTypeList = typelist;
                        break;
                    case ElementId.VarnameList:
                        S7VarNameList namelist = S7VarNameList.Deserialize(buffer);
                        if (obj != null)
                            obj.VarNameList = namelist;
                        break;
                    default:
                        terminate = true;
                        break;
                }
            } while (terminate == false);

            return obj;
        }
    }
}