// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Text;

#if !SYSTEM_TEXT_ENCODINGEXTENSIONS
using Smdn.Text.Encodings; // EncodingReadOnlySequenceExtensions
#endif

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStack {
  private static readonly Encoding DefaultEncoding = Encoding.ASCII;

  public static byte[] ToByteSequence(string text)
    => DefaultEncoding.GetBytes(text);

#if !SYSTEM_TEXT_ASCII
  public static int ToByteSequence(ReadOnlySpan<char> source, Span<byte> destination)
    => DefaultEncoding.GetBytes(source, destination);
#endif

  public static string GetString(ReadOnlySpan<byte> sequence)
    => DefaultEncoding.GetString(sequence);

  public static string GetString(ReadOnlySequence<byte> sequence)
    => DefaultEncoding.GetString(sequence);

  public static ReadOnlySpan<byte> CRLFSpan => "\r\n"u8;

  public const byte SP = 0x20;
}
