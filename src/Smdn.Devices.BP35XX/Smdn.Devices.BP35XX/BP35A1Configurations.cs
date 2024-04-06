// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using Smdn.Net.SkStackIP;

namespace Smdn.Devices.BP35XX;

public sealed class BP35A1Configurations : IBP35Configurations {
  /// <inheritdoc cref="IBP35Configurations.SerialPortName"/>
  public string? SerialPortName { get; set; }

  /// <inheritdoc cref="IBP35Configurations.BaudRate"/>
  public BP35UartBaudRate BaudRate { get; set; } = BP35A1.DefaultValueForBP35UartBaudRate;

  /// <inheritdoc cref="IBP35Configurations.TryLoadFlashMemory"/>
  public bool TryLoadFlashMemory { get; set; } = BP35Base.DefaultValueForTryLoadFlashMemory;

  SkStackERXUDPDataFormat IBP35Configurations.ERXUDPDataFormat => SkStackERXUDPDataFormat.Binary;
}
