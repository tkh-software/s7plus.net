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
using TKH.S7Plus.Net.Constants;
using TKH.S7Plus.Net.Models;
using TKH.S7Plus.Net.Models.OffsetInfos;
using TKH.S7Plus.Net.S7Variables;

namespace TKH.S7Plus.Net.Helpers
{
    public class VariableBrowser
    {
        private readonly List<Node> _nodes = new List<Node>();
        private readonly List<S7Object> _infoObjects = new List<S7Object>();

        public VariableBrowser(List<S7Object> infoObjects)
        {
            _infoObjects = infoObjects ?? throw new ArgumentNullException(nameof(infoObjects));
        }

        public void AddRootNode(string name, UInt32 relationId, UInt32 typeInfoRelationId)
        {
            _nodes.Add(new Node
            {
                Type = NodeType.Root,
                Name = name,
                AccessId = relationId,
                RelationId = typeInfoRelationId
            });
        }

        public List<VariableInfo> GetAllVariables()
        {
            foreach (Node node in _nodes)
            {
                S7Object? infoObject = _infoObjects.Find(x => x.RelationId == node.RelationId);
                if (infoObject != null)
                {
                    AddSubNodes(node, infoObject);
                }
            }

            List<VariableInfo> variables = new List<VariableInfo>();
            foreach (Node node in _nodes)
            {
                FillList(variables, node, string.Empty, string.Empty);
            }

            return variables;
        }

        private void FillList(List<VariableInfo> variables, Node node, string path, string accessId)
        {
            switch (node.Type)
            {
                case NodeType.Root:
                    path += node.Name;
                    accessId += String.Format("{0:X}", node.AccessId);
                    break;
                case NodeType.Array:
                    path += node.Name;
                    accessId += "." + String.Format("{0:X}", node.AccessId);
                    break;
                case NodeType.StructArray:
                    path += node.Name;
                    // Special: Between an array-index and the access-id is an additional 1. It's not known if it's a fixed or variable value.
                    accessId += "." + String.Format("{0:X}", node.AccessId) + ".1";
                    break;
                default:
                    path += "." + node.Name;
                    accessId += "." + String.Format("{0:X}", node.AccessId);
                    break;
            }

            if (node.Children.Count == 0)
            {
                // We are at the leaf of our tree
                if (IsKnownDatatype(node.SoftDatatype))
                {
                    var info = new VariableInfo(path, node.SoftDatatype, new S7Address(accessId));
                    variables.Add(info);
                }
            }
            else
            {
                foreach (var sub in node.Children)
                {
                    FillList(variables, sub, path, accessId);
                }
            }
        }

        private void AddSubNodes(Node node, S7Object obj)
        {
            int element_index = 0;
            // If there are no variables at all in an area, then this list does not exist (no error).
            if (obj.VarTypeList != null)
            {
                foreach (var vte in obj.VarTypeList.Types)
                {
                    var subnode = new Node
                    {
                        Name = obj.VarNameList.Names[element_index],
                        SoftDatatype = vte.SoftDatatype,
                        AccessId = vte.LID
                    };
                    node.Children.Add(subnode);

                    if (vte.OffsetInfo.IsOneDimensional)
                    {
                        // Struct/UDT or flat arrays with one dimension
                        Process1DimArray(vte, subnode);
                    }
                    else if (vte.OffsetInfo.IsMultiDimensional)
                    {
                        // Struct/UDT or flat array with more than one dimension
                        ProcessMDimArray(vte, subnode);
                    }
                    else if (vte.OffsetInfo.HasRelation)
                    {
                        // Struct / UDT / system library types (DTL, IEC_TIMER, ...) but not an array ...
                        ProcessRelation(vte, subnode);
                    }

                    element_index++;
                }
            }
        }

        private void Process1DimArray(S7VarType varType, Node node)
        {
            var offsetInfo = (IOffsetInfo1Dim)varType.OffsetInfo;

            // The access-id always starts with 0, independent of lowerbounds
            for (uint i = 0; i < offsetInfo.ArrayElementCount; i++)
            {
                // Handle Struct/FB Array separate: Has an additional ID between array index and access-LID.
                if (offsetInfo.HasRelation)
                {
                    var arraynode = new Node
                    {
                        Type = NodeType.StructArray,
                        Name = "[" + (i + offsetInfo.ArrayLowerBounds) + "]",
                        SoftDatatype = varType.SoftDatatype,
                        AccessId = i
                    };

                    node.Children.Add(arraynode);

                    // All OffsetInfoTypes which occur at this point should have a Relation Id
                    ProcessRelation(varType, arraynode);
                }
                else
                {
                    var arraynode = new Node
                    {
                        Type = NodeType.Array,
                        Name = "[" + (i + offsetInfo.ArrayLowerBounds) + "]",
                        SoftDatatype = varType.SoftDatatype,
                        AccessId = i
                    };

                    node.Children.Add(arraynode);
                }
            }
        }

