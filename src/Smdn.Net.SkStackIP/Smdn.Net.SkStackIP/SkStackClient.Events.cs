// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  private SkStackERXUDPDataFormat erxudpDataFormat = SkStackERXUDPDataFormat.Raw; // RAW as default
  public SkStackERXUDPDataFormat ERXUDPDataFormat {
    get => erxudpDataFormat;
    set {
#if SYSTEM_ENUM_ISDEFINED_OF_TENUM
      if (!Enum.IsDefined(value))
#else
      if (!Enum.IsDefined(typeof(SkStackERXUDPDataFormat), value))
#endif
        throw new ArgumentException($"undefined value of {nameof(SkStackERXUDPDataFormat)}", nameof(ERXUDPDataFormat));

      erxudpDataFormat = value;
    }
  }

  /// <value><see cref="IPAddress"/> of current PANA session peer. <see langword="null"/> if PANA session has been terminated, expired, or not been established.</value>
  public IPAddress PanaSessionPeerAddress { get; private set; } = null;

#pragma warning disable CA2012
  private static readonly ValueTask<bool> TrueResultValueTask =
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMRESULT
    ValueTask.FromResult(true);
#else
    new(result: true);
#endif

  private static readonly ValueTask<bool> FalseResultValueTask =
#if SYSTEM_THREADING_TASKS_VALUETASK_FROMRESULT
    ValueTask.FromResult(false);
#else
    new(result: false);
#endif
#pragma warning restore CA2012

  private delegate bool ProcessNotificationalEventsFunc(ISkStackSequenceParserContext context);

#pragma warning disable CA1502 // TODO: refactor
  /// <returns>true if the first event processed and consumed, otherwise false.</returns>
  private ValueTask<bool> ProcessEventsAsync(
    ISkStackSequenceParserContext context,
    SkStackEventHandlerBase eventHandler // handles events that are triggered by commands
  )
  {
    var reader = context.CreateReader();

    if (reader.TryRead(out var firstByte)) {
      const byte firstByteOfEVENTOrERXUDP = (byte)'E';

      if (firstByte != firstByteOfEVENTOrERXUDP) {
        context.Ignore();
        return FalseResultValueTask;
      }
    }
    else {
      context.SetAsIncomplete();
      return FalseResultValueTask;
    }

    var statusEVENT = SkStackEventParser.TryExpectEVENT(context, out var ev);

    if (statusEVENT == OperationStatus.NeedMoreData) {
      context.SetAsIncomplete();
      return FalseResultValueTask;
    }
    else if (statusEVENT == OperationStatus.Done) {
      var eventHandlerStatesCompleted = eventHandler is not null && eventHandler.TryProcessEvent(ev);

      // log event
      switch (ev.Number) {
        case SkStackEventNumber.NeighborSolicitationReceived:
        case SkStackEventNumber.NeighborAdvertisementReceived:
        case SkStackEventNumber.EchoRequestReceived:
        case SkStackEventNumber.UdpSendCompleted:
          logger?.LogInfoIPEventReceived(ev);
          break;

        case SkStackEventNumber.PanaSessionEstablishmentError:
        case SkStackEventNumber.PanaSessionEstablishmentCompleted:
        case SkStackEventNumber.PanaSessionTerminationRequestReceived:
        case SkStackEventNumber.PanaSessionTerminationCompleted:
        case SkStackEventNumber.PanaSessionTerminationTimedOut:
        case SkStackEventNumber.PanaSessionExpired:
          logger?.LogInfoPanaEventReceived(ev);
          break;

        case SkStackEventNumber.TransmissionTimeControlLimitationActivated:
        case SkStackEventNumber.TransmissionTimeControlLimitationDeactivated:
          logger?.LogInfoAribStdT108EventReceived(ev);
          break;

        case SkStackEventNumber.WakeupSignalReceived:
          // TODO: log event
          break;
      }

      // raise event
      switch (ev.Number) {
        case SkStackEventNumber.PanaSessionEstablishmentCompleted:
          PanaSessionPeerAddress = ev.SenderAddress;
          RaiseEventPanaSessionEstablished(ev);
          break;

        case SkStackEventNumber.PanaSessionTerminationRequestReceived:
        case SkStackEventNumber.PanaSessionTerminationCompleted:
        case SkStackEventNumber.PanaSessionTerminationTimedOut:
          PanaSessionPeerAddress = null;
          RaiseEventPanaSessionTerminated(ev);
          break;

        case SkStackEventNumber.PanaSessionExpired:
          PanaSessionPeerAddress = null;
          RaiseEventPanaSessionExpired(ev);
          break;

        case SkStackEventNumber.TransmissionTimeControlLimitationActivated:
        case SkStackEventNumber.TransmissionTimeControlLimitationDeactivated:
          // TODO: raise event
          break;

        case SkStackEventNumber.WakeupSignalReceived:
          RaiseEventWokeUp(ev);
          break;

        case SkStackEventNumber.BeaconReceived:
          SkStackUnexpectedResponseException.ThrowIfUnexpectedSubsequentEventCode(
            subsequentEventCode: ev.ExpectedSubsequentEventCode,
            expectedEventCode: SkStackEventCode.EPANDESC
          );
          break;

        case SkStackEventNumber.EnergyDetectScanCompleted:
          SkStackUnexpectedResponseException.ThrowIfUnexpectedSubsequentEventCode(
            subsequentEventCode: ev.ExpectedSubsequentEventCode,
            expectedEventCode: SkStackEventCode.EEDSCAN
          );
          break;
      }

      if (eventHandlerStatesCompleted)
        context.Complete();
      else
        context.Continue();

      return TrueResultValueTask;
    }

    var statusERXUDP = SkStackEventParser.TryExpectERXUDP(context, erxudpDataFormat, out var erxudp, out var erxudpData, out var erxudpDataLength);

    if (statusERXUDP == OperationStatus.NeedMoreData) {
      context.SetAsIncomplete();
      return FalseResultValueTask;
    }
    else if (statusERXUDP == OperationStatus.Done) {
      logger?.LogInfoIPEventReceived(erxudp, erxudpData);

      return ProcessERXUDPAsync();

      async ValueTask<bool> ProcessERXUDPAsync()
      {
        await OnERXUDPAsync(
          localPort: erxudp.LocalEndPoint.Port,
          remoteAddress: erxudp.RemoteEndPoint.Address,
          data: erxudpData,
          dataLength: erxudpDataLength,
          dataFormat: erxudpDataFormat
        ).ConfigureAwait(false);

        context.Continue();
        return true;
      }
    }

    // if (status == OperationStatus.InvalidData)
    context.Ignore();
    return FalseResultValueTask;
  }
