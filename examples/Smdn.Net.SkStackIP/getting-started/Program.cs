using System;
using System.IO.Ports;
using System.Text;

using Smdn.Net.SkStackIP;

// Open the serial port to which the device implementing SKSTACK-IP is connected
using var port = new SerialPort(
  portName: "/dev/ttyACM0", // Specify a port name such as COM1 on Windows
  baudRate: 115200, // Specify the appropriate baud rate for the device
  parity: Parity.None,
  dataBits: 8,
  stopBits: StopBits.One
) {
  Handshake = Handshake.None,
  DtrEnable = false,
  RtsEnable = false,
  NewLine = "\r\n", // CRLF
};

port.Open();
port.DiscardInBuffer(); // Discard previous buffer contents before communication starts.

// Create a SkStackClient instance based on a stream of the SerialPort instance
using var client = new SkStackClient(stream: port.BaseStream);

// Send SKVER command to get and display the firmware version of SKSTACK IP
var respSKVER = await client.SendSKVERAsync();

Console.WriteLine($"VER: {respSKVER.Payload}");

// Send SKAPPVER command to get and display the firmware version of application
var respSKAPPVER = await client.SendSKAPPVERAsync();

Console.WriteLine($"APPVER: {respSKAPPVER.Payload}");

// Send SKINFO command to get and display the communication configurations.
var respSKINFO = await client.SendSKINFOAsync();

Console.WriteLine($"LinkLocalAddress: {respSKINFO.Payload.LinkLocalAddress}");
Console.WriteLine($"MacAddress: {respSKINFO.Payload.MacAddress}");
Console.WriteLine($"Channel: {respSKINFO.Payload.Channel}");
Console.WriteLine($"PanId: {respSKINFO.Payload.PanId:X4}");
