// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP {
  public abstract class SkStackResponseException : InvalidOperationException {
    protected SkStackResponseException(string message, Exception innerException = null)
      : base(message: message, innerException: innerException)
    {
    }
  }
}