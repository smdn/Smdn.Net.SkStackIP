// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP;

internal sealed class SkStackDuplexPipe : IDuplexPipe {
  public PipeReader Input => receivePipe.Reader;
  public PipeWriter Output => sendPipe.Writer;

  private readonly Pipe receivePipe;
  private readonly Pipe sendPipe;

  private CancellationTokenSource? stopTokenSource;
  private Task? sendTask;

  private readonly MemoryStream sentDataBuffer = new();

  public SkStackDuplexPipe()
  {
    receivePipe = new Pipe(
      new PipeOptions(
        useSynchronizationContext: false
      )
    );
    sendPipe = new Pipe(
      new PipeOptions(
        useSynchronizationContext: false
      )
    );
  }

  public void Start()
  {
    if (stopTokenSource is not null)
      throw new InvalidOperationException("already started");

    stopTokenSource = new();

    sentDataBuffer.SetLength(0L);

    sendTask = SendAsync(stopTokenSource.Token);
  }

  public async ValueTask StopAsync()
  {
    if (stopTokenSource is null)
      throw new InvalidOperationException("not started yet");

    stopTokenSource.Cancel();

    try {
      await sendTask!.ConfigureAwait(false);
    }
    catch (OperationCanceledException ex) when (ex.CancellationToken == stopTokenSource.Token) {
      // expected cancellation exception
    }
  }

  public byte[] ReadSentData()
    => sentDataBuffer.ToArray();

  public async ValueTask WriteResponseLineAsync(string line)
  {
    if (stopTokenSource is null)
      throw new InvalidOperationException("not started yet");

    try {
      Encoding.ASCII.GetBytes(line, receivePipe.Writer);

      // CRLF
      var crlf = receivePipe.Writer.GetMemory(2);

      crlf.Span[0] = (byte)'\r';
      crlf.Span[1] = (byte)'\n';

      receivePipe.Writer.Advance(2);

      var flushResult = await receivePipe.Writer.FlushAsync(stopTokenSource.Token).ConfigureAwait(false);

      if (flushResult.IsCompleted || flushResult.IsCanceled)
        throw new InvalidOperationException("flush failed");
    }
    catch (Exception ex) {
      await receivePipe.Writer.CompleteAsync(ex).ConfigureAwait(false);
    }
  }

  private async Task SendAsync(CancellationToken stopToken)
  {
    try {
      while (!stopToken.IsCancellationRequested) {
        var read = await sendPipe.Reader.ReadAsync(stopToken).ConfigureAwait(false);
        var buffer = read.Buffer;

        if (buffer.IsEmpty && read.IsCompleted)
          break;

        foreach (var segment in buffer) {
          await sentDataBuffer.WriteAsync(segment, stopToken).ConfigureAwait(false);
        }

        await sentDataBuffer.FlushAsync(stopToken).ConfigureAwait(false);

        sendPipe.Reader.AdvanceTo(buffer.End);
      }
    }
    catch (Exception ex) {
      await sendPipe.Reader.CompleteAsync(ex);
      throw;
    }

    await sendPipe.Reader.CompleteAsync(null);
  }
}
