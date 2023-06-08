// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

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

    Assert.AreEqual(client.PanaSessionPeerAddress, address);

    stream.ClearSentData();
  }

  [Test]
  public async Task SKTERM_CompletedWithEVENT27()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

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

    Exception thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionTerminated += (sender, e) => {
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(address, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
        Assert.AreEqual(SkStackEventNumber.PanaSessionTerminationCompleted, e.EventNumber, nameof(e.EventNumber));
        Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKTERMAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionTerminationEventsAsync())
    );
#pragma warning restore CA2012

    Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
    Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

    var (response, isCompletedSuccessfully) = taskSendCommand.Result;

    Assert.IsTrue(response.Success);
    Assert.IsTrue(isCompletedSuccessfully);

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTERM\r\n".ToByteSequence())
    );
  }

  [Test]
  public async Task SKTERM_CompletedWithEVENT28()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

    await JoinAsync(client, stream, addressString);

    stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionTerminationEventsAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 28"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($" {addressString}");
    }

    Exception thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionTerminated += (sender, e) => {
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(address, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
        Assert.AreEqual(SkStackEventNumber.PanaSessionTerminationTimedOut, e.EventNumber, nameof(e.EventNumber));
        Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKTERMAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionTerminationEventsAsync())
    );
#pragma warning restore CA2012

    Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
    Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

    var (response, isCompletedSuccessfully) = taskSendCommand.Result;

    Assert.IsTrue(response.Success);
    Assert.IsFalse(isCompletedSuccessfully);

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTERM\r\n".ToByteSequence())
    );
  }

  [Test]
  public async Task SKTERM_FAIL()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

    await JoinAsync(client, stream, addressString);

    stream.ResponseWriter.WriteLine("FAIL ER10");

    var raisedEventCount = 0;

    client.PanaSessionTerminated += (sender, e) => raisedEventCount++;

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.SendSKTERMAsync()
    );

    Assert.AreEqual(0, raisedEventCount, nameof(raisedEventCount));

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTERM\r\n".ToByteSequence())
    );
  }
}
