// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP.Protocol {
  public class SkStackUnexpectedResponseException : SkStackResponseException {
    public string CausedText { get; }

    public SkStackUnexpectedResponseException(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }

    private SkStackUnexpectedResponseException(string causedText, string message, Exception innerException = null)
      : base(message, innerException)
    {
      this.CausedText = causedText;
    }

    internal static SkStackUnexpectedResponseException CreateLackOfExpectedResponseText(Exception innerException = null)
      => new SkStackUnexpectedResponseException($"lack of expected response text", innerException);

    internal static SkStackUnexpectedResponseException CreateInvalidFormat(
      ReadOnlySpan<byte> text,
      Exception innerException = null
    )
      => new SkStackUnexpectedResponseException(
        causedText: text.ToControlCharsPicturizedString(),
        message: $"unexpected response format: '{text.ToControlCharsPicturizedString()}'",
        innerException: innerException
      );

    internal static SkStackUnexpectedResponseException CreateInvalidToken(
      ReadOnlySpan<byte> token,
      string expectedFormat,
      Exception innerException = null
    )
      => new SkStackUnexpectedResponseException(
        causedText: token.ToControlCharsPicturizedString(),
        message: $"unexpected response token: '{token.ToControlCharsPicturizedString()}' ({expectedFormat})",
        innerException: innerException
      );

    internal static SkStackUnexpectedResponseException CreateInvalidToken(
      ReadOnlySequence<byte> token,
      string expectedFormat,
      Exception innerException = null
    )
      => new SkStackUnexpectedResponseException(
        causedText: token.ToControlCharsPicturizedString(),
        message: $"unexpected response token: '{token.ToControlCharsPicturizedString()}' ({expectedFormat})",
        innerException: innerException
      );

    internal static SkStackUnexpectedResponseException CreateInvalidToken(
      string causedText,
      string expectedFormat,
      Exception innerException = null
    )
      => new SkStackUnexpectedResponseException(
        causedText: causedText,
        message: $"unexpected response token: '{causedText}' ({expectedFormat})'",
        innerException: innerException
      );

    internal static void ThrowIfUnexpectedSubsequentEventCode(
      SkStackEvent ev,
      SkStackEventCode expectedEventCode
    )
    {
      if (ev.ExpectedSubsequentEventCode != expectedEventCode) {
        throw new SkStackUnexpectedResponseException(
          message: $"expected subsequent event code is {expectedEventCode}, but was {ev.ExpectedSubsequentEventCode}"
        );
      }
    }
  }
}