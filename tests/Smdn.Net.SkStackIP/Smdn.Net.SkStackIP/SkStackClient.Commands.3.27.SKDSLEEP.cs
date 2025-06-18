// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKDSLEEPTests : SkStackClientTestsBase {
  [Test]
  public void SKDSLEEP()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    Exception? thrownExceptionInSleptEventHandler = null;
    Exception? thrownExceptionInWokeUpEventHandler = null;
    var raisedSleptEventCount = 0;
    var raisedWokeUpEventCount = 0;

    client.Slept += (sender, e) => {
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.Undefined), nameof(e.EventNumber));
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

    SkStackResponse? response = default;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKDSLEEPAsync(waitUntilWakeUp: false));

    Assert.That(thrownExceptionInSleptEventHandler, Is.Null, nameof(thrownExceptionInSleptEventHandler));
    Assert.That(raisedSleptEventCount, Is.EqualTo(1), nameof(raisedSleptEventCount));

    Assert.That(thrownExceptionInWokeUpEventHandler, Is.Null, nameof(thrownExceptionInWokeUpEventHandler));
    Assert.That(raisedWokeUpEventCount, Is.Zero, nameof(raisedWokeUpEventCount));

    Assert.That(response, Is.Not.Null);
    Assert.That(response!.Success, Is.True);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKDSLEEP\r\n".ToByteSequence())
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

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    Exception? thrownExceptionInSleptEventHandler = null;
    Exception? thrownExceptionInWokeUpEventHandler = null;
    var raisedSleptEventCount = 0;
    var raisedWokeUpEventCount = 0;
    DateTime sleptEventRaisedAt = default;
    DateTime wokeUpEventRaisedAt = default;

    client.Slept += (sender, e) => {
      sleptEventRaisedAt = DateTime.Now;
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.Undefined), nameof(e.EventNumber));
        raisedSleptEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInSleptEventHandler = ex;
      }
    };
    client.WokeUp += (sender, e) => {
      wokeUpEventRaisedAt = DateTime.Now;
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.WakeupSignalReceived), nameof(e.EventNumber));
        raisedWokeUpEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInWokeUpEventHandler = ex;
      }
    };

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKDSLEEPAsync(waitUntilWakeUp: true).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaiseWakeupSignalReceivedEventsAsync())
    );
#pragma warning restore CA2012

    Assert.That(thrownExceptionInSleptEventHandler, Is.Null, nameof(thrownExceptionInSleptEventHandler));
    Assert.That(raisedSleptEventCount, Is.EqualTo(1), nameof(raisedSleptEventCount));

    Assert.That(thrownExceptionInWokeUpEventHandler, Is.Null, nameof(thrownExceptionInWokeUpEventHandler));
    Assert.That(raisedWokeUpEventCount, Is.EqualTo(1), nameof(raisedWokeUpEventCount));

    Assert.That(sleptEventRaisedAt, Is.LessThan(wokeUpEventRaisedAt), "event raised time");

    var response = taskSendCommand.Result;

    Assert.That(response, Is.Not.Null);
    Assert.That(response.Success, Is.True);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKDSLEEP\r\n".ToByteSequence())
    );
  }
}
