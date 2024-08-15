using Microsoft.Extensions.Logging;

using TKH.S7Plus.Net;
using TKH.S7Plus.Net.Constants;
using TKH.S7Plus.Net.DriverExtensions;
using TKH.S7Plus.Net.Models;
using TKH.S7Plus.Net.Requests;
using TKH.S7Plus.Net.Responses;
using TKH.S7Plus.Net.S7Variables;

using System;
using System.Collections.Generic;
using System.Linq;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug) // Set minimum log level for all loggers
        .AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Debug; // Set log level for console
        });
});

IS7Driver driver = new S7Driver(loggerFactory.CreateLogger<S7Client>());
driver.SetTimeout(TimeSpan.FromSeconds(5));
await driver.Connect("192.168.178.140", 102);
Console.WriteLine("Hello World!");

GetMultiVariablesRequest request = new GetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address>{
    new S7AbsoluteAddress(1, 9)
});

GetMultiVariablesResponse getMultiVariablesResponse = await driver.GetMultiVariables(request);

SetMultiVariablesRequest setMultiVariablesRequest = new SetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address> { new S7AbsoluteAddress(1, 9) },
    new List<S7VariableBase>{
        new S7VariableBool(false)
    });

SetMultiVariablesResponse setMultiResponse = await driver.SetMultiVariables(setMultiVariablesRequest);

List<Datablock> datablocks = await driver.GetDatablocks();
VariableInfo? variableInfo = await driver.GetVariableInfoBySymbol("DB1.TEST_DINT", datablocks);

request = new GetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address>{
    variableInfo!.Address
});

getMultiVariablesResponse = await driver.GetMultiVariables(request);

Console.WriteLine($"Value of DB1.TEST_DINT: {((S7VariableDInt)getMultiVariablesResponse.Values.First().Value).Value}");

variableInfo = await driver.GetVariableInfoBySymbol("DB1.TEST_ARRAY", datablocks);
request = new GetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address>{
    variableInfo!.Address
});

getMultiVariablesResponse = await driver.GetMultiVariables(request);

S7VariableDIntArray dintArray = (S7VariableDIntArray)getMultiVariablesResponse.Values.First().Value;
Console.WriteLine($"Value of DB1.TEST_ARRAY: {string.Join(", ", dintArray.Value)}");

await driver.Disconnect();
Console.WriteLine("Goodbye World!");