#pragma warning restore CA1502

  public ISynchronizeInvoke SynchronizingObject { get; set; }

  public event EventHandler<SkStackPanaSessionEventArgs> PanaSessionEstablished;
  public event EventHandler<SkStackPanaSessionEventArgs> PanaSessionTerminated;
  public event EventHandler<SkStackPanaSessionEventArgs> PanaSessionExpired;

  internal void RaiseEventPanaSessionEstablished(SkStackEvent baseEvent) => RaiseEventPanaSession(PanaSessionEstablished, baseEvent);
  internal void RaiseEventPanaSessionTerminated(SkStackEvent baseEvent) => RaiseEventPanaSession(PanaSessionTerminated, baseEvent);
  internal void RaiseEventPanaSessionExpired(SkStackEvent baseEvent) => RaiseEventPanaSession(PanaSessionExpired, baseEvent);

  private void RaiseEventPanaSession(EventHandler<SkStackPanaSessionEventArgs> ev, SkStackEvent baseEvent)
  {
    if (ev is null)
      return; // return without creating event args if event hanlder is null

    InvokeEvent(SynchronizingObject, ev, this, new SkStackPanaSessionEventArgs(baseEvent));
  }

  public event EventHandler<SkStackEventArgs> Slept;
  public event EventHandler<SkStackEventArgs> WokeUp;

  internal void RaiseEventSlept() => RaiseEvent(Slept, default);
  private void RaiseEventWokeUp(SkStackEvent baseEvent) => RaiseEvent(WokeUp, baseEvent);

  private void RaiseEvent(EventHandler<SkStackEventArgs> ev, SkStackEvent baseEvent)
  {
    if (ev is null)
      return; // return without creating event args if event hanlder is null

    InvokeEvent(SynchronizingObject, ev, this, new SkStackEventArgs(baseEvent));
  }

  private static void InvokeEvent<TEventArgs>(
    ISynchronizeInvoke synchronizingObject,
    EventHandler<TEventArgs> ev,
    object sender,
    TEventArgs args
  )
    where TEventArgs : SkStackEventArgs
  {
    if (synchronizingObject is null || !synchronizingObject.InvokeRequired) {
      try {
        ev(sender, args);
      }
#pragma warning disable CA1031
      catch {
        // ignore exceptions
      }
#pragma warning restore CA1031
    }
    else {
      synchronizingObject.BeginInvoke(
        method: ev,
        args: new object[] { sender, args }
      );
    }
  }
}
