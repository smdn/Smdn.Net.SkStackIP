// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

class PseudoSkStackDeviceStream : Stream {
  public override bool CanWrite => true;
  public override bool CanRead => true;
  public override bool CanSeek => false;
  public override long Length => throw new NotSupportedException();
  public override long Position {
    get => throw new NotSupportedException();
    set => throw new NotSupportedException();
  }

  private readonly Pipe readStreamPipe;
  private readonly Stream readStreamReaderStream;
  private readonly Stream readStreamWriterStream;

  public PseudoSkStackDeviceStream()
  {
    readStreamPipe = new Pipe(
      new PipeOptions(
        useSynchronizationContext: false
      )
    );
    readStreamReaderStream = readStreamPipe.Reader.AsStream();
    readStreamWriterStream = readStreamPipe.Writer.AsStream();
  }

  public Stream GetWriterStream() => readStreamWriterStream;

  public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
  public override void SetLength(long value) => throw new NotImplementedException();
  public override void Flush() => Stream.Null.Flush();

  public override void Write(byte[] buffer, int offset, int count)
    => Stream.Null.Write(buffer, offset, count);

  public override int Read(byte[] buffer, int offset, int count)
    => readStreamReaderStream.Read(buffer, offset, count);

  public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    => readStreamReaderStream.ReadAsync(buffer, offset, count, cancellationToken);

  public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    => readStreamReaderStream.ReadAsync(buffer, cancellationToken);
}
