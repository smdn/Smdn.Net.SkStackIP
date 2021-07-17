// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Text;

namespace Smdn.Text {
  public static class EncodingExtensions {
#if !NET5_0_OR_GREATER
    public static string GetString(this Encoding encoding, ReadOnlySequence<byte> sequence)
    {
      var sb = new StringBuilder((int)Math.Min(int.MaxValue, sequence.Length));
      var decoder = encoding.GetDecoder();
      var pos = sequence.Start;
      char[] buffer = null;

      try {
        while (sequence.TryGet(ref pos, out var memory, advance: true)) {
          var doFlush = sequence.End.Equals(pos);

          var count = decoder.GetCharCount(memory.Span, doFlush);

          if (buffer is null) {
            buffer = ArrayPool<char>.Shared.Rent(count);
          }
          else if (buffer.Length < count) {
            ArrayPool<char>.Shared.Return(buffer);
            buffer = ArrayPool<char>.Shared.Rent(count);
          }

          var len = decoder.GetChars(memory.Span, buffer, doFlush);

          sb.Append(buffer, 0, len);
        }
      }
      finally {
        if (buffer is not null)
          ArrayPool<char>.Shared.Return(buffer);
      }

      return sb.ToString();
    }
#endif
  }
}
