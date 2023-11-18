// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP;

internal class PseudoSkStackStream : Stream {
  public override bool CanWrite => true;
  public override bool CanRead => true;
  public override bool CanSeek => false;
  public override long Length => throw new NotSupportedException();
  public override long Position {
    get => throw new NotSupportedException();
    set => throw new NotSupportedException();
  }
  public bool IsClosed => writeStream is null;

  private readonly Pipe readStreamPipe;
  private readonly Stream readStreamReaderStream;
  private readonly Stream readStreamWriterStream;
  private readonly TextWriter readStreamWriter;
  private Stream writeStream;

  public PseudoSkStackStream()
  {
    readStreamPipe = new Pipe(
      new PipeOptions(
        useSynchronizationContext: false
      )
    );
    writeStream = new MemoryStream();
    readStreamReaderStream = readStreamPipe.Reader.AsStream();
    readStreamWriterStream = readStreamPipe.Writer.AsStream();
    readStreamWriter = new StreamWriter(readStreamWriterStream, Encoding.ASCII) {
      NewLine = "\r\n",
      AutoFlush = true,
    };
  }

  public Stream ResponseStream => readStreamWriterStream;
  public TextWriter ResponseWriter => readStreamWriter;

  public void ClearSentData()
  {
    writeStream.SetLength(0L);
  }

  public byte[] ReadSentData()
  {
    try {
      writeStream.Position = 0L;

      using var stream = new MemoryStream();

      writeStream.CopyTo(stream);

      return stream.ToArray();
    }
    finally {
      writeStream.Position = 0L;
    }
  }

  public override void Close()
  {
    writeStream?.Close();
    writeStream = null!;
  }

  public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
  public override void SetLength(long value) => throw new NotImplementedException();
  public override void Flush() => writeStream.Flush();

  public override void Write(byte[] buffer, int offset, int count)
    => writeStream.Write(buffer, offset, count);

  public override int Read(byte[] buffer, int offset, int count)
    => readStreamReaderStream.Read(buffer, offset, count);

  public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    => readStreamReaderStream.ReadAsync(buffer, offset, count, cancellationToken);

  public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    => readStreamReaderStream.ReadAsync(buffer, cancellationToken);
}
