// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Net.SkStackIP {
  public class SkStackErrorResponseException : SkStackResponseException {
    public SkStackResponse Response { get; }
    public string ErrorCode { get; }
    public string ErrorText { get; }
    public SkStackErrorResponseException(string message)
      : base(message)
    {
    }

    public SkStackErrorResponseException(
      SkStackResponse response,
      ReadOnlySpan<byte> errorCode,
      ReadOnlySpan<byte> errorText,
      string message
    )
      : this(
        response,
        Encoding.ASCII.GetString(errorCode),
        Encoding.ASCII.GetString(errorText),
        message
      )
    {
    }

    private SkStackErrorResponseException(
      SkStackResponse response,
      string errorCode,
      string errorText,
      string message
    )
      : base(
        string.IsNullOrEmpty(errorText)
          ? $"{message} [{errorCode}]"
          : $"{message} [{errorCode}] ({errorText})"
      )
    {
      this.Response = response;
      this.ErrorCode = errorCode;
      this.ErrorText = errorText;
    }
  }
}