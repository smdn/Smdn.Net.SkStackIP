// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKTERMTests : SkStackClientTestsBase {
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

      using var client = new SkStackClient(stream, ServiceProvider);
      Exception thrownExceptionInEventHandler = null;
      var raisedEventCount = 0;

      client.PanaSessionTerminated += (sender, e) => {
        try {
          Assert.AreSame(client, sender, nameof(sender));
          Assert.IsNotNull(e, nameof(e));
          Assert.AreEqual(senderAddress, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
          Assert.AreEqual(SkStackEventNumber.PanaSessionTerminationCompleted, e.EventNumber, nameof(e.EventNumber));
          raisedEventCount++;
        }
        catch (Exception ex) {
          thrownExceptionInEventHandler = ex;
        }
      };

      var taskSendCommand = client.SendSKTERMAsync();

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionTerminationEventsAsync());
      });

      Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
      Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

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

      using var client = new SkStackClient(stream, ServiceProvider);
      Exception thrownExceptionInEventHandler = null;
      var raisedEventCount = 0;

      client.PanaSessionTerminated += (sender, e) => {
        try {
          Assert.AreSame(client, sender, nameof(sender));
          Assert.IsNotNull(e, nameof(e));
          Assert.AreEqual(senderAddress, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
          Assert.AreEqual(SkStackEventNumber.PanaSessionTerminationTimedOut, e.EventNumber, nameof(e.EventNumber));
          raisedEventCount++;
        }
        catch (Exception ex) {
          thrownExceptionInEventHandler = ex;
        }
      };

      var taskSendCommand = client.SendSKTERMAsync();

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionTerminationEventsAsync());
      });

      Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
      Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

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

      using var client = new SkStackClient(stream, ServiceProvider);
      var raisedEventCount = 0;

      client.PanaSessionTerminated += (sender, e) => raisedEventCount++;

      Assert.ThrowsAsync<SkStackErrorResponseException>(
        async () => await client.SendSKTERMAsync()
      );

      Assert.AreEqual(0, raisedEventCount, nameof(raisedEventCount));

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKTERM\r\n".ToByteSequence())
      );
    }
  }
}