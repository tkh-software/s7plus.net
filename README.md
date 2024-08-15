![Build Status](https://github.com/tkh-software/s7plus.net/actions/workflows/build.yml/badge.svg)

# S7Plus.NET

S7Plus.NET is a .NET library for communicating with Siemens S7-1200 and S7-1500 PLCs.
The library is based on the excellent work of Thomas Wiens in the S7CommPlusDriver project.
(https://github.com/thomas-v2/S7CommPlusDriver)

## Installation

The library is available as a NuGet package. You can install it using the following command:

```bash
dotnet add package TKH.S7Plus.Net
```

## Usage

### Connecting to the PLC

- Create an instance of the S7Driver and connect to the PLC:

  ```csharp
  using TKH.S7Plus.Net;
  using System;

  IS7Driver driver = new S7Driver();
  driver.SetTimeout(TimeSpan.FromSeconds(5));
  await driver.Connect("192.168.178.140", 102);
  ```

### Writing Variables

- Write a boolean variable to the PLC:

  ```csharp
  using TKH.S7Plus.Net.Models;
  using TKH.S7Plus.Net.S7Variables;

  await driver.SetVariable(new S7AbsoluteAddress(1, 9), new S7VariableBool(true));
  ```

### Reading Variables

- Read a variable from the PLC using the symbol name:

  ```csharp
    List<Datablock> datablocks = await driver.GetDatablocks();
    VariableInfo? variableInfo = await driver.GetVariableInfoBySymbol("DB1.TEST_DINT", datablocks);
    var variableResult = await driver.GetVariable(variableInfo!.Address);
  ```

- Read an array variable from the PLC using the symbol name:

  ```csharp
    variableInfo = await driver.GetVariableInfoBySymbol("DB1.TEST_ARRAY", datablocks);
    variableResult = await driver.GetVariable(variableInfo!.Address);
    S7VariableDIntArray dintArray = (S7VariableDIntArray)variableResult;
    Console.WriteLine($"Value of DB1.TEST_ARRAY: {string.Join(", ", dintArray.Value)}");
  ```

### Disconnecting from the PLC

- Disconnect from the PLC:

  ```csharp
  await driver.Disconnect();
  ```

## Commercially Supported

This library is commercially supported by [TKH Software](https://tkh-software.com/).
Don't hesitate contacting us if you're building something large, in need of advice or having other business inquiries in mind.

## License

This project is licensed under the [LGPL-3.0 License](./LICENSE).
