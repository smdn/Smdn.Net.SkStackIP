// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;

namespace Smdn.Devices.BP35XX;

/// <summary>
/// The exception that is thrown when an unexpected exception thrown in a call
/// to <see cref="IBP35SerialPortStreamFactory.CreateSerialPortStream"/>.
/// </summary>
public class BP35SerialPortException : IOException {
  public BP35SerialPortException()
    : base()
  {
  }

  public BP35SerialPortException(string message)
    : base(message: message)
  {
  }

  public BP35SerialPortException(string message, Exception? innerException = null)
    : base(message: message, innerException: innerException)
  {
  }
}
