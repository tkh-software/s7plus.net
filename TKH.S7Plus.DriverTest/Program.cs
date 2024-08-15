using Microsoft.Extensions.Logging;

using TKH.S7Plus.Net;
using TKH.S7Plus.Net.DriverExtensions;
using TKH.S7Plus.Net.Models;
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

IS7Driver driver = new S7Driver(loggerFactory.CreateLogger<S7Client>());
driver.SetTimeout(TimeSpan.FromSeconds(5));
await driver.Connect("192.168.178.140", 102);
Console.WriteLine("Hello World!");

await driver.SetVariable(new S7AbsoluteAddress(1, 9), new S7VariableBool(true));

List<Datablock> datablocks = await driver.GetDatablocks();
VariableInfo? variableInfo = await driver.GetVariableInfoBySymbol("DB1.TEST_DINT", datablocks);

var variableResult = await driver.GetVariable(variableInfo!.Address);

Console.WriteLine($"Value of DB1.TEST_DINT: {((S7VariableDInt)variableResult).Value}");

variableInfo = await driver.GetVariableInfoBySymbol("DB1.TEST_ARRAY", datablocks);
variableResult = await driver.GetVariable(variableInfo!.Address);

S7VariableDIntArray dintArray = (S7VariableDIntArray)variableResult;
Console.WriteLine($"Value of DB1.TEST_ARRAY: {string.Join(", ", dintArray.Value)}");

await driver.Disconnect();
Console.WriteLine("Goodbye World!");