// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Net.SkStackIP {
  public class SkStackUnexpectedResponseException : SkStackResponseException {
    public SkStackUnexpectedResponseException(string message)
      : base(message)
    {
    }

    internal static SkStackUnexpectedResponseException CreateLackOfExpectedResponseText()
      => new SkStackUnexpectedResponseException($"lack of expected response text");

    internal static SkStackUnexpectedResponseException CreateInvalidFormat(ReadOnlySpan<byte> text)
      => new SkStackUnexpectedResponseException($"unexpected response format: {Encoding.ASCII.GetString(text)}");

    internal static SkStackUnexpectedResponseException CreateInvalidToken(ReadOnlySpan<byte> token, string expectedFormat)
      => new SkStackUnexpectedResponseException($"unexpected response token: {Encoding.ASCII.GetString(token)} ({expectedFormat})");
  }
}