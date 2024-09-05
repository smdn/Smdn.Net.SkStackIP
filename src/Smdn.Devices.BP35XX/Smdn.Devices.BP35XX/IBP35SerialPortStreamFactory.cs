// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.IO;

namespace Smdn.Devices.BP35XX;

public interface IBP35SerialPortStreamFactory {
  Stream CreateSerialPortStream(string? serialPortName);
}
