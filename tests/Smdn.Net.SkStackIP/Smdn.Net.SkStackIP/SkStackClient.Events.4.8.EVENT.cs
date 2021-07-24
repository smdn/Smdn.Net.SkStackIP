// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientEventsEVENTTests : SkStackClientTestsBase {
    [Test]
    public void EVENT_26_RaiseEventPanaSessionTerminated()
    {
      const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var senderAddress = IPAddress.Parse(senderAddressString);

      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      var raisedEventCount = 0;
      Exception thrownExceptionInEventHandler = null;

      client.PanaSessionTerminated += (sender, e) => {
        try {
          Assert.AreSame(client, sender, nameof(sender));
          Assert.IsNotNull(e, nameof(e));
          Assert.AreEqual(senderAddress, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
          Assert.AreEqual(SkStackEventNumber.PanaSessionTerminationRequestReceived, e.EventNumber, nameof(e.EventNumber));
          raisedEventCount++;
        }
        catch (Exception ex) {
          thrownExceptionInEventHandler = ex;
        }
      };

      stream.ResponseWriter.WriteLine($"EVENT 26 {senderAddressString}");
      stream.ResponseWriter.WriteLine("OK"); // TEST

      Assert.DoesNotThrowAsync(async () => await client.SendCommandAsync("TEST".ToByteSequence()));

      Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
      Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));
    }

    [Test]
    public void EVENT_29_RaiseEventPanaSessionExpired()
    {
      const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var senderAddress = IPAddress.Parse(senderAddressString);

      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Exception thrownExceptionInEventHandler = null;
      var raisedEventCount = 0;

      client.PanaSessionExpired += (sender, e) => {
        try {
          Assert.AreSame(client, sender, nameof(sender));
          Assert.IsNotNull(e, nameof(e));
          Assert.AreEqual(senderAddress, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
          Assert.AreEqual(SkStackEventNumber.PanaSessionExpired, e.EventNumber, nameof(e.EventNumber));
          raisedEventCount++;
        }
        catch (Exception ex) {
          thrownExceptionInEventHandler = ex;
        }
      };

      stream.ResponseWriter.WriteLine($"EVENT 29 {senderAddressString}");
      stream.ResponseWriter.WriteLine("OK"); // TEST

      Assert.DoesNotThrowAsync(async () => await client.SendCommandAsync("TEST".ToByteSequence()));

      Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
      Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));
    }
  }
}