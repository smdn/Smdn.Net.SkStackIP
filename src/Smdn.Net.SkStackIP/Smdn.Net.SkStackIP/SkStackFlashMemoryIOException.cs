// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Net.SkStackIP {
  public class SkStackFlashMemoryIOException : SkStackErrorResponseException {
    internal SkStackFlashMemoryIOException(
      SkStackResponse response,
      SkStackErrorCode errorCode,
      ReadOnlySpan<byte> errorText,
      string message
    )
      : base(
        response: response,
        errorCode: errorCode,
        errorText: errorText,
        message: message
      )
    {
    }
  }
}