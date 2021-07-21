// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    private delegate bool ProcessNotificationalEventsFunc(ISkStackSequenceParserContext context);

    /// <summary>Handles events that are not triggered by commands, or non-response notifications, especially ERXUDP/EVENT.</summary>
    /// <returns>true if the first event processed and consumed, otherwise false.</returns>
    private bool ProcessNotificationalEvents(
      ISkStackSequenceParserContext context
    )
    {
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
        return true;
      }

      if (SkStackEventParser.TryExpectERXUDP(context, out var erxudp, out var erxudpData)) {
        logger?.LogInfoIPEventReceived(erxudp, erxudpData);
        // TODO: copy to buffer
        context.Continue();
        return true;
      }

      context.Ignore();
      return false;
    }
  }
}