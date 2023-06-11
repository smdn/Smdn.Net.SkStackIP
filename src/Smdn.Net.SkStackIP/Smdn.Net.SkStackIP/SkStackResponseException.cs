// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Net.SkStackIP;

public class SkStackResponseException : InvalidOperationException {
  public SkStackResponseException()
    : base()
  {
  }

  public SkStackResponseException(string message)
    : base(message: message)
  {
  }

  public SkStackResponseException(string message, Exception? innerException = null)
    : base(message: message, innerException: innerException)
  {
  }
}
