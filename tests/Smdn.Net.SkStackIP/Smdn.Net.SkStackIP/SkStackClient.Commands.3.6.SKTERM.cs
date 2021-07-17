// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKTERMTests : SkStackClientCommandsTestsBase {
    [Test]
    public void SKTERM_CompletedWithEVENT27()
    {
      const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var senderAddress = IPAddress.Parse(senderAddressString);

      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddressString} 02");
      stream.ResponseWriter.WriteLine("OK");

      async Task RaisePanaSessionTerminationEventsAsync()
      {
        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.WriteLine($"ERXUDP {senderAddressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.Write($"EVENT 2"); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.WriteLine($"7 {senderAddressString}");
      }

      using var client = SkStackClient.Create(stream, ServiceProvider);
      var taskSendCommand = client.SendSKTERMAsync();

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionTerminationEventsAsync());
      });

      var (response, isCompletedSuccessfully) = taskSendCommand.Result;

      Assert.IsTrue(response.Success);
      Assert.IsTrue(isCompletedSuccessfully);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKTERM\r\n".ToByteSequence())
      );
    }

    [Test]
    public void SKTERM_CompletedWithEVENT28()
    {
      const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var senderAddress = IPAddress.Parse(senderAddressString);

      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddressString} 02");
      stream.ResponseWriter.WriteLine("OK");

      async Task RaisePanaSessionTerminationEventsAsync()
      {
        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.Write($"EVENT 28"); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.WriteLine($" {senderAddressString}");
      }

      using var client = SkStackClient.Create(stream, ServiceProvider);
      var taskSendCommand = client.SendSKTERMAsync();

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionTerminationEventsAsync());
      });

      var (response, isCompletedSuccessfully) = taskSendCommand.Result;

      Assert.IsTrue(response.Success);
      Assert.IsFalse(isCompletedSuccessfully);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKTERM\r\n".ToByteSequence())
      );
    }

    [Test]
    public void SKTERM_FAIL()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL ER10");

      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.ThrowsAsync<SkStackErrorResponseException>(
        async () => await client.SendSKTERMAsync()
      );

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKTERM\r\n".ToByteSequence())
      );
    }
  }
}