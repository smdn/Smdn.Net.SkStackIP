// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;

namespace Smdn.Net.SkStackIP.Protocol;

internal sealed class SkStackCommandLineWriter : ISkStackCommandLineWriter {
  private readonly IBufferWriter<byte> writer;
  private readonly IBufferWriter<byte>? writerForLog;

  public SkStackCommandLineWriter(
    IBufferWriter<byte> writer,
    IBufferWriter<byte>? writerForWriter
  )
  {
    this.writer = writer;
    this.writerForLog = writerForWriter;
  }

  public void Write(ReadOnlySpan<byte> sequence)
  {
    if (sequence.IsEmpty)
      throw new ArgumentException("cannot be empty", paramName: nameof(sequence));

    writer.Write(sequence);

    writerForLog?.Write(sequence);
  }

  public void WriteToken(ReadOnlySpan<byte> token)
  {
    if (token.IsEmpty)
      throw new ArgumentException("cannot be empty", paramName: nameof(token));

    writer.GetSpan(1)[0] = SkStack.SP;
    writer.Advance(1);

    writer.Write(token);

    if (writerForLog is not null) {
      writerForLog.GetSpan(1)[0] = SkStack.SP;
      writerForLog.Advance(1);

      writerForLog.Write(token);
    }
  }

  public void WriteMaskedToken(ReadOnlySpan<byte> token)
  {
    if (token.IsEmpty)
      throw new ArgumentException("cannot be empty", paramName: nameof(token));

    writer.GetSpan(1)[0] = SkStack.SP;
    writer.Advance(1);

    writer.Write(token);

    if (writerForLog is not null) {
      writerForLog.GetSpan(1)[0] = SkStack.SP;
      writerForLog.Advance(1);

      const int MaskLength = 4;
      const byte MaskByte = (byte)'*';

      writerForLog.GetSpan(MaskLength).Slice(0, MaskLength).Fill(MaskByte);
      writerForLog.Advance(MaskLength);
    }
  }
}
