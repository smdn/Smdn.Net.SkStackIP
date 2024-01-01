// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that is thrown when the <see cref="SkStackClient"/> received an error response.
/// </summary>
public class SkStackErrorResponseException : SkStackResponseException {
  /// <summary>
  /// Gets the <see cref="SkStackResponse"/> that caused the exception.
  /// </summary>
  public SkStackResponse Response { get; }

  /// <summary>
  /// Gets the <see cref="SkStackErrorCode"/> that caused the exception.
  /// </summary>
  public SkStackErrorCode ErrorCode { get; }

  /// <summary>
  /// Gets the <see langword="string"/> that describes the reason of the error.
  /// </summary>
  public string ErrorText { get; }

  internal SkStackErrorResponseException(
    SkStackResponse response,
    SkStackErrorCode errorCode,
    ReadOnlySpan<byte> errorText,
    string message,
    Exception? innerException = null
  )
    : base(
      message: errorText.IsEmpty
        ? $"{message} [{errorCode}]"
        : $"{message} [{errorCode}] \"{SkStack.GetString(errorText)}\"",
      innerException: innerException
    )
  {
    Response = response;
    ErrorCode = errorCode;
    ErrorText = SkStack.GetString(errorText);
  }
}
