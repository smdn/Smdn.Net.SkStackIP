// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientEventsTests : SkStackClientTestsBase {
  private class PseudoSynchronizingObject : ISynchronizeInvoke {
    public bool InvokeRequired { get; }
    private readonly bool expectSynchronousCall;

    public PseudoSynchronizingObject(bool invokeRequired, bool expectSynchronousCall)
    {
      InvokeRequired = invokeRequired;
      this.expectSynchronousCall = expectSynchronousCall;
    }

    public IAsyncResult BeginInvoke(Delegate method, object?[]? args)
    {
      if (expectSynchronousCall)
        Assert.Fail("synchronous call expected");

      Task.Run(() => method.DynamicInvoke(args));

      return null!;
    }

    public object EndInvoke(IAsyncResult result)
      => throw new NotImplementedException();

    public object Invoke(Delegate method, object?[]? args)
    {
      if (!expectSynchronousCall)
        Assert.Fail("asynchronous call expected");

      return method.DynamicInvoke(args)!;
    }
  }

  [Test]
  public void EventHandler_WithoutSynchronizingObject()
    => EventHandler(
      synchronizingObject: null
    );

  [Test]
  public void EventHandler_WithSynchronizingObject_InvokeRequired()
    => EventHandler(
      synchronizingObject: new PseudoSynchronizingObject(
        invokeRequired: true,
        expectSynchronousCall: false
      )
    );

  [Test]
  public void EventHandler_WithSynchronizingObject_InvokeNotRequired()
    => EventHandler(
      synchronizingObject: new PseudoSynchronizingObject(
        invokeRequired: false,
        expectSynchronousCall: true
      )
    );

  private void EventHandler(
    ISynchronizeInvoke? synchronizingObject
  )
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    client.SynchronizingObject = synchronizingObject;

    const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    using var waitHandle = new ManualResetEvent(false);
    Exception? thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionTerminated += (sender, e) => {
      try {
        try {
          Assert.That(sender, Is.SameAs(client), nameof(sender));

          Assert.That(e, Is.Not.Null, nameof(e));
          Assert.That(e.PanaSessionPeerAddress, Is.Not.Null, nameof(e.PanaSessionPeerAddress));
          Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(IPAddress.Parse(senderAddressString)), nameof(e.PanaSessionPeerAddress));
          Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionTerminationCompleted), nameof(e.EventNumber));

          raisedEventCount++;
        }
        catch (Exception ex) {
          thrownExceptionInEventHandler = ex;
        }
      }
      finally {
        waitHandle.Set();
      }
    };

    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 27 {senderAddressString}");

    var taskSendSKTERM = client.SendSKTERMAsync();

    if (!waitHandle.WaitOne(3000)) // wait for event handler finished
      Assert.Fail("event handler not called or not finished");

    Assert.DoesNotThrowAsync(async () => await taskSendSKTERM);

    Assert.That(thrownExceptionInEventHandler, Is.Null, nameof(thrownExceptionInEventHandler));
    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));
  }

  [Test]
  public void EventHandler_EventHandlerThrownException_WithoutSynchronizingObject()
    => EventHandler_EventHandlerThrownException(
      synchronizingObject: null
    );

  [Test]
  public void EventHandler_EventHandlerThrownException_WithSynchronizingObject_InvokeRequired()
    => EventHandler_EventHandlerThrownException(
      synchronizingObject: new PseudoSynchronizingObject(
        invokeRequired: true,
        expectSynchronousCall: false
      )
    );

  [Test]
  public void EventHandler_EventHandlerThrownException_WithSynchronizingObject_InvokeNotRequired()
    => EventHandler_EventHandlerThrownException(
      synchronizingObject: new PseudoSynchronizingObject(
        invokeRequired: false,
        expectSynchronousCall: true
      )
    );

  private void EventHandler_EventHandlerThrownException(
    ISynchronizeInvoke? synchronizingObject
  )
  {
    const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    using var waitHandle = new ManualResetEvent(false);
    var raisedEventCount = 0;

    client.SynchronizingObject = synchronizingObject;
    client.PanaSessionTerminated += (sender, e) => {
      try {
        raisedEventCount++;
        throw new Exception("exception thrown by event handler");
      }
      finally {
        waitHandle.Set();
      }
    };

    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 27 {senderAddressString}");

    var taskSendSKTERM = client.SendSKTERMAsync();

    if (!waitHandle.WaitOne(1000)) // wait for event handler finished
      Assert.Fail("event handler not called or not finished");

    Assert.DoesNotThrowAsync(async () => await taskSendSKTERM);

    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));
  }
}
