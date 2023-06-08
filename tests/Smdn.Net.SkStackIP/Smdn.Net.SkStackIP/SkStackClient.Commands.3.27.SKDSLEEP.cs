// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKDSLEEPTests : SkStackClientTestsBase {
  [Test]
  public void SKDSLEEP()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, ServiceProvider);
    Exception thrownExceptionInSleptEventHandler = null;
    Exception thrownExceptionInWokeUpEventHandler = null;
    var raisedSleptEventCount = 0;
    var raisedWokeUpEventCount = 0;

    client.Slept += (sender, e) => {
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(SkStackEventNumber.Undefined, e.EventNumber, nameof(e.EventNumber));
        raisedSleptEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInSleptEventHandler = ex;
      }
    };
    client.WokeUp += (sender, e) => {
      try {
        raisedWokeUpEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInWokeUpEventHandler = ex;
      }
    };

    SkStackResponse response = default;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKDSLEEPAsync(waitUntilWakeUp: false));

    Assert.IsNull(thrownExceptionInSleptEventHandler, nameof(thrownExceptionInSleptEventHandler));
    Assert.AreEqual(1, raisedSleptEventCount, nameof(raisedSleptEventCount));

    Assert.IsNull(thrownExceptionInWokeUpEventHandler, nameof(thrownExceptionInWokeUpEventHandler));
    Assert.AreEqual(0, raisedWokeUpEventCount, nameof(raisedWokeUpEventCount));

    Assert.IsNotNull(response);
    Assert.IsTrue(response.Success);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKDSLEEP\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKDSLEEP_WaitUntilAwoken()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaiseWakeupSignalReceivedEventsAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write($" C0"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
    }

    using var client = new SkStackClient(stream, ServiceProvider);
    Exception thrownExceptionInSleptEventHandler = null;
    Exception thrownExceptionInWokeUpEventHandler = null;
    var raisedSleptEventCount = 0;
    var raisedWokeUpEventCount = 0;
    DateTime sleptEventRaisedAt = default;
    DateTime wokeUpEventRaisedAt = default;

    client.Slept += (sender, e) => {
      sleptEventRaisedAt = DateTime.Now;
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(SkStackEventNumber.Undefined, e.EventNumber, nameof(e.EventNumber));
        raisedSleptEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInSleptEventHandler = ex;
      }
    };
    client.WokeUp += (sender, e) => {
      wokeUpEventRaisedAt = DateTime.Now;
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(SkStackEventNumber.WakeupSignalReceived, e.EventNumber, nameof(e.EventNumber));
        raisedWokeUpEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInWokeUpEventHandler = ex;
      }
    };

    var taskSendCommand = client.SendSKDSLEEPAsync(waitUntilWakeUp: true);

    Assert.DoesNotThrowAsync(async () => {
      await Task.WhenAll(taskSendCommand.AsTask(), RaiseWakeupSignalReceivedEventsAsync());
    });

    Assert.IsNull(thrownExceptionInSleptEventHandler, nameof(thrownExceptionInSleptEventHandler));
    Assert.AreEqual(1, raisedSleptEventCount, nameof(raisedSleptEventCount));

    Assert.IsNull(thrownExceptionInWokeUpEventHandler, nameof(thrownExceptionInWokeUpEventHandler));
    Assert.AreEqual(1, raisedWokeUpEventCount, nameof(raisedWokeUpEventCount));

    Assert.That(sleptEventRaisedAt, Is.LessThan(wokeUpEventRaisedAt), "event raised time");

    var response = taskSendCommand.Result;

    Assert.IsNotNull(response);
    Assert.IsTrue(response.Success);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKDSLEEP\r\n".ToByteSequence())
    );
  }
}
