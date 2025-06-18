// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040

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

  private readonly Dictionary<IPAddress, bool> lastUdpSendResult = new(capacity: 2);

#pragma warning disable CA1502 // TODO: refactor
  /// <returns><see langword="true"/> if the first event processed and consumed, otherwise <see langword="false"/>.</returns>
  private ValueTask<bool> ProcessEventsAsync(
    ISkStackSequenceParserContext context,
    SkStackEventHandlerBase? eventHandler, // handles events that are triggered by commands
    CancellationToken cancellationToken
  )
  {
    var reader = context.CreateReader();

    if (reader.TryRead(out var firstByte)) {
      const byte FirstByteOfEVENTOrERXUDP = (byte)'E';

      if (firstByte != FirstByteOfEVENTOrERXUDP) {
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
          Logger?.LogInfoIPEventReceived(ev);
          break;

        case SkStackEventNumber.PanaSessionEstablishmentError:
        case SkStackEventNumber.PanaSessionEstablishmentCompleted:
        case SkStackEventNumber.PanaSessionTerminationRequestReceived:
        case SkStackEventNumber.PanaSessionTerminationCompleted:
        case SkStackEventNumber.PanaSessionTerminationTimedOut:
        case SkStackEventNumber.PanaSessionExpired:
          Logger?.LogInfoPanaEventReceived(ev);
          break;

        case SkStackEventNumber.TransmissionTimeControlLimitationActivated:
        case SkStackEventNumber.TransmissionTimeControlLimitationDeactivated:
          Logger?.LogInfoAribStdT108EventReceived(ev);
          break;

        case SkStackEventNumber.WakeupSignalReceived:
          // TODO: log event
          break;
      }

      // update state and raise event
      switch (ev.Number) {
        case SkStackEventNumber.PanaSessionEstablishmentError:
          PanaSessionState = ev.Number;
          PanaSessionPeerAddress = null;
          break;

        case SkStackEventNumber.PanaSessionEstablishmentCompleted:
          PanaSessionState = ev.Number;
          PanaSessionPeerAddress = ev.SenderAddress;
          RaiseEventPanaSessionEstablished(ev);
          break;

        case SkStackEventNumber.PanaSessionTerminationRequestReceived:
        case SkStackEventNumber.PanaSessionTerminationCompleted:
        case SkStackEventNumber.PanaSessionTerminationTimedOut:
          PanaSessionState = ev.Number;
          PanaSessionPeerAddress = null;
          RaiseEventPanaSessionTerminated(ev);
          break;

        case SkStackEventNumber.PanaSessionExpired:
          PanaSessionState = ev.Number;
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

        case SkStackEventNumber.UdpSendCompleted:
#if DEBUG
          if (!ev.HasSenderAddress)
            throw new InvalidOperationException($"{nameof(ev.SenderAddress)} must not be null");
#endif
          switch (ev.Parameter) {
            case 0: // success
              lastUdpSendResult[ev.SenderAddress!] = true;
              break;

            case 1: // failed
              lastUdpSendResult[ev.SenderAddress!] = false;
              break;

            case 2: // performed Neighbor Solicitation
            default:
              break; // nothing to do
          }

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

    var statusERXUDP = SkStackEventParser.TryExpectERXUDP(
      context,
      erxudpDataFormat,
      out var erxudp,
      out var erxudpData,
      out var erxudpDataLength
    );

    if (statusERXUDP == OperationStatus.NeedMoreData) {
      context.SetAsIncomplete();
      return FalseResultValueTask;
    }
    else if (statusERXUDP == OperationStatus.Done) {
      Logger?.LogInfoIPEventReceived(erxudp, erxudpData);

      return ProcessERXUDPAsync();

      async ValueTask<bool> ProcessERXUDPAsync()
      {
        await OnERXUDPAsync(
          localPort: erxudp.LocalEndPoint.Port,
          remoteAddress: erxudp.RemoteEndPoint.Address,
          data: erxudpData,
          dataLength: erxudpDataLength,
          dataFormat: erxudpDataFormat,
          cancellationToken: cancellationToken
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

  /// <summary>
  /// Gets or sets the object used to marshal the event handler calls that are issued when an event received.
  /// </summary>
  public ISynchronizeInvoke? SynchronizingObject { get; set; }

  /// <summary>
  /// Occurs when a PANA session is established.
  /// </summary>
  public event EventHandler<SkStackPanaSessionEventArgs>? PanaSessionEstablished;

  /// <summary>
  /// Occurs when a PANA session is terminated.
  /// </summary>
  public event EventHandler<SkStackPanaSessionEventArgs>? PanaSessionTerminated;

  /// <summary>
  /// Occurs when a PANA session is expired.
  /// </summary>
  public event EventHandler<SkStackPanaSessionEventArgs>? PanaSessionExpired;

  internal void RaiseEventPanaSessionEstablished(SkStackEvent baseEvent) => RaiseEventPanaSession(PanaSessionEstablished, baseEvent);
  internal void RaiseEventPanaSessionTerminated(SkStackEvent baseEvent) => RaiseEventPanaSession(PanaSessionTerminated, baseEvent);
  internal void RaiseEventPanaSessionExpired(SkStackEvent baseEvent) => RaiseEventPanaSession(PanaSessionExpired, baseEvent);

  private void RaiseEventPanaSession(EventHandler<SkStackPanaSessionEventArgs>? ev, SkStackEvent baseEvent)
  {
    if (ev is null)
      return; // return without creating event args if event handler is null

    InvokeEvent(SynchronizingObject, ev, this, new SkStackPanaSessionEventArgs(baseEvent));
  }

  /// <summary>
  /// Occurs when a device enters sleep mode.
  /// </summary>
  /// <seealso cref="SendSKDSLEEPAsync"/>
  public event EventHandler<SkStackEventArgs>? Slept;

  /// <summary>
  /// Occurs when a device returns from sleep mode.
  /// </summary>
  /// <seealso cref="SendSKDSLEEPAsync"/>
  public event EventHandler<SkStackEventArgs>? WokeUp;

  internal void RaiseEventSlept() => RaiseEvent(Slept, default);
  private void RaiseEventWokeUp(SkStackEvent baseEvent) => RaiseEvent(WokeUp, baseEvent);

  private void RaiseEvent(EventHandler<SkStackEventArgs>? ev, SkStackEvent baseEvent)
  {
    if (ev is null)
      return; // return without creating event args if event handler is null

    InvokeEvent(SynchronizingObject, ev, this, new SkStackEventArgs(baseEvent));
  }

  private static void InvokeEvent<TEventArgs>(
    ISynchronizeInvoke? synchronizingObject,
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
