// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel; // ReadOnlyDictionary
using System.Net;
using System.Net.NetworkInformation;

using Smdn.Buffers;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP.Protocol {
  internal static class SkStackEventParser {
    /// <remarks>reference: BP35A1コマンドリファレンス 4.1. ERXUDP</remarks>
    public static OperationStatus TryExpectERXUDP(
      ISkStackSequenceParserContext context,
      SkStackERXUDPDataFormat erxudpDataFormat,
      out SkStackUdpReceiveEvent erxudp,
      out ReadOnlySequence<byte> erxudpData,
      out int erxudpDataLength
    )
    {
      erxudp = default;
      erxudpData = default;
      erxudpDataLength = default;

      var reader = context.CreateReader();
      var status = SkStackTokenParser.TryExpectToken(ref reader, SkStackEventCodeNames.ERXUDP);

      if (status == OperationStatus.NeedMoreData || status == OperationStatus.InvalidData)
        return status;

      if (
        SkStackTokenParser.ExpectIPADDR(ref reader, out var sender) &&
        SkStackTokenParser.ExpectIPADDR(ref reader, out var dest) &&
        SkStackTokenParser.ExpectUINT16(ref reader, out var rport) &&
        SkStackTokenParser.ExpectUINT16(ref reader, out var lport) &&
        SkStackTokenParser.ExpectADDR64(ref reader, out var senderlla) &&
        SkStackTokenParser.ExpectBinary(ref reader, out var secured) &&
        SkStackTokenParser.ExpectUINT16(ref reader, out var datalen)
      ) {
        erxudpDataLength = (int)datalen;

        var lengthOfDataSequence = erxudpDataFormat switch {
          SkStackERXUDPDataFormat.HexAsciiText => erxudpDataLength * 2,
          _ => erxudpDataLength,
        };

        if (reader.GetUnreadSequence().Length < lengthOfDataSequence + 2 /*CRLF*/)
          return OperationStatus.NeedMoreData;

        var erxudpDataStart = reader.Position;

        reader.Advance(lengthOfDataSequence);

        var erxudpDataEnd = reader.Position;

        if (!SkStackTokenParser.ExpectEndOfLine(ref reader))
          return OperationStatus.NeedMoreData;

        erxudp = new(
          sender: sender,
          dest: dest,
          rport: rport,
          lport: lport,
          senderlla: senderlla,
          secured: secured
        );

        erxudpData = reader.Sequence.Slice(erxudpDataStart, erxudpDataEnd);

        context.Complete(reader);
        return OperationStatus.Done;
      }

      return OperationStatus.NeedMoreData;
    }

    /// <remarks>reference: BP35A1コマンドリファレンス 4.2. EPONG</remarks>
    public static bool ExpectEPONG(
      ISkStackSequenceParserContext context
    )
      => throw new NotImplementedException();

    /// <remarks>reference: BP35A1コマンドリファレンス 4.3. EADDR</remarks>
    public static IReadOnlyList<IPAddress> ExpectEADDR(
      ISkStackSequenceParserContext context
    )
    {
      var reader = context.CreateReader();

      if (SkStackTokenParser.TryExpectStatusLine(ref reader, out var status)) {
        // do not consume here
        context.Ignore();
        return status == SkStackResponseStatus.Ok ? Array.Empty<IPAddress>() : null;
      }
      else if (
        SkStackTokenParser.ExpectToken(ref reader, SkStackEventCodeNames.EADDR) &&
        SkStackTokenParser.ExpectEndOfLine(ref reader)
      ) {
        var list = new List<IPAddress>(capacity: 2);

        for (; ; ) {
          var statusLineReader = reader;

          if (SkStackTokenParser.TryExpectStatusLine(ref statusLineReader, out var st)) {
            context.Complete(reader); // do not consume status line here
            return st == SkStackResponseStatus.Ok ? list : null;
          }
          else if (
            SkStackTokenParser.ExpectIPADDR(ref reader, out var address) &&
            SkStackTokenParser.ExpectEndOfLine(ref reader)
          ) {
            list.Add(address);
          }
          else {
            context.SetAsIncomplete();
            return default;
          }
        }
      }

      context.SetAsIncomplete();
      return default;
    }

    private static readonly IReadOnlyDictionary<IPAddress, PhysicalAddress> EmptyNeighborCacheList = new ReadOnlyDictionary<IPAddress, PhysicalAddress>(
      new Dictionary<IPAddress, PhysicalAddress>(capacity: 0)
    );

    /// <remarks>reference: BP35A1コマンドリファレンス 4.4. ENEIGHBOR</remarks>
    public static IReadOnlyDictionary<IPAddress, PhysicalAddress> ExpectENEIGHBOR(
      ISkStackSequenceParserContext context
    )
    {
      var statusLineReader = context.CreateReader();
      var reader = context.CreateReader();

      if (SkStackTokenParser.TryExpectStatusLine(ref statusLineReader, out var status)) {
        // do not consume status line here
        context.Ignore();
        return status == SkStackResponseStatus.Ok ? EmptyNeighborCacheList : null;
      }
      else if (
        SkStackTokenParser.ExpectToken(ref reader, SkStackEventCodeNames.ENEIGHBOR) &&
        SkStackTokenParser.ExpectEndOfLine(ref reader)
      ) {
        const int numberOfNeighborCacheEntry = 8; // 3.18. SKADDNBR
        var neighborCache = new Dictionary<IPAddress, PhysicalAddress>(capacity: numberOfNeighborCacheEntry);

        for (; ; ) {
          statusLineReader = reader;

          if (SkStackTokenParser.TryExpectStatusLine(ref statusLineReader, out var st)) {
            context.Complete(reader); // do not consume status line here
            return st == SkStackResponseStatus.Ok ? neighborCache : null;
          }
          else if (
            SkStackTokenParser.ExpectIPADDR(ref reader, out var ipaddr) &&
            SkStackTokenParser.ExpectADDR64(ref reader, out var addr64) &&
            SkStackTokenParser.ExpectADDR16(ref reader, out var addr16) &&
            SkStackTokenParser.ExpectEndOfLine(ref reader)
          ) {
            neighborCache[ipaddr] = addr64;
          }
          else {
            break;
          }
        }
      }

      context.SetAsIncomplete();
      return default;
    }

    /// <remarks>reference: BP35A1コマンドリファレンス 4.5. EPANDESC</remarks>
    public static bool ExpectEPANDESC(
      ISkStackSequenceParserContext context,
      bool expectPairingId,
      out SkStackPanDescription pandesc
    )
    {
      pandesc = default;

      var reader = context.CreateReader();

      if (
        SkStackTokenParser.ExpectToken(ref reader, SkStackEventCodeNames.EPANDESC) &&
        SkStackTokenParser.ExpectEndOfLine(ref reader)
      ) {
        if (
          SkStackTokenParser.ExpectSequence(ref reader, EPANDESCPrefixChannel) &&
          SkStackTokenParser.ExpectUINT8(ref reader, out var channel) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&

          SkStackTokenParser.ExpectSequence(ref reader, EPANDESCPrefixChannelPage) &&
          SkStackTokenParser.ExpectUINT8(ref reader, out var channelPage) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&

          SkStackTokenParser.ExpectSequence(ref reader, EPANDESCPrefixPanID) &&
          SkStackTokenParser.ExpectUINT16(ref reader, out var panId) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&

          SkStackTokenParser.ExpectSequence(ref reader, EPANDESCPrefixAddress) &&
          SkStackTokenParser.ExpectADDR64(ref reader, out var macAddress) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader) &&

          SkStackTokenParser.ExpectSequence(ref reader, EPANDESCPrefixLQI) &&
          SkStackTokenParser.ExpectUINT8(ref reader, out var lqi) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          uint pairingId = default;

          if (expectPairingId) {
            if (!(
              SkStackTokenParser.ExpectSequence(ref reader, EPANDESCPrefixPairID) &&
              SkStackTokenParser.ExpectUINT32(ref reader, out pairingId) && // instead of CHAR[8]
              SkStackTokenParser.ExpectEndOfLine(ref reader)
            )) {
              context.SetAsIncomplete();
              return false;
            }
          }

          pandesc = new SkStackPanDescription(
            channel: SkStackChannel.FindByChannelNumber(channel),
            channelPage: channelPage,
            panId: panId,
            macAddress: macAddress,
            rssi: SkStackLQI.ToRSSI((int)lqi),
            pairingId: expectPairingId ? pairingId : default
          );

          context.Complete(reader);
          return true;
        }
      }

      context.SetAsIncomplete();
      return false;
    }

    private static readonly ReadOnlyMemory<byte> EPANDESCPrefixChannel      = SkStack.ToByteSequence("  Channel:");
    private static readonly ReadOnlyMemory<byte> EPANDESCPrefixChannelPage  = SkStack.ToByteSequence("  Channel Page:");
    private static readonly ReadOnlyMemory<byte> EPANDESCPrefixPanID        = SkStack.ToByteSequence("  Pan ID:");
    private static readonly ReadOnlyMemory<byte> EPANDESCPrefixAddress      = SkStack.ToByteSequence("  Addr:");
    private static readonly ReadOnlyMemory<byte> EPANDESCPrefixLQI          = SkStack.ToByteSequence("  LQI:");
    private static readonly ReadOnlyMemory<byte> EPANDESCPrefixPairID       = SkStack.ToByteSequence("  PairID:");

    /// <remarks>reference: BP35A1コマンドリファレンス 4.6. EEDSCAN</remarks>
    public static bool ExpectEEDSCAN(
      ISkStackSequenceParserContext context,
      out IReadOnlyDictionary<SkStackChannel, double> result
    )
    {
      result = default;

      var reader = context.CreateReader();

      if (
        SkStackTokenParser.ExpectToken(ref reader, SkStackEventCodeNames.EEDSCAN) &&
        SkStackTokenParser.ExpectEndOfLine(ref reader)
      ) {
        var ret = new Dictionary<SkStackChannel, double>(SkStackChannel.Channels.Count);

        result = ret;

        for (var i = 0; i < SkStackChannel.Channels.Count; i++) {
          if (
            SkStackTokenParser.ExpectUINT8(ref reader, out var channel) &&
            SkStackTokenParser.ExpectUINT8(ref reader, out var lqi)
          ) {
            ret[SkStackChannel.FindByChannelNumber(channel)] = SkStackLQI.ToRSSI((int)lqi);
          }
          else {
            context.SetAsIncomplete();
            return false;
          }
        }

        if (SkStackTokenParser.ExpectEndOfLine(ref reader)) {
          // [VER 1.2.10, APPVER rev26e] EEDSCAN responds extra CRLF
          if (SkStackTokenParser.ExpectSequence(ref reader, SkStack.CRLFMemory))
            SkStackTokenParser.ExpectEndOfLine(ref reader);

          context.Complete(reader);
          return true;
        }
      }

      context.SetAsIncomplete();
      return false;
    }

    /// <remarks>reference: BP35A1コマンドリファレンス 4.7. EPORT</remarks>
    public static IReadOnlyList<SkStackUdpPort> ExpectEPORT(
      ISkStackSequenceParserContext context
    )
    {
      var statusLineReader = context.CreateReader();
      var reader = context.CreateReader();

      if (SkStackTokenParser.TryExpectStatusLine(ref statusLineReader, out var status)) {
        // do not consume status line here
        context.Ignore();
        return status == SkStackResponseStatus.Ok ? Array.Empty<SkStackUdpPort>() : null;
      }
      else if (
        SkStackTokenParser.ExpectToken(ref reader, SkStackEventCodeNames.EPORT) &&
        SkStackTokenParser.ExpectEndOfLine(ref reader)
      ) {
        var ports = new SkStackUdpPort[SkStackUdpPort.NumberOfPorts];

        for (var i = 0; i < SkStackUdpPort.NumberOfPorts; i++) {
          if (
            SkStackTokenParser.ExpectDecimalNumber(ref reader, out var port) &&
            SkStackTokenParser.ExpectEndOfLine(ref reader)
          ) {
            ports[i] = new SkStackUdpPort(
              handle: (SkStackUdpPortHandle)((int)SkStackUdpPort.HandleMin + i),
              port: (int)port
            );
          }
          else {
            break; // incomplete
          }
        }

        for (; ; ) {
          statusLineReader = reader;

          if (SkStackTokenParser.TryExpectStatusLine(ref statusLineReader, out var st)) {
            context.Complete(reader); // do not consume status line here
            return st == SkStackResponseStatus.Ok ? ports : null;
          }

          // [VER 1.2.10, APPVER rev26e] EPORT responds extra CRLF and PORT_UDPs?
          // "EPORT␍␊3610␍␊716␍␊0␍␊0␍␊0␍␊0␍␊␍␊3610␍␊0␍␊0␍␊0␍␊OK␍␊"

          if (reader.TryReadTo(out ReadOnlySequence<byte> _, SkStack.CRLFSpan, advancePastDelimiter: true))
            continue; // ignore extra lines
          else
            break; // incomplete
        }
      }

      context.SetAsIncomplete();
      return default;
    }

    /// <remarks>reference: BP35A1コマンドリファレンス 4.8. EVENT</remarks>
    public static OperationStatus TryExpectEVENT(
      ISkStackSequenceParserContext context,
      out SkStackEvent ev
    )
    {
      ev = default;

      var reader = context.CreateReader();
      var status = SkStackTokenParser.TryExpectToken(ref reader, SkStackEventCodeNames.EVENT);

      if (status == OperationStatus.NeedMoreData || status == OperationStatus.InvalidData)
        return status;

      if (SkStackTokenParser.ExpectUINT8(ref reader, out var num)) {
        var number = (SkStackEventNumber)num;

        IPAddress sender = default;
        int parameter = default;
        SkStackEventCode expectedSubsequentEventCode = default;

        // C0 does not define <IPADDR> and <PARAM>
        if (number != SkStackEventNumber.WakeupSignalReceived) {
          if (!SkStackTokenParser.ExpectIPADDR(ref reader, out sender))
            return OperationStatus.NeedMoreData;

          switch (number) {
            case SkStackEventNumber.EnergyDetectScanCompleted:
              expectedSubsequentEventCode = SkStackEventCode.EEDSCAN;
              break;
            case SkStackEventNumber.BeaconReceived:
              expectedSubsequentEventCode = SkStackEventCode.EPANDESC;
              break;
            case SkStackEventNumber.UdpSendCompleted:
              if (!SkStackTokenParser.ExpectUINT8(ref reader, out var param))
                return OperationStatus.NeedMoreData;

              parameter = (int)param;
              break;
          }
        }

        if (SkStackTokenParser.ExpectEndOfLine(ref reader)) {
          ev = new SkStackEvent(number, sender, (int)parameter, expectedSubsequentEventCode);
          context.Complete(reader);
          return OperationStatus.Done;
        }
      }

      return OperationStatus.NeedMoreData;
    }
  }
}