// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.IO.Ports;

namespace Smdn.Devices.BP35XX;

#pragma warning disable IDE0040
partial class BP35Base {
#pragma warning restore IDE0040
  private protected abstract class SerialPortStreamFactory : IBP35SerialPortStreamFactory {
    public abstract BP35UartBaudRate BaudRate { get; }
    public abstract bool UseFlowControl { get; }

    public void Dispose()
    {
      // nothing to do in this class
    }

    public Stream CreateSerialPortStream(string? serialPortName)
    {
      if (string.IsNullOrEmpty(serialPortName)) {
        throw new ArgumentException(
          message: $"The {nameof(serialPortName)} must be a non-empty string.",
          paramName: nameof(serialPortName)
        );
      }

      const string CRLF = "\r\n";

#pragma warning disable CA2000
      var port = new SerialPort(
        portName: serialPortName,
        baudRate: BaudRate switch {
          BP35UartBaudRate.Baud2400 => 2_400,
          BP35UartBaudRate.Baud4800 => 4_800,
          BP35UartBaudRate.Baud9600 => 9_600,
          BP35UartBaudRate.Baud19200 => 19_200,
          BP35UartBaudRate.Baud38400 => 38_400,
          BP35UartBaudRate.Baud57600 => 57_600,
          BP35UartBaudRate.Baud115200 => 115_200,
          _ => throw new InvalidOperationException($"A valid {nameof(BP35UartBaudRate)} value is not set for the {nameof(BaudRate)}"),
        },
        parity: Parity.None,
        dataBits: 8,
        stopBits: StopBits.One
      ) {
        Handshake = UseFlowControl ? Handshake.RequestToSend : Handshake.None,
        DtrEnable = false,
        RtsEnable = UseFlowControl,
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
