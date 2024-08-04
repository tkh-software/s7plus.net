using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using S7Plus.Net;
using S7Plus.Net.Constants;
using S7Plus.Net.Models;
using S7Plus.Net.Requests;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug) // Set minimum log level for all loggers
        .AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Debug; // Set log level for console
        });
});

S7Client client = new S7Client(loggerFactory.CreateLogger<S7Client>());
client.SetTimeout(TimeSpan.FromSeconds(5));
await client.Connect("192.168.178.140", 102, TimeSpan.FromSeconds(10));
Console.WriteLine("Hello World!");

GetMultiVariablesRequest request = new GetMultiVariablesRequest(ProtocolVersion.V2, new List<IS7Address>{
    new S7AbsoluteAddress(1, 9)
});

byte[] response = await client.Send(request);

Console.Read();
await client.Disconnect();
client.Dispose();
Console.WriteLine("Goodbye World!");
