// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

namespace Smdn.Text.Unicode.ControlPictures {
  public static class ReadOnlySequenceExtensions {
    public static bool TryPicturizeControlChars(this ReadOnlySequence<byte> sequence, Span<char> destination)
    {
      if (sequence.IsEmpty)
        return true;

      var pos = sequence.Start;

      while (sequence.TryGet(ref pos, out var memory, advance: true)) {
        if (!ReadOnlySpanExtensions.TryPicturizeControlChars(memory.Span, destination))
          return false;

        destination = destination.Slice(memory.Length);
      }

      return true;
    }

    public static string ToControlCharsPicturizedString(this ReadOnlySequence<byte> sequence)
      => string.Create(
        (int)Math.Min(int.MaxValue, sequence.Length),
        sequence,
        static (span, seq) => seq.TryPicturizeControlChars(span)
      );
  }
}