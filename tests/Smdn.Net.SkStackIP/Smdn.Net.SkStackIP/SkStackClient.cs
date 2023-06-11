// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Smdn.Net.SkStackIP.Protocol;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientTests {
  private static readonly TimeSpan responseDelayInterval = TimeSpan.FromMilliseconds(50);

  private IServiceProvider serviceProvider;

  [SetUp]
  public void SetUp()
  {
    var services = new ServiceCollection();

    services.AddLogging(
      builder => builder
        .AddSimpleConsole(static options => options.SingleLine = true)
        .AddFilter(static level => true/*level <= LogLevel.Trace*/)
    );

    serviceProvider = services.BuildServiceProvider();
  }

  private class SkStackClientEx : SkStackClient {
    public SkStackClientEx(Stream stream, IServiceProvider serviceProvider)
      : base(stream, serviceProvider)
    {
    }

    public ValueTask<SkStackResponse> SendCommandAsync(
      string command,
      IEnumerable<string> arguments = null,
      SkStackProtocolSyntax syntax = null,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
      => base.SendCommandAsync(
        command: command.ToByteSequence(),
        arguments: arguments?.Select(arg => arg.ToByteSequence()),
        syntax: syntax,
        throwIfErrorStatus: throwIfErrorStatus,
        cancellationToken: cancellationToken
      );

    public ValueTask<SkStackResponse<TPayload>> SendCommandAsync<TPayload>(
      string command,
      IEnumerable<string> arguments = null,
      SkStackSequenceParser<TPayload> parseResponsePayload = null,
      SkStackProtocolSyntax syntax = null,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
      => base.SendCommandAsync(
        command: command.ToByteSequence(),
        arguments: arguments?.Select(arg => arg.ToByteSequence()),
        parseResponsePayload: parseResponsePayload,
        syntax: syntax,
        throwIfErrorStatus: throwIfErrorStatus,
        cancellationToken: cancellationToken
      );
  }

  private static SkStackClientEx CreateClient(Stream stream, IServiceProvider serviceProvider)
    => new(stream, serviceProvider);

  [Test]
  public void Create_FromSerialPortName_PortNameNull()
    => Assert.Throws<ArgumentNullException>(() => new SkStackClient(serialPortName: null!));

  [Test]
  public void Create_FromSerialPortName_PortNameEmpty()
    => Assert.Throws<ArgumentException>(() => new SkStackClient(serialPortName: string.Empty));

  [Test]
  public void Create_FromStream_StreamNull()
    => Assert.Throws<ArgumentNullException>(() => new SkStackClient(stream: null!));

  [Test]
  public void Create_FromStream_StreamNotWritable()
    => Assert.Throws<ArgumentException>(() => new SkStackClient(stream: new System.IO.MemoryStream(Array.Empty<byte>(), writable: false)));

  private class UnreadableMemoryStream : System.IO.MemoryStream {
    public override bool CanRead => false;

    public UnreadableMemoryStream()
      : base()
    {
    }
  }

  [Test]
  public void Create_FromStream_StreamNotReadable()
    => Assert.Throws<ArgumentException>(() => new SkStackClient(stream: new UnreadableMemoryStream()));

  [Test]
  public void Dispose()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    var client = CreateClient(stream, serviceProvider: serviceProvider);

    Assert.DoesNotThrowAsync(async () => await client.SendCommandAsync("TEST"), $"{nameof(client.SendCommandAsync)} before {nameof(client.Dispose)}");

    Assert.DoesNotThrow(client.Dispose, $"{nameof(client.Dispose)} #1");

    Assert.ThrowsAsync<ObjectDisposedException>(async () => await client.SendCommandAsync("TEST"), $"{nameof(client.SendCommandAsync)} after {nameof(client.Dispose)}");

    Assert.DoesNotThrow(client.Dispose, $"{nameof(client.Dispose)} #2");
  }

  [Test]
  public void Command_CommandEmpty()
  {
    var stream = new PseudoSkStackStream();

    using var client = CreateClient(stream, serviceProvider: serviceProvider);

    Assert.ThrowsAsync<ArgumentException>(async () => await client.SendCommandAsync(string.Empty));

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void Command_ArgumentsEmpty()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);

    SkStackResponse resp = default;

    Assert.DoesNotThrowAsync(async () => {
      resp = await client.SendCommandAsync(
        command: "TEST",
        arguments: Array.Empty<string>()
      );
    });

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp!.Success);
  }

  [Test]
  public void Command_ArgumentsEmpty_ContainsEmpty()
  {
    var stream = new PseudoSkStackStream();

    using var client = CreateClient(stream, serviceProvider: serviceProvider);

    Assert.ThrowsAsync<ArgumentException>(async () => {
      await client.SendCommandAsync(
        command: "TEST",
        arguments: new[] { string.Empty }
      );
    });

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public async Task Command_CommandWithoutArguments()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
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

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      syntax: syntax
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo($"TEST{commandLineTerminator}".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }

  [Test]
  public async Task Response_CommandWithArguments()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
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
      Is.EqualTo("TEST ARG1 ARG2 ARG3\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }

  [Test]
  public async Task Response_IgnoreEchoback()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }

  [Test]
  public async Task Response_IgnoreEchoback_WithArguments()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST ARG1 ARG2 ARG3");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
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
      Is.EqualTo("TEST ARG1 ARG2 ARG3\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }

  [Test]
  public async Task Response_IgnoreEchoback_PayloadMustNotBeTreatedAsEchoback()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST2");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "TEST2".ToByteSequence()) &&
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
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
    Assert.AreEqual("payload", resp.Payload);
  }

  [Test]
  [Category("limitation")]
  public async Task Response_AmbiguousWhetherEchobackOrPayload()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("TEST"); // will be treated as echoback
    stream.ResponseWriter.WriteLine("OK"); // will be treated as payload
    stream.ResponseWriter.WriteLine("TEST echoback of next command"); // will be treated as status line

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "OK".ToByteSequence()) &&
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
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsFalse(resp.Success);
    Assert.AreEqual(SkStackResponseStatus.Undetermined, resp.Status);
    Assert.AreEqual("OK", resp.Payload);
  }

  [Test]
  public void Response_ParseSequenceThrownUnexpectedResponseException()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("UNEXPECTEDTOKEN");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);

    var ex = Assert.ThrowsAsync<SkStackUnexpectedResponseException>(async () => {
      await client.SendCommandAsync(
        command: "TEST",
        arguments: null,
        parseResponsePayload: static context => {
          var reader = context.CreateReader();

          if (
            SkStackTokenParser.ExpectToken(ref reader, "EXPECTEDTOKEN".ToByteSequence()) &&
            SkStackTokenParser.ExpectEndOfLine(ref reader)
          ) {
            context.Complete(reader);
            return "payload";
          }

          context.SetAsIncomplete();
          return default;
        }
      );
    });

    Assert.AreEqual("UNEXPECTEDTOKEN", ex!.CausedText, nameof(ex.CausedText));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
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

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      syntax: syntax
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo(("TEST" + lineTerminator).ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
    Assert.AreEqual(resp.Status, SkStackResponseStatus.Ok);
    Assert.That(resp.StatusText, Is.EqualTo(ReadOnlyMemory<byte>.Empty));
  }

  [Test]
  public async Task Response_StatusOk()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
    Assert.AreEqual(resp.Status, SkStackResponseStatus.Ok);
    Assert.That(resp.StatusText, Is.EqualTo(ReadOnlyMemory<byte>.Empty));
  }

  [Test]
  public async Task Response_StatusOkWithStatusText()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK DONE");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync("TEST");

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
    Assert.AreEqual(resp.Status, SkStackResponseStatus.Ok);
    Assert.That(resp.StatusText, Is.EqualTo("DONE".ToByteSequence()));
  }

  [Test]
  public async Task Response_StatusFail()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsFalse(resp.Success);
    Assert.AreEqual(resp.Status, SkStackResponseStatus.Fail);
    Assert.That(resp.StatusText, Is.EqualTo(ReadOnlyMemory<byte>.Empty));
  }

  [Test]
  public async Task Response_StatusFailWithStatusText()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL REASON");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsFalse(resp.Success);
    Assert.AreEqual(resp.Status, SkStackResponseStatus.Fail);
    Assert.That(resp.StatusText, Is.EqualTo("REASON".ToByteSequence()));
  }

  [Test] public void Response_StatusFailWithStatusText_AsException_ErrorCode_ER01()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackErrorResponseException>(nameof(SkStackErrorCode.ER01), SkStackErrorCode.ER01);

  [Test] public void Response_StatusFailWithStatusText_AsException_ErrorCode_ER04()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackCommandNotSupportedException>(nameof(SkStackErrorCode.ER04), SkStackErrorCode.ER04);

  [Test] public void Response_StatusFailWithStatusText_AsException_ErrorCode_ER09()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackUartIOException>(nameof(SkStackErrorCode.ER09), SkStackErrorCode.ER09);

  [Test] public void Response_StatusFailWithStatusText_AsException_ErrorCode_Undefined()
    => Response_StatusFailWithStatusText_AsException_ErrorCode<SkStackErrorResponseException>("ERXX", SkStackErrorCode.Undefined);

  private void Response_StatusFailWithStatusText_AsException_ErrorCode<TExpectedException>(
    string errorCodeString,
    SkStackErrorCode expectedErrorCode
  )
    where TExpectedException : SkStackErrorResponseException
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"FAIL {errorCodeString}");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var ex = Assert.ThrowsAsync<TExpectedException>(
      async () => await client.SendCommandAsync("TEST", throwIfErrorStatus: true)
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.AreEqual(expectedErrorCode, ex!.ErrorCode);
    Assert.IsEmpty(ex.ErrorText);

    Assert.IsNotNull(ex.Response);
    Assert.IsFalse(ex.Response.Success);
    Assert.AreEqual(ex.Response.Status, SkStackResponseStatus.Fail);
    Assert.That(ex.Response.StatusText, Is.EqualTo(errorCodeString.ToByteSequence()));
  }

  [Test]
  public void Response_StatusFailWithStatusText_AsException_ErrorCodeWithText()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER01 error text");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.SendCommandAsync("TEST", throwIfErrorStatus: true)
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.AreEqual(SkStackErrorCode.ER01, ex!.ErrorCode);
    Assert.AreEqual("error text", ex.ErrorText);

    Assert.IsNotNull(ex.Response);
    Assert.IsFalse(ex.Response.Success);
    Assert.AreEqual(ex.Response.Status, SkStackResponseStatus.Fail);
    Assert.That(ex.Response.StatusText, Is.EqualTo("ER01 error text".ToByteSequence()));
  }

  [Test]
  public async Task Response_StatusUnexpected()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("ERROR undefined status");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);

    var resp = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.IsFalse(resp.Success);
    Assert.AreEqual(SkStackResponseStatus.Undetermined, resp.Status);
    Assert.That(resp.StatusText, Is.EqualTo(ReadOnlyMemory<byte>.Empty));

    // must be able to continue processing next response
    stream.ResponseWriter.WriteLine("OK done.");

    var resp2 = await client.SendCommandAsync("TEST", throwIfErrorStatus: false);

    Assert.IsTrue(resp2.Success);
    Assert.AreEqual(SkStackResponseStatus.Ok, resp2.Status);
    Assert.That(resp2.StatusText, Is.EqualTo("done.".ToByteSequence()));
  }

  [Test]
  public async Task Response_Payload()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("LINE1");
    stream.ResponseWriter.WriteLine("LINE2");
    stream.ResponseWriter.WriteLine("LINE3");
    stream.ResponseWriter.WriteLine("OK");

    using var client = CreateClient(stream, serviceProvider: serviceProvider);
    var resp = await client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "LINE1".ToByteSequence()) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader , "LINE2".ToByteSequence()) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader, "LINE3".ToByteSequence()) &&
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
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
    Assert.IsNotNull(resp.Payload);
    Assert.AreEqual("3-line payload", resp.Payload);
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

    var client = CreateClient(stream, serviceProvider: serviceProvider);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync("TEST", throwIfErrorStatus: false).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsFalse(resp.Success);
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

    var client = CreateClient(stream, serviceProvider: serviceProvider);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync("TEST", throwIfErrorStatus: false).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
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

    var client = CreateClient(stream, serviceProvider: serviceProvider);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync("TEST", throwIfErrorStatus: false).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
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

    var client = CreateClient(stream, serviceProvider: serviceProvider);

#pragma warning disable CA2012
    var taskSendCommand = client.SendCommandAsync(
      command: "TEST",
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "LINE1".ToByteSequence()) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader, "LINE2".ToByteSequence()) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&
          SkStackTokenParser.ExpectToken(ref reader, "LINE3".ToByteSequence()) &&
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
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
    Assert.IsNotNull(resp.Payload);
    Assert.AreEqual("3-line payload", resp.Payload);
  }

  [Test]
  public async Task Response_IncompleteLine_NotificationalEvents_EVENT()
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

    var client = CreateClient(stream, serviceProvider: serviceProvider);

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
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }

  [Test]
  public async Task Response_IncompleteLine_NotificationalEvents_ERXUDP()
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

    var client = CreateClient(stream, serviceProvider: serviceProvider);

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
      Is.EqualTo("TEST\r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }
}
