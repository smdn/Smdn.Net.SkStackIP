// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Text;
using System.Text.Unicode;

using Microsoft.Extensions.Logging;

namespace Smdn.Net.SkStackIP {
  internal class DuplicateBufferWriter<T> : IBufferWriter<T> {
    private readonly IBufferWriter<T> targetWriter;
    private readonly int defaultSegmentSize;
    private Memory<T> currentMemory;
    private int currentMemorySize;
    private DuplicatedBufferSegment firstSegment;
    private DuplicatedBufferSegment currentSegment;

    public ReadOnlySequence<T> Sequence => new ReadOnlySequence<T>(
      firstSegment,
      0,
      currentSegment,
      currentMemorySize - currentMemory.Length
    );

    public DuplicateBufferWriter(IBufferWriter<T> targetWriter, int defaultSegmentSize = 0)
    {
      if (defaultSegmentSize < 0)
        throw new ArgumentOutOfRangeException(nameof(defaultSegmentSize), defaultSegmentSize, "must be zero or positive value");

      this.targetWriter = targetWriter;
      this.defaultSegmentSize = defaultSegmentSize == 0 ? 1024 : defaultSegmentSize;
    }

    private Memory<T> Allocate(int minimumSize)
    {
      var len = minimumSize == 0 ? defaultSegmentSize : Math.Max(minimumSize, defaultSegmentSize);

      currentMemorySize = len;
      currentMemory = new T[currentMemorySize]; // XXX: allocate
      currentSegment = new DuplicatedBufferSegment(currentSegment, currentMemory);

      if (firstSegment is null)
        firstSegment = currentSegment;

      return currentMemory;
    }

    public void Advance(int count)
    {
      if (currentMemory.Length < count)
        throw new InvalidOperationException("requested size has not been written");

      currentMemory.Slice(0, count).CopyTo(targetWriter.GetMemory(count));
      targetWriter.Advance(count);

      currentMemory = currentMemory.Slice(count);
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
      if (sizeHint < 0)
        throw new ArgumentOutOfRangeException(nameof(sizeHint), sizeHint, "must be zero or positive number");

      if (sizeHint < currentMemory.Length)
        return currentMemory;

      return Allocate(sizeHint);
    }

    public Span<T> GetSpan(int sizeHint = 0)
      => GetMemory(sizeHint).Span;

    private class DuplicatedBufferSegment : ReadOnlySequenceSegment<T> {
      public DuplicatedBufferSegment(DuplicatedBufferSegment prev, ReadOnlyMemory<T> memory)
      {
        Memory = memory;

        if (prev == null) {
          RunningIndex = 0;
        }
        else {
          RunningIndex = prev.RunningIndex + prev.Memory.Length;
          prev.Next = this;
        }
      }
    }
  }

  internal static class SkStackClientLoggerExtensions {
    private static readonly EventId eventIdCommandSequence = new EventId(1, "sent command sequence");
    private static readonly EventId eventIdResponseSequence = new EventId(2, "received response sequence");

    private const string prefixCommand = "↦ ";
    private const string prefixResponse = "↤ ";
    private const string prefixEchoBack = "↩ ";

    public static bool IsCommandEnabled(this ILogger logger)
    {
      const LogLevel level = LogLevel.Trace;

      return logger.IsEnabled(level);
    }

    public static void LogTraceCommand(this ILogger logger, ReadOnlySequence<byte> sequence)
    {
      const LogLevel level = LogLevel.Trace;

      if (!logger.IsEnabled(level))
        return;

      logger.Log(
        level,
        eventIdCommandSequence,
        CreateLogMessage(prefixCommand, sequence)
      );
    }

    public static void LogTraceResponse(this ILogger logger, ReadOnlySequence<byte> sequence, bool isEchoBack = false)
    {
      const LogLevel level = LogLevel.Trace;

      if (!logger.IsEnabled(level))
        return;

      logger.Log(
        level,
        eventIdResponseSequence,
        CreateLogMessage(isEchoBack ? prefixEchoBack : prefixResponse, sequence)
      );
    }

    public static void LogDebugResponse(this ILogger logger, ReadOnlySequence<byte> sequence, Exception exception = null)
    {
      const LogLevel level = LogLevel.Debug;

      if (!logger.IsEnabled(level))
        return;

      logger.Log(
        level,
        eventIdResponseSequence,
        exception,
        CreateLogMessage(prefixResponse, sequence)
      );
    }

    internal static string CreateLogMessage(string prefix, ReadOnlySequence<byte> sequence)
    {
      var length = (int)Math.Min(int.MaxValue, prefix.Length + sequence.Length);

      return string.Create(length, (pfx: prefix, seq: sequence), (span, arg) => {
        var index = 0;

        // copy prefix
        for (var i = 0; i < arg.pfx.Length; i++) {
          span[index++] = arg.pfx[i];
        }

        // copy sequence
        byte[] buffer = null;

        try {
          var len = (int)Math.Min(span.Length - index, arg.seq.Length);
          buffer = ArrayPool<byte>.Shared.Rent(len);

          arg.seq.CopyTo(buffer.AsSpan());

          for (var i = 0; i < len; i++) {
            // replace control characters to control pictures
            if (0x00 <= buffer[i] && buffer[i] <= 0x20)
              span[index++] = (char)(UnicodeRanges.ControlPictures.FirstCodePoint + buffer[i]); // U+2400-U+2420
            else if (buffer[i] == 0x7F)
              span[index++] = (char)(UnicodeRanges.ControlPictures.FirstCodePoint + 0x21); // U+2421
            else
              span[index++] = (char)buffer[i];
          }
        }
        finally {
          ArrayPool<byte>.Shared.Return(buffer);
        }
      });
    }

    private static string ToControlPicturedString(this ReadOnlySequence<byte> sequence)
    {
      var length = (int)Math.Min(int.MaxValue, sequence.Length);

      return string.Create(length, (len: length, seq: sequence), (span, arg) => {
        byte[] buffer = null;

        try {
          buffer = ArrayPool<byte>.Shared.Rent(arg.len);

          arg.seq.CopyTo(buffer.AsSpan());

          for (var i = 0; i < arg.len; i++) {
            if (0x00 <= buffer[i] && buffer[i] <= 0x20)
              span[i] = (char)(UnicodeRanges.ControlPictures.FirstCodePoint + buffer[i]); // U+2400-U+2420
            else if (buffer[i] == 0x7F)
              span[i] = (char)(UnicodeRanges.ControlPictures.FirstCodePoint + 0x21); // U+2421
            else
              span[i] = (char)buffer[i];
          }
        }
        finally {
          ArrayPool<byte>.Shared.Return(buffer);
        }
      });
    }
  }
}