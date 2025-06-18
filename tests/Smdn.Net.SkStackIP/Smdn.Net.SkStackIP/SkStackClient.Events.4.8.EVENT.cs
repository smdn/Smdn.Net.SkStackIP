// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientEventsEVENTTests : SkStackClientTestsBase {
  private static async Task JoinAsync(
    SkStackClient client,
    PseudoSkStackStream stream,
    string addressString
  )
  {
    var address = IPAddress.Parse(addressString);

    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 25 {addressString}");

    await client.SendSKJOINAsync(address);

    Assert.That(address, Is.EqualTo(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    stream.ClearSentData();
  }

  [Test]
  public async Task EVENT_26_RaiseEventPanaSessionTerminated()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    await JoinAsync(client, stream, addressString);

    var raisedEventCount = 0;
    Exception? thrownExceptionInEventHandler = null;

    client.PanaSessionTerminated += (sender, e) => {
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(address), nameof(e.PanaSessionPeerAddress));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationRequestReceived), nameof(e.EventNumber));
        Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
        Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationRequestReceived), nameof(client.PanaSessionState));
        Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    stream.ResponseWriter.WriteLine($"EVENT 26 {addressString}");
    // SKVER
    stream.ResponseWriter.WriteLine("EVER 1.2.10");
    stream.ResponseWriter.WriteLine("OK");

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    Assert.DoesNotThrowAsync(async () => await client.SendSKVERAsync());

    Assert.That(thrownExceptionInEventHandler, Is.Null, nameof(thrownExceptionInEventHandler));
    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationRequestReceived), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));
  }

  [Test]
  public async Task EVENT_29_RaiseEventPanaSessionExpired()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    await JoinAsync(client, stream, addressString);

    Exception? thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionExpired += (sender, e) => {
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(address), nameof(e.PanaSessionPeerAddress));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionExpired), nameof(e.EventNumber));
        Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
        Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionExpired), nameof(client.PanaSessionState));
        Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    stream.ResponseWriter.WriteLine($"EVENT 29 {addressString}");
    // SKVER
    stream.ResponseWriter.WriteLine("EVER 1.2.10");
    stream.ResponseWriter.WriteLine("OK");

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    Assert.DoesNotThrowAsync(async () => await client.SendSKVERAsync());

    Assert.That(thrownExceptionInEventHandler, Is.Null, nameof(thrownExceptionInEventHandler));
    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionExpired), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));
  }
}
