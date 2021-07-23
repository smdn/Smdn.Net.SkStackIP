// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    private SkStackERXUDPDataFormat erxudpDataFormat = SkStackERXUDPDataFormat.Raw; // RAW as default
    public SkStackERXUDPDataFormat ERXUDPDataFormat {
      get { return erxudpDataFormat; }
      set {
#if NET5_0_OR_GREATER
        if (!Enum.IsDefined(value))
#else
        if (!Enum.IsDefined(typeof(SkStackERXUDPDataFormat), value))
#endif
          throw new ArgumentException($"undefined value of {nameof(SkStackERXUDPDataFormat)}", nameof(ERXUDPDataFormat));

        erxudpDataFormat = value;
      }
    }

    private static readonly ValueTask<bool> TrueResultValueTask =
#if NET5_0_OR_GREATER
      ValueTask.FromResult(true);
#else
      new ValueTask<bool>(result: true);
#endif

    private static readonly ValueTask<bool> FalseResultValueTask =
#if NET5_0_OR_GREATER
      ValueTask.FromResult(false);
#else
      new ValueTask<bool>(result: false);
#endif

    private delegate bool ProcessNotificationalEventsFunc(ISkStackSequenceParserContext context);

    /// <summary>Handles events that are not triggered by commands, or non-response notifications, especially ERXUDP/EVENT.</summary>
    /// <returns>true if the first event processed and consumed, otherwise false.</returns>
    private ValueTask<bool> ProcessNotificationalEventsAsync(
      ISkStackSequenceParserContext context
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

      static bool IsNotificationalEvent(SkStackEventNumber eventNumber)
        => eventNumber switch {
          SkStackEventNumber.NeighborSolicitationReceived => true,
          SkStackEventNumber.NeighborAdvertisementReceived => true,
          SkStackEventNumber.EchoRequestReceived => true,
          SkStackEventNumber.UdpSendCompleted => true,
          SkStackEventNumber.PanaSessionTerminationRequestReceived => true,
          SkStackEventNumber.PanaSessionExpired => true,
          SkStackEventNumber.TransmissionTimeControlLimitationActivated => true,
          SkStackEventNumber.TransmissionTimeControlLimitationDeactivated => true,
          _ => false,
        };

      if (SkStackEventParser.TryExpectEVENT(context, IsNotificationalEvent, out var ev)) {
        switch (ev.Number) {
          case SkStackEventNumber.NeighborSolicitationReceived:
          case SkStackEventNumber.NeighborAdvertisementReceived:
          case SkStackEventNumber.EchoRequestReceived:
          case SkStackEventNumber.UdpSendCompleted:
            logger?.LogInfoIPEventReceived(ev);
            break;

          case SkStackEventNumber.PanaSessionTerminationRequestReceived:
          case SkStackEventNumber.PanaSessionExpired:
            logger?.LogInfoPanaEventReceived(ev);
            // TODO: dispose session
            break;

          case SkStackEventNumber.TransmissionTimeControlLimitationActivated:
          case SkStackEventNumber.TransmissionTimeControlLimitationDeactivated:
            logger?.LogInfoAribStdT108EventReceived(ev);
            // TODO: raise event
            break;
        }

        context.Continue();
        return TrueResultValueTask;
      }

      if (SkStackEventParser.TryExpectERXUDP(context, erxudpDataFormat, out var erxudp, out var erxudpData, out var erxudpDataLength)) {
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

      return FalseResultValueTask;
    }
  }
}