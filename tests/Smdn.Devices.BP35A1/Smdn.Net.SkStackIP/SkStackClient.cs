// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Is = Smdn.NUnitExtensions.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientTests {
    private static ReadOnlyMemory<byte> ToByteSequence(string str)
      => str is null ? ReadOnlyMemory<byte>.Empty : Encoding.ASCII.GetBytes(str);

    private static readonly ReadOnlyMemory<byte> StatusOk = ToByteSequence("OK");
    private static readonly ReadOnlyMemory<byte> StatusFail = ToByteSequence("FAIL");

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

    [Test]
    public async Task Command_CommandWithoutArguments()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"));

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
    }

    [Test]
    public async Task Response_CommandWithArguments()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"), new[] {"ARG1", "ARG2", "ARG3"});

      Assert.AreEqual(
        "TEST ARG1 ARG2 ARG3\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
    }

    [Test]
    public async Task Response_IgnoreEchoBack()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("TEST ARG1 ARG2 ARG3");
      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"), new[] {"ARG1", "ARG2", "ARG3"});

      Assert.AreEqual(
        "TEST ARG1 ARG2 ARG3\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
      Assert.IsEmpty(resp.Lines);
    }

    [Test]
    public async Task Response_StatusOk()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"));

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
      Assert.AreEqual(resp.Status, SkStackResponseStatus.Ok);
      Assert.That(resp.StatusText, Is.Equal(ReadOnlyMemory<byte>.Empty));
    }

    [Test]
    public async Task Response_StatusOkWithStatusText()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK DONE");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"));

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
      Assert.AreEqual(resp.Status, SkStackResponseStatus.Ok);
      Assert.That(resp.StatusText, Is.Equal(ToByteSequence("DONE")));
    }

    [Test]
    public async Task Response_StatusFail()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: false);

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsFalse(resp.Success);
      Assert.AreEqual(resp.Status, SkStackResponseStatus.Fail);
      Assert.That(resp.StatusText, Is.Equal(ReadOnlyMemory<byte>.Empty));
    }

    [Test]
    public async Task Response_StatusFailWithStatusText()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL REASON");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: false);

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsFalse(resp.Success);
      Assert.AreEqual(resp.Status, SkStackResponseStatus.Fail);
      Assert.That(resp.StatusText, Is.Equal(ToByteSequence("REASON")));
    }

    [Test]
    public void Response_StatusFailWithStatusText_AsException_ErrorCode()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL ER01");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(
        async () => await client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: true)
      );

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.AreEqual("ER01", ex.ErrorCode);
      Assert.IsEmpty(ex.ErrorText);

      Assert.IsNotNull(ex.Response);
      Assert.IsFalse(ex.Response.Success);
      Assert.AreEqual(ex.Response.Status, SkStackResponseStatus.Fail);
      Assert.That(ex.Response.StatusText, Is.Equal(ToByteSequence("ER01")));
    }

    [Test]
    public void Response_StatusFailWithStatusText_AsException_ErrorCodeUnknown()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL ERXX");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(
        async () => await client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: true)
      );

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.AreEqual("ERXX", ex.ErrorCode);
      Assert.IsEmpty(ex.ErrorText);

      Assert.IsNotNull(ex.Response);
      Assert.IsFalse(ex.Response.Success);
      Assert.AreEqual(ex.Response.Status, SkStackResponseStatus.Fail);
      Assert.That(ex.Response.StatusText, Is.Equal(ToByteSequence("ERXX")));
    }

    [Test]
    public void Response_StatusFailWithStatusText_AsException_ErrorCodeWithText()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL ER01 error text");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(
        async () => await client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: true)
      );

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.AreEqual("ER01", ex.ErrorCode);
      Assert.AreEqual("error text", ex.ErrorText);

      Assert.IsNotNull(ex.Response);
      Assert.IsFalse(ex.Response.Success);
      Assert.AreEqual(ex.Response.Status, SkStackResponseStatus.Fail);
      Assert.That(ex.Response.StatusText, Is.Equal(ToByteSequence("ER01 error text")));
    }

    [Test]
    public async Task Response_Lines()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("LINE1");
      stream.ResponseWriter.WriteLine("LINE2");
      stream.ResponseWriter.WriteLine("LINE3");
      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var resp = await client.SendCommandAsync(ToByteSequence("TEST"));

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
      Assert.AreEqual(resp.Lines.Count, 3);
      Assert.That(resp.Lines[0], Is.Equal(ToByteSequence("LINE1")));
      Assert.That(resp.Lines[1], Is.Equal(ToByteSequence("LINE2")));
      Assert.That(resp.Lines[2], Is.Equal(ToByteSequence("LINE3")));
    }

    [Test]
    public void Response_IncompleteLine_Status()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.Write("FA");

      async Task CompleteResponseAsync()
      {
        await Task.Delay(100);
        stream.ResponseWriter.WriteLine("IL");
      }

      var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var taskSendCommand = client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: false);
      var taskCompleteResponse = CompleteResponseAsync();

      Task.WaitAll(taskSendCommand, taskCompleteResponse);

      var resp = taskSendCommand.Result;

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsFalse(resp.Success);
      Assert.AreEqual(resp.Lines.Count, 0);
    }

    [Test]
    public void Response_IncompleteLine_NewLine()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.Write("OK\r");

      async Task CompleteResponseAsync()
      {
        await Task.Delay(100);
        stream.ResponseWriter.Write("\n");
      }

      var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var taskSendCommand = client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: false);
      var taskCompleteResponse = CompleteResponseAsync();

      Task.WaitAll(taskSendCommand, taskCompleteResponse);

      var resp = taskSendCommand.Result;

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
      Assert.AreEqual(resp.Lines.Count, 0);
    }

    [Test]
    public void Response_IncompleteLine_EchoBack()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.Write("TE");

      async Task CompleteResponseAsync()
      {
        await Task.Delay(100);
        stream.ResponseWriter.WriteLine("ST");
        stream.ResponseWriter.WriteLine("OK");
      }

      var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var taskSendCommand = client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: false);
      var taskCompleteResponse = CompleteResponseAsync();

      Task.WaitAll(taskSendCommand, taskCompleteResponse);

      var resp = taskSendCommand.Result;

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
      Assert.AreEqual(resp.Lines.Count, 0);
    }

    [Test]
    public void Response_IncompleteLine_Lines()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("LINE1");
      stream.ResponseWriter.Write("LI");

      async Task CompleteResponseAsync()
      {
        stream.ResponseWriter.WriteLine("NE2");
        stream.ResponseWriter.Write("LI");
        await Task.Delay(100);
        stream.ResponseWriter.WriteLine("NE3");
        stream.ResponseWriter.WriteLine("OK");
      }

      var client = SkStackClient.Create(stream, serviceProvider: serviceProvider);
      var taskSendCommand = client.SendCommandAsync(ToByteSequence("TEST"), throwIfErrorStatus: false);
      var taskCompleteResponse = CompleteResponseAsync();

      Task.WaitAll(taskSendCommand, taskCompleteResponse);

      var resp = taskSendCommand.Result;

      Assert.AreEqual(
        "TEST\r\n",
        stream.ReadSentData()
      );

      Assert.IsTrue(resp.Success);
      Assert.AreEqual(resp.Lines.Count, 3);
      Assert.That(resp.Lines[0], Is.Equal(ToByteSequence("LINE1")));
      Assert.That(resp.Lines[1], Is.Equal(ToByteSequence("LINE2")));
      Assert.That(resp.Lines[2], Is.Equal(ToByteSequence("LINE3")));
    }
  }
}