        private void ProcessMDimArray(S7VarType varType, Node node)
        {
            var offsetInfo = (IOffsetInfoMDim)varType.OffsetInfo;

            // Determine the actual number of dimensions
            int actdimensions = 0;
            for (int d = 0; d < 6; d++)
            {
                if (offsetInfo.MDimElementCounts[d] > 0)
                {
                    actdimensions++;
                }
            }

            string aname = "";
            int n = 1;
            uint id = 0;
            uint[] xx = new uint[6] { 0, 0, 0, 0, 0, 0 };
            do
            {
                aname = "[";
                for (int j = actdimensions - 1; j >= 0; j--)
                {
                    aname += (xx[j] + offsetInfo.MDimLowerBounds[j]).ToString();
                    if (j > 0)
                    {
                        aname += ",";
                    }
                    else
                    {
                        aname += "]";
                    }
                }

                if (offsetInfo.HasRelation)
                {
                    var arraynode = new Node
                    {
                        Type = NodeType.StructArray,
                        Name = aname,
                        SoftDatatype = varType.SoftDatatype,
                        AccessId = id
                    };

                    node.Children.Add(arraynode);

                    // All OffsetInfoTypes which occur at this point should have a Relation Id
                    ProcessRelation(varType, arraynode);
                }
                else
                {
                    var arraynode = new Node
                    {
                        Type = NodeType.Array,
                        Name = aname,
                        SoftDatatype = varType.SoftDatatype,
                        AccessId = id
                    };

                    node.Children.Add(arraynode);
                }

                xx[0]++;

                // BBOOL-Arrays on overflow the ID of the lowest array index goes only up to 8.
                if (node.SoftDatatype == SoftDatatype.BBool && xx[0] >= offsetInfo.MDimElementCounts[0])
                {
                    if (offsetInfo.MDimElementCounts[0] % 8 != 0)
                    {
                        id += 8 - (xx[0] % 8);
                    }
                }
                for (int dim = 0; dim < 5; dim++)
                {
                    if (xx[dim] >= offsetInfo.MDimElementCounts[dim])
                    {
                        xx[dim] = 0;
                        xx[dim + 1]++;
                    }
                }
                id++;
                n++;
            } while (n <= offsetInfo.ArrayElementCount);
        }

        private void ProcessRelation(S7VarType varType, Node node)
        {
            var offsetInfo = (IOffsetInfoRelation)varType.OffsetInfo;

            foreach (var obj in _infoObjects)
            {
                if (obj.RelationId == offsetInfo.RelationId)
                {
                    AddSubNodes(node, obj);
                    break;
                }
            }
        }

        private static bool IsKnownDatatype(UInt32 softDatatype)
        {
            return softDatatype switch
            {
                SoftDatatype.Bool or
                SoftDatatype.Byte or
                SoftDatatype.Char or
                SoftDatatype.Word or
                SoftDatatype.Int or
                SoftDatatype.Dword or
                SoftDatatype.Dint or
                SoftDatatype.Real or
                SoftDatatype.Date or
                SoftDatatype.TimeOfDay or
                SoftDatatype.Time or
                SoftDatatype.S5Time or
                SoftDatatype.DateAndTime or
                SoftDatatype.String or
                SoftDatatype.Pointer or
                SoftDatatype.Any or
                SoftDatatype.BlockFb or
                SoftDatatype.BlockFc or
                SoftDatatype.Counter or
                SoftDatatype.Timer or
                SoftDatatype.BBool or
                SoftDatatype.LReal or
                SoftDatatype.Ulint or
                SoftDatatype.Lint or
                SoftDatatype.Lword or
                SoftDatatype.Usint or
                SoftDatatype.Uint or
                SoftDatatype.Udint or
                SoftDatatype.Sint or
                SoftDatatype.Wchar or
                SoftDatatype.Wstring or
                SoftDatatype.LTime or
                SoftDatatype.LTod or
                SoftDatatype.Ldt or
                SoftDatatype.Dtl or
                SoftDatatype.Remote or
                SoftDatatype.AomIdent or
                SoftDatatype.EventAny or
                SoftDatatype.EventAtt or
                SoftDatatype.Folder or
                SoftDatatype.AomLink or
                SoftDatatype.HwAny or
                SoftDatatype.HwIoSystem or
                SoftDatatype.HwDpMaster or
                SoftDatatype.HwDevice or
                SoftDatatype.HwDpSlave or
                SoftDatatype.HwIo or
                SoftDatatype.HwModule or
                SoftDatatype.HwSubModule or
                SoftDatatype.HwHsc or
                SoftDatatype.HwPwm or
                SoftDatatype.HwPto or
                SoftDatatype.HwInterface or
                SoftDatatype.HwIePort or
                SoftDatatype.ObAny or
                SoftDatatype.ObDelay or
                SoftDatatype.ObTod or
                SoftDatatype.ObCyclic or
                SoftDatatype.ObAtt or
                SoftDatatype.ConnAny or
                SoftDatatype.ConnPrg or
                SoftDatatype.ConnOuc or
                SoftDatatype.ConnRid or
                SoftDatatype.Port or
                SoftDatatype.Rtm or
                SoftDatatype.Pip or
                SoftDatatype.ObPCycle or
                SoftDatatype.ObHwInt or
                SoftDatatype.ObDiag or
                SoftDatatype.ObTimeError or
                SoftDatatype.ObStartup or
                SoftDatatype.DbAny or
                SoftDatatype.DbWww or
                SoftDatatype.DbDyn => true,
                _ => false,
            };
        }

        private class Node
        {
            public NodeType Type { get; set; } = NodeType.Undefined;
            public string Name { get; set; } = "";
            public UInt32 AccessId { get; set; }
            public UInt32 SoftDatatype { get; set; }
            public UInt32 RelationId { get; set; }
            public List<Node> Children { get; set; } = new List<Node>();
        }

        private enum NodeType
        {
            Undefined,
            Root,
            Variable,
            Array,
            StructArray
        }
    }
}