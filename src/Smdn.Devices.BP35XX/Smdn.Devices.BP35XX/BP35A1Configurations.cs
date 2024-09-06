// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.BP35XX;

public sealed class BP35A1Configurations {
  /// <summary>
  /// Gets or sets the <see cref="string"/> value that holds the serial port name for communicating with the device that implements the SKSTACK-IP protocol.
  /// </summary>
  public string? SerialPortName { get; set; }

  /// <summary>
  /// Gets or sets the <see cref="BP35UartBaudRate"/> value that specifies the baud rate of the serial port for communicating with the device.
  /// </summary>
  public BP35UartBaudRate BaudRate { get; set; } = BP35A1.DefaultValueForBP35UartBaudRate;

  /// <summary>
  /// Gets or sets a value indicating whether or not to attempt to load the configuration from flash memory during initialization.
  /// </summary>
  public bool TryLoadFlashMemory { get; set; } = BP35Base.DefaultValueForTryLoadFlashMemory;
}
