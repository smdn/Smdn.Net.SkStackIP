// SPDX-FileCopyrightText: 2025 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.BP35XX;

public
/* sealed */ // TODO: remove BP35A1Configurations and seal BP35A1Options
class BP35A1Options {
  /// <summary>
  /// Gets or sets the <see cref="string"/> value that holds the serial port name for communicating with the device that implements the SKSTACK-IP protocol.
  /// </summary>
  public string? SerialPortName { get; set; }

  /// <summary>
  /// Gets or sets the <see cref="BP35UartBaudRate"/> value that specifies the baud rate of the serial port for communicating with the device.
  /// </summary>
  public BP35UartBaudRate BaudRate { get; set; } = BP35A1.DefaultValueForBP35UartBaudRate;

  /// <summary>
  /// Gets or sets a value indicating whether or not to use the Request-to-Send (RTS) hardware flow control for communicating with the device.
  /// </summary>
  public bool UseFlowControl { get; set; } = BP35A1.DefaultValueForUseFlowControl;

  /// <summary>
  /// Gets or sets a value indicating whether or not to attempt to load the configuration from flash memory during initialization.
  /// </summary>
  public bool TryLoadFlashMemory { get; set; } = BP35Base.DefaultValueForTryLoadFlashMemory;

  /// <summary>
  /// Configure this instance to have the same values as the instance passed as an argument.
  /// </summary>
  /// <param name="baseOptions">
  /// A <see cref="BP35A1Options"/> that holds the values that are used to configure this instance.
  /// </param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="baseOptions"/> is <see langword="null"/>.
  /// </exception>
  /// <returns>
  /// The current <see cref="BP35A1Options"/> so that additional calls can be chained.
  /// </returns>
  public BP35A1Options Configure(BP35A1Options baseOptions)
  {
    if (baseOptions is null)
      throw new ArgumentNullException(nameof(baseOptions));

    SerialPortName = baseOptions.SerialPortName;
    BaudRate = baseOptions.BaudRate;
    UseFlowControl = baseOptions.UseFlowControl;
    TryLoadFlashMemory = baseOptions.TryLoadFlashMemory;

    return this;
  }
}
