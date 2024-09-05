// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;

namespace Smdn.Devices.BP35XX;

public interface IBP35SerialPortStreamFactory : IDisposable {
  Stream CreateSerialPortStream(string? serialPortName);
}
