// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Text;

using Smdn.Net.SkStackIP;

using var pseudoDeviceStream = new PseudoSkStackDeviceStream();

var pseudoDeviceResponseWriter = new StreamWriter(pseudoDeviceStream.GetWriterStream(), Encoding.ASCII) {
  NewLine = "\r\n",
  AutoFlush = true,
};

using var client = new SkStackClient(stream: pseudoDeviceStream);

// write response lines of SKVER command
pseudoDeviceResponseWriter.WriteLine("EVER 1.2.3");
pseudoDeviceResponseWriter.WriteLine("OK");

// send SKVER command
var respSKVER = await client.SendSKVERAsync();

Console.WriteLine($"VER: {respSKVER.Payload}");

// write response lines of SKAPPVER command
pseudoDeviceResponseWriter.WriteLine("EAPPVER pseudo-device");
pseudoDeviceResponseWriter.WriteLine("OK");

// send SKAPPVER command
var respSKAPPVER = await client.SendSKAPPVERAsync();

Console.WriteLine($"APPVER: {respSKAPPVER.Payload}");
