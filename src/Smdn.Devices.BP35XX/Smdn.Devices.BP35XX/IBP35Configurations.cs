// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using Smdn.Net.SkStackIP;

namespace Smdn.Devices.BP35XX;

public interface IBP35Configurations {
  /// <summary>
  /// Gets the <see cref="string"/> value that holds the serial port name for communicating with the device that implements the SKSTACK-IP protocol.
  /// </summary>
  string? SerialPortName { get; }

  /// <summary>
  /// Gets the <see cref="BP35UartBaudRate"/> value that specifies the baud rate of the serial port for communicating with the device.
  /// </summary>
  BP35UartBaudRate BaudRate { get; }

  /// <summary>
  /// Gets a value indicating whether or not to attempt to load the configuration from flash memory during initialization.
  /// </summary>
  bool TryLoadFlashMemory { get; }

  /// <summary>
  /// Gets the value that specifies the format of the data part received in the event <c>ERXUDP</c>. See <see cref="SkStackClient.ERXUDPDataFormat"/>.
  /// </summary>
  /// <seealso cref="SkStackClient.ERXUDPDataFormat"/>
  /// <seealso cref="SkStackERXUDPDataFormat"/>
  SkStackERXUDPDataFormat ERXUDPDataFormat { get; }
}
