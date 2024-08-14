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

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug) // Set minimum log level for all loggers
        .AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Debug; // Set log level for console
        });
});

IS7Driver client = new S7Driver(loggerFactory.CreateLogger<S7Client>());
client.SetTimeout(TimeSpan.FromSeconds(5));
await client.Connect("192.168.178.140", 102);
Console.WriteLine("Hello World!");

GetMultiVariablesRequest request = new GetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address>{
    new S7AbsoluteAddress(1, 9)
});

GetMultiVariablesResponse getMultiVariablesResponse = await client.GetMultiVariables(request);

SetMultiVariablesRequest setMultiVariablesRequest = new SetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address> { new S7AbsoluteAddress(1, 9) },
    new List<S7VariableBase>{
        new S7VariableBool(false)
    });

SetMultiVariablesResponse setMultiResponse = await client.SetMultiVariables(setMultiVariablesRequest);

List<Datablock> datablocks = await client.GetDatablocks();
VariableInfo? variableInfo = await client.GetVariableInfoBySymbol("DB1.TEST_DINT", datablocks);

request = new GetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address>{
    variableInfo!.Address
});

getMultiVariablesResponse = await client.GetMultiVariables(request);

Console.Read();
await client.Disconnect();
Console.WriteLine("Goodbye World!");