// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Smdn.Net.SkStackIP.Protocol;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientTests : SkStackClientTestsBase {
  private static readonly TimeSpan responseDelayInterval = TimeSpan.FromMilliseconds(50);

  private class SkStackClientEx : SkStackClient {
    public new ILogger? Logger => base.Logger;

    public SkStackClientEx(Stream stream, ILogger logger)
      : base(stream, logger: logger)
    {
    }

    public SkStackClientEx(PipeWriter sender, PipeReader receiver, ILogger logger)
      : base(sender: sender, receiver: receiver, logger: logger)
    {
    }

    public ValueTask<SkStackResponse> SendCommandAsync(
      string command,
      IEnumerable<string>? arguments = null,
      SkStackProtocolSyntax? syntax = null,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
      => base.SendCommandAsync(
        command: command.ToByteSequence(),
        writeArguments: writer => WriteArguments(writer, arguments),
        syntax: syntax,
        throwIfErrorStatus: throwIfErrorStatus,
        cancellationToken: cancellationToken
      );

    public ValueTask<SkStackResponse<TPayload>> SendCommandAsync<TPayload>(
      string command,
      SkStackSequenceParser<TPayload?> parseResponsePayload,
      IEnumerable<string>? arguments = null,
      SkStackProtocolSyntax? syntax = null,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
      => base.SendCommandAsync(
        command: command.ToByteSequence(),
        writeArguments: writer => WriteArguments(writer, arguments),
        parseResponsePayload: parseResponsePayload,
        syntax: syntax,
        throwIfErrorStatus: throwIfErrorStatus,
        cancellationToken: cancellationToken
      );

    private static void WriteArguments(ISkStackCommandLineWriter writer, IEnumerable<string>? arguments)
    {
      if (arguments is null)
        return;

      foreach (var arg in arguments) {
        writer.WriteToken(arg.ToByteSequence().Span);
      }
    }
  }

  private SkStackClientEx CreateClient(Stream stream)
    => new(stream, CreateLoggerForTestCase());

  [Test]
  public void Ctor_WithStream_StreamNull()
    => Assert.That(() => new SkStackClient(stream: null!), Throws.ArgumentNullException);

  [Test]
  public void Ctor_WithStream_StreamNotWritable()
    => Assert.That(() => new SkStackClient(stream: new MemoryStream(Array.Empty<byte>(), writable: false)), Throws.ArgumentException);

  private class UnreadableMemoryStream : MemoryStream {
    public override bool CanRead => false;

    public UnreadableMemoryStream()
      : base()
    {
    }
  }

  [Test]
  public void Ctor_WithStream_StreamNotReadable()
    => Assert.That(() => new SkStackClient(stream: new UnreadableMemoryStream()), Throws.ArgumentException);

  [TestCase(true)]
  [TestCase(false)]
  public void Ctor_WithStream_LeaveStreamOpen(bool leaveStreamOpen)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream: stream, leaveStreamOpen: leaveStreamOpen);

    client.Dispose();

    Assert.That(stream.IsClosed, Is.EqualTo(!leaveStreamOpen), nameof(stream.IsClosed));
  }

  [Test]
  public void Ctor_WithPipeReaderWriter_SenderNull()
  {
    var pipe = new Pipe();

    Assert.That(() => new SkStackClient(sender: null!, receiver: pipe.Reader), Throws.ArgumentNullException);
  }

  [Test]
  public void Ctor_WithPipeReaderWriter_ReceiverNull()
  {
    var pipe = new Pipe();

    Assert.That(() => new SkStackClient(sender: pipe.Writer, receiver: null!), Throws.ArgumentNullException);
  }

  [TestCase(SkStackERXUDPDataFormat.Binary)]
  [TestCase(SkStackERXUDPDataFormat.HexAsciiText)]
  public void Ctor_ERXUDPDataFormat(SkStackERXUDPDataFormat format)
  {
    Assert.DoesNotThrow(() => {
      using var client = new SkStackClient(Stream.Null, erxudpDataFormat: format);

      Assert.That(client.ERXUDPDataFormat, Is.EqualTo(format), nameof(client.ERXUDPDataFormat));
    });
  }

  [TestCase(-1)]
  public void Ctor_ERXUDPDataFormat_InvalidValue(SkStackERXUDPDataFormat format)
  {
    Assert.That(
      () => {
        using var client = new SkStackClient(Stream.Null, erxudpDataFormat: format);
      },
      Throws.ArgumentException
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Logger()
  {
    yield return new object?[] { Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance };
    yield return new object?[] { null };
  }

  [TestCaseSource(nameof(YieldTestCases_Logger))]
  public void Logger(ILogger logger)
  {
    using var client = new SkStackClientEx(Stream.Null, logger);

    if (logger is null)
      Assert.That(client.Logger, Is.Null, nameof(client.Logger));
    else
      Assert.That(client.Logger, Is.SameAs(logger), nameof(client.Logger));
  }

  [Test]
  public void Dispose()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    var client = CreateClient(stream);

    Assert.That(
      async () => await client.SendCommandAsync("TEST"),
      Throws.Nothing,
      $"{nameof(client.SendCommandAsync)} before {nameof(client.Dispose)}"
    );

    Assert.DoesNotThrow(client.Dispose, $"{nameof(client.Dispose)} #1");

    Assert.That(
      async () => await client.SendCommandAsync("TEST"),
      Throws.TypeOf<ObjectDisposedException>(),
      $"{nameof(client.SendCommandAsync)} after {nameof(client.Dispose)}"
    );

    Assert.DoesNotThrow(client.Dispose, $"{nameof(client.Dispose)} #2");
  }

  [Test]
  public void Command_ClientConstructedFromStream()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClientEx(stream, CreateLoggerForTestCase());

    Assert.DoesNotThrowAsync(async () => {
      _ = await client.SendCommandAsync(
        command: "TEST",
        arguments: new[] { "arg1", "arg2" }
      );
    });

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST arg1 arg2\r\n".ToByteSequence())
    );
  }

  [Test]
  public async Task Command_ClientConstructedFromPipeReaderWriter()
  {
    var pipe = new SkStackDuplexPipe();

    pipe.Start();

    await pipe.WriteResponseLineAsync("OK").ConfigureAwait(false);

    using var client = new SkStackClientEx(pipe.Output, pipe.Input, CreateLoggerForTestCase());

    Assert.DoesNotThrowAsync(async () => {
      _ = await client.SendCommandAsync(
        command: "TEST",
        arguments: new[] { "arg1", "arg2" }
      );
    });

    await pipe.StopAsync().ConfigureAwait(false);

    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { // disable the unstable test case
      Assert.That(
        pipe.ReadSentData,
        Throws.Nothing
      );
    }
    else {
      Assert.That(
        pipe.ReadSentData(),
        SequenceIs.EqualTo("TEST arg1 arg2\r\n".ToByteSequence())
      );
    }
  }

  [Test]
  public void Command_CommandEmpty()
  {
    var stream = new PseudoSkStackStream();

    using var client = CreateClient(stream);

    Assert.That(async () => await client.SendCommandAsync(string.Empty), Throws.ArgumentException);

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [Test]
  public void Command_ArgumentsEmpty()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);

    SkStackResponse? resp = default;

    Assert.DoesNotThrowAsync(async () => {
      resp = await client.SendCommandAsync(
        command: "TEST",
        arguments: Array.Empty<string>()
      );
    });

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp!.Success, Is.True);
  }

  [Test]
  public void Command_ArgumentsEmpty_ContainsEmpty()
  {
    var stream = new PseudoSkStackStream();

    using var client = CreateClient(stream);

    Assert.ThrowsAsync<ArgumentException>(async () => {
      await client.SendCommandAsync(
        command: "TEST",
        arguments: new[] { string.Empty }
      );
    });

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [Test]
  public async Task Command_CommandWithoutArguments()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  private class EndOfCommandLineSyntax : SkStackProtocolSyntax {
    private readonly ReadOnlyMemory<byte> endOfCommandLine;
    public override ReadOnlySpan<byte> EndOfCommandLine => endOfCommandLine.Span;

    public override bool ExpectStatusLine => true;

    private static readonly ReadOnlyMemory<byte> endOfStatusLine = "\r\n".ToByteSequence();
    public override ReadOnlySpan<byte> EndOfStatusLine => endOfStatusLine.Span;

    public EndOfCommandLineSyntax(string endOfCommandLine)
    {
      this.endOfCommandLine = endOfCommandLine.ToByteSequence();
    }
  }

  [TestCase("\r\n")] // SK commands
  [TestCase("")] // SKSENDTO
  [TestCase("\r")] // ROHM product setting commands
  public async Task Command_EndOfCommandLine(string commandLineTerminator)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    var syntax = new EndOfCommandLineSyntax(commandLineTerminator);

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      syntax: syntax
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"TEST{commandLineTerminator}".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [Test]
  public async Task Response_CommandWithArguments()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync(
      "TEST",
      new[] {
        "ARG1",
        "ARG2",
        "ARG3"
      }
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST ARG1 ARG2 ARG3\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [Test]
  public async Task Response_IgnoreEchoback()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [Test]
  public async Task Response_IgnoreEchoback_WithArguments()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST ARG1 ARG2 ARG3");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync(
      "TEST",
      new[] {
        "ARG1",
        "ARG2",
        "ARG3"
      }
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST ARG1 ARG2 ARG3\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [Test]
  public async Task Response_IgnoreEchoback_PayloadMustNotBeTreatedAsEchoback()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST2");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "TEST2".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return "payload";
        }

        context.SetAsIncomplete();
        return default;
      }
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
    Assert.That(resp.Payload, Is.EqualTo("payload"));
  }

  [Test]
  [Category("limitation")]
  public async Task Response_AmbiguousWhetherEchobackOrPayload()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST"); // will be treated as echoback
    stream.ResponseWriter.WriteLine("OK"); // will be treated as payload
    stream.ResponseWriter.WriteLine("TEST echoback of next command"); // will be treated as status line

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "OK".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return "OK";
        }

        context.SetAsIncomplete();
        return default;
      }
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.False);
    Assert.That(resp.Status, Is.EqualTo(SkStackResponseStatus.Undetermined));
    Assert.That(resp.Payload, Is.EqualTo("OK"));
  }

  [Test]
  public void Response_ParseSequenceThrownUnexpectedResponseException()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("UNEXPECTEDTOKEN");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);

    Assert.That(
      async () => {
        await client.SendCommandAsync(
          command: "TEST",
          arguments: null,
          parseResponsePayload: static context => {
            var reader = context.CreateReader();

            if (
              SkStackTokenParser.ExpectToken(ref reader, "EXPECTEDTOKEN".ToByteSequence().Span) &&
              SkStackTokenParser.ExpectEndOfLine(ref reader)
            ) {
              context.Complete(reader);
              return "payload";
            }

            context.SetAsIncomplete();
            return default;
          }
        );
      },
      Throws
        .TypeOf<SkStackUnexpectedResponseException>()
        .With
        .Property(nameof(SkStackUnexpectedResponseException.CausedText))
        .EqualTo("UNEXPECTEDTOKEN")
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );
  }

  private class EndOfStatusLineSyntax : SkStackProtocolSyntax {
    private readonly ReadOnlyMemory<byte> endOfCommandLine;
    public override ReadOnlySpan<byte> EndOfCommandLine => endOfCommandLine.Span;

    public override bool ExpectStatusLine => true;

    private readonly ReadOnlyMemory<byte> endOfStatusLine;
    public override ReadOnlySpan<byte> EndOfStatusLine => endOfStatusLine.Span;

    public EndOfStatusLineSyntax(string lineTerminator)
    {
      endOfCommandLine = lineTerminator.ToByteSequence();
      endOfStatusLine = lineTerminator.ToByteSequence();
    }
  }

  [TestCase("\r\n")] // SK commands
  [TestCase("\r")] // ROHM product setting commands
  public async Task Response_EndOfStatusLine(string lineTerminator)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.Write("OK");
    stream.ResponseWriter.Write(lineTerminator);

    var syntax = new EndOfStatusLineSyntax(lineTerminator);

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      syntax: syntax
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(("TEST" + lineTerminator).ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
    Assert.That(resp.Status, Is.EqualTo(SkStackResponseStatus.Ok));
    Assert.That(resp.StatusText, SequenceIs.EqualTo(ReadOnlyMemory<byte>.Empty));
  }

  [Test]
  public async Task Response_StatusOk()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
    Assert.That(resp.Status, Is.EqualTo(SkStackResponseStatus.Ok));
    Assert.That(resp.StatusText, SequenceIs.EqualTo(ReadOnlyMemory<byte>.Empty));
  }

  [Test]
  public async Task Response_StatusOkWithStatusText()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK DONE");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
    Assert.That(resp.Status, Is.EqualTo(SkStackResponseStatus.Ok));
    Assert.That(resp.StatusText, SequenceIs.EqualTo("DONE".ToByteSequence()));
  }

  [Test]
  public async Task Response_StatusFail()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.False);
    Assert.That(resp.Status, Is.EqualTo(SkStackResponseStatus.Fail));
    Assert.That(resp.StatusText, SequenceIs.EqualTo(ReadOnlyMemory<byte>.Empty));
  }

  [Test]
  public async Task Response_StatusFailWithStatusText()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL REASON");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.False);
    Assert.That(resp.Status, Is.EqualTo(SkStackResponseStatus.Fail));
    Assert.That(resp.StatusText, SequenceIs.EqualTo("REASON".ToByteSequence()));
  }

  [Test]
  public void Response_StatusFailWithStatusText_AsException_ErrorCode_ER01()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackErrorResponseException>(nameof(SkStackErrorCode.ER01), SkStackErrorCode.ER01);

  [Test]
  public void Response_StatusFailWithStatusText_AsException_ErrorCode_ER04()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackCommandNotSupportedException>(nameof(SkStackErrorCode.ER04), SkStackErrorCode.ER04);

  [Test]
  public void Response_StatusFailWithStatusText_AsException_ErrorCode_ER09()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackUartIOException>(nameof(SkStackErrorCode.ER09), SkStackErrorCode.ER09);

  [Test]
  public void Response_StatusFailWithStatusText_AsException_ErrorCode_Undefined()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackErrorResponseException>("ERXX", SkStackErrorCode.Undefined);

  private void Response_StatusFailWithStatusText_AsException_ErrorCode<TExpectedException>(
    string errorCodeString,
    SkStackErrorCode expectedErrorCode
  )
    where TExpectedException : SkStackErrorResponseException
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"FAIL {errorCodeString}");

    using var client = CreateClient(stream);

    Assert.That(
      async () => await client.SendCommandAsync("TEST", throwIfErrorStatus: true),
      Throws
        .TypeOf<TExpectedException>()
        .And.Property(nameof(SkStackErrorResponseException.ErrorCode)).EqualTo(expectedErrorCode)
        .And.Property(nameof(SkStackErrorResponseException.ErrorText)).Empty
        .And.Property(nameof(SkStackErrorResponseException.Response)).Not.Null
        .And
          .Property(nameof(SkStackErrorResponseException.Response))
            .Property(nameof(SkStackResponse.Success)).False
        .And
          .Property(nameof(SkStackErrorResponseException.Response))
            .Property(nameof(SkStackResponse.Status)).EqualTo(SkStackResponseStatus.Fail)
        .And
          .Property(nameof(SkStackErrorResponseException.Response))
            .Property(nameof(SkStackResponse.StatusText))
              .Append(new Smdn.Test.NUnit.Constraints.Buffers.ReadOnlyByteMemoryEqualConstraint(errorCodeString.ToByteSequence()))
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );
  }

  [Test]
  public void Response_StatusFailWithStatusText_AsException_ErrorCodeWithText()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER01 error text");

    using var client = CreateClient(stream);

    Assert.That(
      async () => await client.SendCommandAsync("TEST", throwIfErrorStatus: true),
      Throws
        .TypeOf<SkStackErrorResponseException>()
        .And.Property(nameof(SkStackErrorResponseException.ErrorCode)).EqualTo(SkStackErrorCode.ER01)
        .And.Property(nameof(SkStackErrorResponseException.ErrorText)).EqualTo("error text")
        .And.Property(nameof(SkStackErrorResponseException.Response)).Not.Null
        .And
          .Property(nameof(SkStackErrorResponseException.Response))
            .Property(nameof(SkStackResponse.Success)).False
        .And
          .Property(nameof(SkStackErrorResponseException.Response))
            .Property(nameof(SkStackResponse.Status)).EqualTo(SkStackResponseStatus.Fail)
        .And
          .Property(nameof(SkStackErrorResponseException.Response))
            .Property(nameof(SkStackResponse.StatusText))
              .Append(new Smdn.Test.NUnit.Constraints.Buffers.ReadOnlyByteMemoryEqualConstraint("ER01 error text".ToByteSequence()))
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );
  }

  [Test]
  public async Task Response_StatusUnexpected()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("ERROR undefined status");

    using var client = CreateClient(stream);

    var resp = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.That(resp.Success, Is.False);
    Assert.That(resp.Status, Is.EqualTo(SkStackResponseStatus.Undetermined));
    Assert.That(resp.StatusText, Is.EqualTo(ReadOnlyMemory<byte>.Empty));

    // must be able to continue processing next response
    stream.ResponseWriter.WriteLine("OK done.");

    var resp2 = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.That(resp2.Success, Is.True);
    Assert.That(resp2.Status, Is.EqualTo(SkStackResponseStatus.Ok));
    Assert.That(resp2.StatusText, SequenceIs.EqualTo("done.".ToByteSequence()));
  }

  [Test]
  public async Task Response_Payload()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("LINE1");
    stream.ResponseWriter.WriteLine("LINE2");
    stream.ResponseWriter.WriteLine("LINE3");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "LINE1".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader , "LINE2".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader, "LINE3".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return "3-line payload";
        }

        context.Ignore();
        return default;
      }
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
    Assert.That(resp.Payload, Is.Not.Null);
    Assert.That(resp.Payload, Is.EqualTo("3-line payload"));
  }

  [Test]
  public async Task Response_IncompleteLine_Status()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.Write("FA"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("IL"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.WriteLine();
    }

    var client = CreateClient(stream);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync("TEST", throwIfErrorStatus: false).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.False);
  }

  [Test]
  public async Task Response_IncompleteLine_NewLine()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.Write("OK\r"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("\n");
    }

    var client = CreateClient(stream);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync("TEST", throwIfErrorStatus: false).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [Test]
  public async Task Response_IncompleteLine_Echoback()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.Write("TE"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("ST"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(responseDelayInterval);

      stream.ResponseWriter.WriteLine("OK");
    }

    var client = CreateClient(stream);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync("TEST", throwIfErrorStatus: false).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [Test]
  public async Task Response_IncompleteLine_Payload()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.WriteLine("LINE1");
      await Task.Delay(responseDelayInterval);

      stream.ResponseWriter.Write("LI"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.WriteLine("NE2");
      await Task.Delay(responseDelayInterval);

      stream.ResponseWriter.Write("LI"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.WriteLine("NE3");
      await Task.Delay(responseDelayInterval);

      stream.ResponseWriter.WriteLine("OK");
    }

    var client = CreateClient(stream);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "LINE1".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader, "LINE2".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader, "LINE3".ToByteSequence().Span) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return "3-line payload";
        }

        context.SetAsIncomplete();
        return default;
      },
      throwIfErrorStatus: false
    ).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
    Assert.That(resp.Payload, Is.Not.Null);
    Assert.That(resp.Payload, Is.EqualTo("3-line payload"));
  }

  [Test]
  public async Task Response_IncompleteLine_NNotifyingEvents_EVENT()
  {
    var stream = new PseudoSkStackStream();

    // EVENT 02 FE80:0000:0000:0000:021D:1290:1234:5678
    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.Write("E"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("VENT "); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("02"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write(" FE80:0000:0000:0000:02"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("1D:1290:1234:5678"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(responseDelayInterval);

      stream.ResponseWriter.WriteLine("OK");
    }

    var client = CreateClient(stream);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      throwIfErrorStatus: false
    ).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [Test]
  public async Task Response_IncompleteLine_NotifyingEvents_ERXUDP()
  {
    var stream = new PseudoSkStackStream();

    // ERXUDP FE80:0000:0000:0000:021D:1290:1234:5678 FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0
    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.Write("ERXUDP"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write(" FE80:0000:0000:0000:021D:1290:1234:5678 FE80:0000:0000:0000:021D:1290:1234:5678 "); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("02CC 02CC 001D129012345678"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write(" 0 00"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.Write("01 0"); await Task.Delay(responseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(responseDelayInterval);

      stream.ResponseWriter.WriteLine("OK");
    }

    var client = CreateClient(stream);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      throwIfErrorStatus: false
    ).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.True);
  }

  [TestCase(1)]
  [TestCase(2)]
  public async Task Response_ReceiveResponseDelay(int delayInSeconds)
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.Write("FAIL"); await Task.Delay(100);
      stream.ResponseWriter.WriteLine();
    }

    var client = CreateClient(stream);
    var delay = TimeSpan.FromSeconds(delayInSeconds);

    client.ReceiveResponseDelay = delay;

    var sw = Stopwatch.StartNew();
#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync("TEST", throwIfErrorStatus: false).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    sw.Stop();

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.That(resp.Success, Is.False);

    if (sw.Elapsed < delay)
      Assert.Warn($"elapsed time does not exceed specified delay time (delay: {delay}, elapsed: {sw.Elapsed})");
  }
}
