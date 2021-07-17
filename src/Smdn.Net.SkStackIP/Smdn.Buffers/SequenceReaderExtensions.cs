// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

namespace Smdn.Buffers {
  public static class SequenceReaderExtensions {
    public static ReadOnlySequence<T> GetUnreadSequence<T>(this SequenceReader<T> sequenceReader) where T : unmanaged, IEquatable<T>
#if NET5_0_OR_GREATER
      => sequenceReader.UnreadSequence;
#else
      => sequenceReader.Sequence.Slice(sequenceReader.Position);
#endif
  }
}