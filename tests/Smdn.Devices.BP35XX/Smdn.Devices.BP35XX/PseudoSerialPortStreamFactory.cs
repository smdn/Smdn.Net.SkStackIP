// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.IO;

namespace Smdn.Devices.BP35XX;

internal class PseudoSerialPortStreamFactory : IBP35SerialPortStreamFactory {
  public PseudoSkStackStream Stream { get; } = new();

  public PseudoSerialPortStreamFactory()
  {
  }

  public void Dispose()
  {
    // nothing to do in this class
  }

  public Stream CreateSerialPortStream(string? serialPortName)
    => Stream;
}
