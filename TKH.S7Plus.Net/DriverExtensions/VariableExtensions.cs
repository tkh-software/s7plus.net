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

using TKH.S7Plus.Net.Models;
using TKH.S7Plus.Net.Requests;
using TKH.S7Plus.Net.Responses;
using TKH.S7Plus.Net.S7Variables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace TKH.S7Plus.Net.DriverExtensions
{
    public static class DriverVariableExtensions
    {
        public static async Task<S7VariableBase> GetVariable(this IS7Driver driver, IS7Address address)
        {
            GetMultiVariablesRequest request = new GetMultiVariablesRequest(new List<IS7Address> { address });
            GetMultiVariablesResponse response = await driver.GetMultiVariables(request);

            if (response.ErrorValues.Any())
                throw new Exception("Error reading variable: " + response.ErrorValues.First().Value);

            return response.Values.Values.FirstOrDefault() ?? throw new Exception("Variable not found");
        }

        public static async Task<List<S7VariableBase>> GetVariables(this IS7Driver driver, List<IS7Address> addresses)
        {
            GetMultiVariablesRequest request = new GetMultiVariablesRequest(addresses);
            GetMultiVariablesResponse response = await driver.GetMultiVariables(request);

            if (response.ErrorValues.Any())
                throw new Exception("Error reading variables: " + string.Join(",", response.ErrorValues.Values));

            return response.Values.Values.ToList();
        }

        public static async Task SetVariable(this IS7Driver driver, IS7Address address, S7VariableBase value)
        {
            SetMultiVariablesRequest request = new SetMultiVariablesRequest(new List<IS7Address> { address }, new List<S7VariableBase> { value });
            SetMultiVariablesResponse response = await driver.SetMultiVariables(request);

            if (response.ErrorValues.Any())
                throw new Exception("Error writing variable: " + response.ErrorValues.First().Value);
        }

        public static async Task SetVariables(this IS7Driver driver, List<IS7Address> addresses, List<S7VariableBase> values)
        {
            SetMultiVariablesRequest request = new SetMultiVariablesRequest(addresses, values);
            SetMultiVariablesResponse response = await driver.SetMultiVariables(request);

            if (response.ErrorValues.Any())
                throw new Exception("Error writing variables: " + string.Join(",", response.ErrorValues.Values));
        }
    }
}