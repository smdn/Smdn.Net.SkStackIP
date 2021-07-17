// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Buffers.Binary;

namespace Smdn.Net.SkStackIP {
  internal static class ReadOnlyMemoryExtensionss {
    private const byte SP = 0x20;
    private const int defaultExpectedMaxTokenCount = 8;

    public static T ConvertTokens<T>(
      this ReadOnlyMemory<byte> text,
      Func<ReadOnlyMemory<byte>,
      ReadOnlyMemory<ReadOnlyMemory<byte>>, T> convertTokens,
      int expectedMaxTokenCount = defaultExpectedMaxTokenCount
    )
    {
      ReadOnlyMemory<byte>[] tokens = null;
      var numberOfTokens = 0;

      try {
        tokens = ArrayPool<ReadOnlyMemory<byte>>.Shared.Rent(expectedMaxTokenCount);

        for (;;) {
          var sp = text.Span.IndexOf(SP);

          if (sp < 0) {
            tokens[numberOfTokens++] = text;
            break;
          }

          tokens[numberOfTokens++] = text.Slice(0, sp);
          text = text.Slice(sp + 1);

          if (numberOfTokens == expectedMaxTokenCount)
            break;
        }

        if (numberOfTokens == expectedMaxTokenCount)
          throw new InvalidOperationException("reached to expected max token count");

        return convertTokens(text, tokens.AsMemory(0, numberOfTokens));
      }
      finally {
        if (tokens is not null)
          ArrayPool<ReadOnlyMemory<byte>>.Shared.Return(tokens);
      }
    }

    private static byte FromHex(byte b)
    {
      if ((byte)'0' <= b && b <= (byte)'9')
        return (byte)(b - '0');
      if ((byte)'A' <= b && b <= (byte)'F')
        return (byte)(0xA + b - 'A');
      if ((byte)'a' <= b && b <= (byte)'a')
        return (byte)(0xA + b - 'a');

      throw SkStackUnexpectedResponseException.CreateInvalidToken(stackalloc byte[1] {b}, "HEX");
    }

    public static byte ToUINT8(this ReadOnlyMemory<byte> token)
    {
      var t = token.Span;

      if (t.Length != 2)
        throw SkStackUnexpectedResponseException.CreateInvalidToken(t, "UINT8");

      return (byte)(
        (FromHex(t[0]) << 4) |
         FromHex(t[1])
      );
    }

    public static ushort ToUINT16(this ReadOnlyMemory<byte> token)
    {
      var t = token.Span;

      if (t.Length != 4)
        throw SkStackUnexpectedResponseException.CreateInvalidToken(t, "UINT16");

      return (ushort)(
        (FromHex(t[0]) << 12) |
        (FromHex(t[1]) << 8) |
        (FromHex(t[2]) << 4) |
         FromHex(t[3])
      );
    }
  }
}