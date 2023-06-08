// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKREJOINTests : SkStackClientTestsBase {
  [Test]
  public void SKREJOIN()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {addressString}");
      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 00");
      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 2"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($"5 {addressString}");
    }

    using var client = new SkStackClient(stream, ServiceProvider);
    Exception thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => {
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(address, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
        Assert.AreEqual(SkStackEventNumber.PanaSessionEstablishmentCompleted, e.EventNumber, nameof(e.EventNumber));
        Assert.AreEqual(client.PanaSessionPeerAddress, e.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    var taskSendCommand = client.SendSKREJOINAsync();

    Assert.DoesNotThrowAsync(async () => {
      await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionEstablishmentEventsAsync());
    });

    Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
    Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

    var (response, rejoinedSessionPeerAddress) = taskSendCommand.Result;

    Assert.IsTrue(response.Success);
    Assert.AreEqual(rejoinedSessionPeerAddress, address);

    Assert.AreEqual(client.PanaSessionPeerAddress, address, nameof(client.PanaSessionPeerAddress));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKREJOIN\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKREJOIN_FailedByEVENT24()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {addressString}");
      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 00");
      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVEN"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write($"T 2"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($"4 {addressString}");
    }

    using var client = new SkStackClient(stream, ServiceProvider);
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => raisedEventCount++;

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    var taskSendCommand = client.SendSKREJOINAsync();

    var ex = Assert.ThrowsAsync<SkStackPanaSessionEstablishmentException>(async () => {
      await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionEstablishmentEventsAsync());
    });

    Assert.AreEqual(SkStackEventNumber.PanaSessionEstablishmentError, ex.EventNumber);
    Assert.AreEqual(address, ex.Address);

    Assert.AreEqual(0, raisedEventCount, nameof(raisedEventCount));

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKREJOIN\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKREJOIN_Fail()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER10");

    using var client = new SkStackClient(stream, ServiceProvider);
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => raisedEventCount++;

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(async () => await client.SendSKREJOINAsync());

    Assert.AreEqual(SkStackErrorCode.ER10, ex.ErrorCode);

    Assert.AreEqual(0, raisedEventCount, nameof(raisedEventCount));

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKREJOIN\r\n".ToByteSequence())
    );
  }
}
