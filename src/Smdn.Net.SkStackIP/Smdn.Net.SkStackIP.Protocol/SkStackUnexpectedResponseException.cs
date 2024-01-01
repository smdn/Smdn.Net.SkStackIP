// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;
using System.Buffers;

using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP.Protocol;

/// <summary>
/// The exception that is thrown when the <see cref="SkStackClient"/> received unexpected response.
/// </summary>
/// <seealso cref="SkStackTokenParser"/>
public class SkStackUnexpectedResponseException : SkStackResponseException {
  /// <summary>
  /// Gets the token or text of the response that caused the exception.
  /// </summary>
  public string? CausedText { get; }

  private SkStackUnexpectedResponseException(string? causedText, string message, Exception? innerException = null)
    : base(message, innerException)
  {
    CausedText = causedText;
  }

  internal static SkStackUnexpectedResponseException CreateLackOfExpectedResponseText(Exception? innerException = null)
    => new(
      causedText: null,
      message: "lack of expected response text",
      innerException: innerException
    );

  internal static SkStackUnexpectedResponseException CreateInvalidFormat(
    ReadOnlySequence<byte> token,
    Exception? innerException = null
  )
    => new(
      causedText: token.ToControlCharsPicturizedString(),
      message: $"unexpected response format: '{token.ToControlCharsPicturizedString()}'",
      innerException: innerException
    );

  internal static SkStackUnexpectedResponseException CreateInvalidToken(
    ReadOnlySpan<byte> token,
    string extraMessage,
    Exception? innerException = null
  )
    => new(
      causedText: token.ToControlCharsPicturizedString(),
      message: $"unexpected response token: '{token.ToControlCharsPicturizedString()}' ({extraMessage})",
      innerException: innerException
    );

  internal static SkStackUnexpectedResponseException CreateInvalidToken(
    ReadOnlySequence<byte> token,
    string extraMessage,
    Exception? innerException = null
  )
    => new(
      causedText: token.ToControlCharsPicturizedString(),
      message: $"unexpected response token: '{token.ToControlCharsPicturizedString()}' ({extraMessage})",
      innerException: innerException
    );

  internal static void ThrowIfUnexpectedSubsequentEventCode(
    SkStackEventCode subsequentEventCode,
    SkStackEventCode expectedEventCode
  )
  {
    if (subsequentEventCode != expectedEventCode) {
      throw new SkStackUnexpectedResponseException(
        causedText: null,
        message: $"expected subsequent event code is {expectedEventCode}, but was {subsequentEventCode}",
        innerException: null
      );
    }
  }
}
