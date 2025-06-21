// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKTERMTests : SkStackClientTestsBase {
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
  public async Task SKTERM_CompletedWithEVENT27()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    await JoinAsync(client, stream, addressString);

    stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionTerminationEventsAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 2"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($"7 {addressString}");
    }

    Exception? thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionTerminated += (sender, e) => {
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(address), nameof(e.PanaSessionPeerAddress));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationCompleted), nameof(e.EventNumber));
        Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationCompleted), nameof(client.PanaSessionState));
        Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
        Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKTERMAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionTerminationEventsAsync())
    );
#pragma warning restore CA2012

    Assert.That(thrownExceptionInEventHandler, Is.Null, nameof(thrownExceptionInEventHandler));
    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));

    var (response, isCompletedSuccessfully) = taskSendCommand.Result;

    Assert.That(response.Success, Is.True);
    Assert.That(isCompletedSuccessfully, Is.True);

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationCompleted), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Assert.That(
      client.ThrowIfPanaSessionNotAlive,
      Throws.TypeOf<SkStackPanaSessionTerminatedException>()
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKTERM\r\n".ToByteSequence())
    );
  }

  [Test]
  public async Task SKTERM_CompletedWithEVENT28()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    await JoinAsync(client, stream, addressString);

    stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionTerminationEventsAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 28"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($" {addressString}");
    }

    Exception? thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionTerminated += (sender, e) => {
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(address), nameof(e.PanaSessionPeerAddress));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationTimedOut), nameof(e.EventNumber));
        Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationTimedOut), nameof(client.PanaSessionState));
        Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
        Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(client.PanaSessionState));

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKTERMAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionTerminationEventsAsync())
    );
#pragma warning restore CA2012

    Assert.That(thrownExceptionInEventHandler, Is.Null, nameof(thrownExceptionInEventHandler));
    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));

    var (response, isCompletedSuccessfully) = taskSendCommand.Result;

    Assert.That(response.Success, Is.True);
    Assert.That(isCompletedSuccessfully, Is.False);

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Assert.That(
      client.ThrowIfPanaSessionNotAlive,
      Throws.TypeOf<SkStackPanaSessionTerminatedException>()
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKTERM\r\n".ToByteSequence())
    );
  }

  [Test]
  public async Task SKTERM_FAIL()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    await JoinAsync(client, stream, addressString);

    stream.ResponseWriter.WriteLine("FAIL ER10");

    var raisedEventCount = 0;

    client.PanaSessionTerminated += (sender, e) => raisedEventCount++;

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.SendSKTERMAsync()
    );

    Assert.That(raisedEventCount, Is.Zero, nameof(raisedEventCount));

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.PanaSessionState, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(client.PanaSessionState));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    Assert.That(
      client.ThrowIfPanaSessionNotAlive,
      Throws.Nothing
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKTERM\r\n".ToByteSequence())
    );
  }
}
