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

namespace TKH.S7Plus.Net.Helpers
{
    public static class SymbolParser
    {
        public static string GetRootLevel(string symbol)
        {
            if (symbol.StartsWith('"'))
            {
                int idx = symbol.IndexOf('"', 1);
                if (idx < 0)
                    throw new Exception("Symbol syntax error");

                return symbol.Substring(1, idx - 1);
            }
            else
            {
                int idx = symbol.IndexOf('.');
                int idx2 = symbol.IndexOf('[');

                if (idx2 > 0 && (idx < 0 || idx2 < idx))
                {
                    return symbol.Substring(0, idx2);
                }
                else
                {
                    if (idx > 0)
                        return symbol.Substring(0, idx);
                    else if (idx == 0)
                        throw new Exception("Symbol syntax error");
                    else
                        return symbol;
                }
            }
        }

        public static string GetNextLevel(string symbol)
        {
            if (symbol.StartsWith('"'))
            {
                int idx = symbol.IndexOf('"', 1);
                if (idx < 0)
                    throw new Exception("Symbol syntax error");

                return symbol.Substring(idx + 1);
            }
            else
            {
                int idx = symbol.IndexOf('.');
                int idx2 = symbol.IndexOf('[');

                if (idx2 > 0 && (idx < 0 || idx2 < idx))
                {
                    return symbol.Substring(idx2);
                }
                else
                {
                    if (idx > 0)
                        return symbol.Substring(idx + 1);
                    else if (idx == 0)
                        throw new Exception("Symbol syntax error");
                    else
                        return string.Empty;
                }
            }
        }
    }
}