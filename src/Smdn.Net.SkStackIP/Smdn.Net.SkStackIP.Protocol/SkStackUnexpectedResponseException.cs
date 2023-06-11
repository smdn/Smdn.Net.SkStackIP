// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;
using System.Buffers;

using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP.Protocol;

public class SkStackUnexpectedResponseException : SkStackResponseException {
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
    ReadOnlySpan<byte> text,
    Exception? innerException = null
  )
    => new(
      causedText: text.ToControlCharsPicturizedString(),
      message: $"unexpected response format: '{text.ToControlCharsPicturizedString()}'",
      innerException: innerException
    );

  internal static SkStackUnexpectedResponseException CreateInvalidToken(
    ReadOnlySpan<byte> token,
    string expectedFormat,
    Exception? innerException = null
  )
    => new(
      causedText: token.ToControlCharsPicturizedString(),
      message: $"unexpected response token: '{token.ToControlCharsPicturizedString()}' ({expectedFormat})",
      innerException: innerException
    );

  internal static SkStackUnexpectedResponseException CreateInvalidToken(
    ReadOnlySequence<byte> token,
    string expectedFormat,
    Exception? innerException = null
  )
    => new(
      causedText: token.ToControlCharsPicturizedString(),
      message: $"unexpected response token: '{token.ToControlCharsPicturizedString()}' ({expectedFormat})",
      innerException: innerException
    );

  internal static SkStackUnexpectedResponseException CreateInvalidToken(
    string causedText,
    string expectedFormat,
    Exception? innerException = null
  )
    => new(
      causedText: causedText,
      message: $"unexpected response token: '{causedText}' ({expectedFormat})'",
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
