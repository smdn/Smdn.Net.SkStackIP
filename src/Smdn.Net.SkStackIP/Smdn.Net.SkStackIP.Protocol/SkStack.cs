// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Text;

#if !SYSTEM_TEXT_ENCODINGEXTENSIONS
using Smdn.Text.Encodings; // EncodingReadOnlySequenceExtensions
#endif

namespace Smdn.Net.SkStackIP.Protocol {
  internal static class SkStack {
    public static readonly Encoding DefaultEncoding = Encoding.ASCII;

    public static byte[] ToByteSequence(string text)
      => DefaultEncoding.GetBytes(text);

    public static string GetString(ReadOnlySequence<byte> sequence)
      => DefaultEncoding.GetString(sequence);

    private static readonly ReadOnlyMemory<byte> crlf = new[] {(byte)'\r', (byte)'\n'};
    internal static ReadOnlyMemory<byte> CRLFMemory => crlf;
    public static ReadOnlySpan<byte> CRLFSpan => crlf.Span;

    public const byte SP = 0x20;
  }
}