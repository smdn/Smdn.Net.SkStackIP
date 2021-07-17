// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  public class SkStackErrorResponseException : SkStackResponseException {
    public SkStackResponse Response { get; }
    public SkStackErrorCode ErrorCode { get; }
    public string ErrorText { get; }

    public SkStackErrorResponseException(string message, Exception innerException = null)
      : base(message: message, innerException: innerException)
    {
    }

    internal SkStackErrorResponseException(
      SkStackResponse response,
      SkStackErrorCode errorCode,
      ReadOnlySpan<byte> errorText,
      string message,
      Exception innerException = null
    )
      : base(
        message: errorText.IsEmpty
          ? $"{message} [{errorCode}]"
          : $"{message} [{errorCode}] \"{SkStack.DefaultEncoding.GetString(errorText)}\"",
        innerException: innerException
      )
    {
      this.Response = response;
      this.ErrorCode = errorCode;
      this.ErrorText = SkStack.DefaultEncoding.GetString(errorText);
    }
  }
}