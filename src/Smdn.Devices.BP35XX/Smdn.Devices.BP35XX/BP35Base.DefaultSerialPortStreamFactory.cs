// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.IO.Ports;

namespace Smdn.Devices.BP35XX;

#pragma warning disable IDE0040
partial class BP35Base {
#pragma warning restore IDE0040
  private class DefaultSerialPortStreamFactory : IBP35SerialPortStreamFactory {
    public static DefaultSerialPortStreamFactory Instance { get; } = new();

    public Stream CreateSerialPortStream(IBP35Configurations configurations)
    {
      if (string.IsNullOrEmpty(configurations.SerialPortName)) {
        throw new ArgumentException(
          message: $"The {nameof(configurations.SerialPortName)} is not set for the {configurations.GetType().Name}",
          paramName: nameof(configurations)
        );
      }

      const string CRLF = "\r\n";

#pragma warning disable CA2000
      var port = new SerialPort(
        portName: configurations.SerialPortName,
        baudRate: configurations.BaudRate switch {
          BP35UartBaudRate.Baud2400 => 2_400,
          BP35UartBaudRate.Baud4800 => 4_800,
          BP35UartBaudRate.Baud9600 => 9_600,
          BP35UartBaudRate.Baud19200 => 19_200,
          BP35UartBaudRate.Baud38400 => 38_400,
          BP35UartBaudRate.Baud57600 => 57_600,
          BP35UartBaudRate.Baud115200 => 115_200,
          _ => throw new ArgumentException(
            message: $"A valid {nameof(BP35UartBaudRate)} value is not set for the {configurations.GetType().Name}",
            paramName: nameof(configurations)
          ),
        },
        parity: Parity.None,
        dataBits: 8,
        stopBits: StopBits.One
      ) {
        Handshake = Handshake.None, // TODO: RequestToSend
        DtrEnable = false,
        RtsEnable = false,
        NewLine = CRLF,
      };
#pragma warning restore CA2000

      port.Open();

      // discard input buffer to avoid reading previously received data
      port.DiscardInBuffer();

      return port.BaseStream;
    }
  }
}
