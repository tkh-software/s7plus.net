using System;
using S7Plus.Net;

S7Client client = new S7Client();
client.SetTimeout(TimeSpan.FromSeconds(5));
await client.ConnectAsync("192.168.178.140", 102, TimeSpan.FromSeconds(5));
Console.WriteLine("Hello World!");

//Console.Read();
await client.DisconnectAsync();
client.Dispose();
Console.WriteLine("Goodbye World!");
