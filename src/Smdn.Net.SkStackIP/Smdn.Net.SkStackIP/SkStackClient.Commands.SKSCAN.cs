// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    /// <summary>`SKSCAN 0`</summary>
    /// <remarks>reference: BP35A1コマンドリファレンス 3.9. SKSCAN</remarks>
    [CLSCompliant(false)]
    public ValueTask<(
      SkStackResponse Response,
      IReadOnlyDictionary<SkStackChannel, double> ScanResult
    )> SendSKSCANEnergyDetectScanAsync(
      TimeSpan duration = default,
      uint channelMask = SKSCANDefaultChannelMask,
      CancellationToken cancellationToken = default
    )
      => SendSKSCANAsyncCore(
        mode: SKSCANMode.EnergyDetectScan,
        channelMask: channelMask,
        durationFactor: TranslateToSKSCANDurationFactorOrThrowIfOutOfRange(duration == default ? SKSCANDefaultDuration : duration, nameof(duration)),
        parseEvent: ParseSKSCANEnergyDetectScanEvent,
        cancellationToken: cancellationToken
      );

    /// <summary>`SKSCAN 0`</summary>
    /// <remarks>reference: BP35A1コマンドリファレンス 3.9. SKSCAN</remarks>
    [CLSCompliant(false)]
    public ValueTask<(
      SkStackResponse Response,
      IReadOnlyDictionary<SkStackChannel, double> ScanResult
    )> SendSKSCANEnergyDetectScanAsync(
      int durationFactor,
      uint channelMask = SKSCANDefaultChannelMask,
      CancellationToken cancellationToken = default
    )
      => SendSKSCANAsyncCore(
        mode: SKSCANMode.EnergyDetectScan,
        channelMask: channelMask,
        durationFactor: ThrowIfDurationFactorOutOfRange(durationFactor, nameof(durationFactor)),
        parseEvent: ParseSKSCANEnergyDetectScanEvent,
        cancellationToken: cancellationToken
      );

    private static IReadOnlyDictionary<SkStackChannel, double> ParseSKSCANEnergyDetectScanEvent(
      ISkStackSequenceParserContext context
    )
    {
      var reader = context.CreateReader(); // retain current buffer

      if (SkStackEventParser.TryExpectEVENT(context, SkStackEventNumber.EnergyDetectScanCompleted, out var ev)) {
        SkStackUnexpectedResponseException.ThrowIfUnexpectedSubsequentEventCode(ev, expectedEventCode: SkStackEventCode.EEDSCAN);

        if (SkStackEventParser.ExpectEEDSCAN(context, out var result)) {
          context.Complete();
          return result;
        }
      }

      context.SetAsIncomplete(reader); // revert buffer
      return default;
    }

    /// <summary>`SKSCAN 2`</summary>
    /// <remarks>reference: BP35A1コマンドリファレンス 3.9. SKSCAN</remarks>
    [CLSCompliant(false)]
    public ValueTask<(
      SkStackResponse Response,
      IReadOnlyList<SkStackPanDescription> PanDescriptions
    )> SendSKSCANActiveScanPairAsync(
      TimeSpan duration = default,
      uint channelMask = SKSCANDefaultChannelMask,
      CancellationToken cancellationToken = default
    )
      => SendSKSCANAsyncCore(
        mode: SKSCANMode.ActiveScanPair,
        channelMask: channelMask,
        durationFactor: TranslateToSKSCANDurationFactorOrThrowIfOutOfRange(duration == default ? SKSCANDefaultDuration : duration, nameof(duration)),
        parseEvent: static context => ParseSKSCANActiveScanEvent(context, expectPairingId: true),
        cancellationToken: cancellationToken
      );

    /// <summary>`SKSCAN 2`</summary>
    /// <remarks>reference: BP35A1コマンドリファレンス 3.9. SKSCAN</remarks>
    [CLSCompliant(false)]
    public ValueTask<(
      SkStackResponse Response,
      IReadOnlyList<SkStackPanDescription> PanDescriptions
    )> SendSKSCANActiveScanPairAsync(
      int durationFactor,
      uint channelMask = SKSCANDefaultChannelMask,
      CancellationToken cancellationToken = default
    )
      => SendSKSCANAsyncCore(
        mode: SKSCANMode.ActiveScanPair,
        channelMask: channelMask,
        durationFactor: ThrowIfDurationFactorOutOfRange(durationFactor, nameof(durationFactor)),
        parseEvent: static context => ParseSKSCANActiveScanEvent(context, expectPairingId: true),
        cancellationToken: cancellationToken
      );

    /// <summary>`SKSCAN 3`</summary>
    /// <remarks>reference: BP35A1コマンドリファレンス 3.9. SKSCAN</remarks>
    [CLSCompliant(false)]
    public ValueTask<(
      SkStackResponse Response,
      IReadOnlyList<SkStackPanDescription> PanDescriptions
    )>
    SendSKSCANActiveScanAsync(
      TimeSpan duration = default,
      uint channelMask = SKSCANDefaultChannelMask,
      CancellationToken cancellationToken = default
    )
      => SendSKSCANAsyncCore(
        mode: SKSCANMode.ActiveScan,
        channelMask: channelMask,
        durationFactor: TranslateToSKSCANDurationFactorOrThrowIfOutOfRange(duration == default ? SKSCANDefaultDuration : duration, nameof(duration)),
        parseEvent: static context => ParseSKSCANActiveScanEvent(context, expectPairingId: false),
        cancellationToken: cancellationToken
      );

    /// <summary>`SKSCAN 3`</summary>
    /// <remarks>reference: BP35A1コマンドリファレンス 3.9. SKSCAN</remarks>
    [CLSCompliant(false)]
    public ValueTask<(
      SkStackResponse Response,
      IReadOnlyList<SkStackPanDescription> PanDescriptions
    )>
    SendSKSCANActiveScanAsync(
      int durationFactor,
      uint channelMask = SKSCANDefaultChannelMask,
      CancellationToken cancellationToken = default
    )
      => SendSKSCANAsyncCore(
        mode: SKSCANMode.ActiveScan,
        channelMask: channelMask,
        durationFactor: ThrowIfDurationFactorOutOfRange(durationFactor, nameof(durationFactor)),
        parseEvent: static context => ParseSKSCANActiveScanEvent(context, expectPairingId: false),
        cancellationToken: cancellationToken
      );

    private static IReadOnlyList<SkStackPanDescription> ParseSKSCANActiveScanEvent(
      ISkStackSequenceParserContext context,
      bool expectPairingId
    )
    {
      const int expectedMaxPanDescriptionCount = 1;

      var incompleteReader = context.CreateReader(); // retain current buffer

      if (SkStackEventParser.TryExpectEVENT(context, SkStackEventNumber.BeaconReceived, out var ev20)) {
        SkStackUnexpectedResponseException.ThrowIfUnexpectedSubsequentEventCode(ev20, SkStackEventCode.EPANDESC);

        if (SkStackEventParser.ExpectEPANDESC(context, expectPairingId, out var pandesc)) {
          var result = context.GetOrCreateState(static () => new List<SkStackPanDescription>(capacity: expectedMaxPanDescriptionCount));

          result.Add(pandesc);

          context.Continue();
          return default;
        }

        context.SetAsIncomplete(incompleteReader); // revert buffer
        return default;
      }

      if (SkStackEventParser.TryExpectEVENT(context, SkStackEventNumber.ActiveScanCompleted, out var ev22)) {
        context.Complete();
        return (IReadOnlyList<SkStackPanDescription>)context.State ?? Array.Empty<SkStackPanDescription>();
      }

      context.SetAsIncomplete(incompleteReader); // revert buffer
      return default;
    }

    private enum SKSCANMode : byte {
      EnergyDetectScan = 0,
      ActiveScanPair = 2,
      ActiveScan = 3,
    }

    private const uint SKSCANDefaultChannelMask = 0xFFFFFFFF;

    private const byte SKSCANMinDurationFactor = 0x0; // 0
    private const byte SKSCANMaxDurationFactor = 0xE; // 14
    private const byte SKSCANDefaultDurationFactor = 0x2; // 2

    private static byte ThrowIfDurationFactorOutOfRange(int durationFactor, string paramName)
    {
      if (!(SKSCANMinDurationFactor <= durationFactor && durationFactor <= SKSCANMaxDurationFactor))
        throw new ArgumentOutOfRangeException(paramName, durationFactor, $"must be in range of {SKSCANMinDurationFactor}~{SKSCANMaxDurationFactor}");

      return (byte)durationFactor;
    }

    private static TimeSpan ToSKSCANDuration(int factor)
      => TimeSpan.FromMilliseconds(9.6) * (Math.Pow(2.0, factor) + 1);

    public static readonly TimeSpan SKSCANMinDuration = ToSKSCANDuration(SKSCANMinDurationFactor);
    public static readonly TimeSpan SKSCANMaxDuration = ToSKSCANDuration(SKSCANMaxDurationFactor);
    public static readonly TimeSpan SKSCANDefaultDuration = ToSKSCANDuration(SKSCANDefaultDurationFactor);

    private static byte TranslateToSKSCANDurationFactorOrThrowIfOutOfRange(TimeSpan duration, string paramName)
    {
      if (SKSCANMinDuration <= duration && duration <= SKSCANMaxDuration) {
        for (byte durationFactor = SKSCANMinDurationFactor + 1; durationFactor <= SKSCANMaxDurationFactor; durationFactor++) {
          if (duration < ToSKSCANDuration(durationFactor))
            return (byte)(durationFactor - 1);
        }

        return SKSCANMaxDurationFactor;
      }

      throw new ArgumentOutOfRangeException(paramName, duration, $"must be in range of {SKSCANMinDuration}~{SKSCANMaxDuration}");
    }

    private async ValueTask<(
      SkStackResponse Response,
      TScanResult ScanResult
    )> SendSKSCANAsyncCore<TScanResult>(
      SKSCANMode mode,
      uint channelMask,
      byte durationFactor,
      SkStackSequenceParser<TScanResult> parseEvent,
      CancellationToken cancellationToken
    )
    {
      SkStackResponse resp = default;
      byte[] CHANNEL_MASK = default;

      try {
        CHANNEL_MASK = ArrayPool<byte>.Shared.Rent(8);

        SkStackCommandArgs.TryConvertToUINT32(CHANNEL_MASK, channelMask, out var lengthChannelMask, zeroPadding: true);

        resp = await SendCommandAsync(
          command: SkStackCommandNames.SKSCAN,
          arguments: SkStackCommandArgs.CreateEnumerable(
            SkStackCommandArgs.GetHex((byte)mode),
            CHANNEL_MASK.AsMemory(0, lengthChannelMask),
            SkStackCommandArgs.GetHex((byte)durationFactor)
          ),
          cancellationToken: cancellationToken,
          throwIfErrorStatus: true
        ).ConfigureAwait(false);
      }
      finally {
        if (CHANNEL_MASK is not null)
          ArrayPool<byte>.Shared.Return(CHANNEL_MASK);
      }

      return (
        Response: resp,
        ScanResult: await ReceiveEventAsync(
          cancellationToken: cancellationToken,
          parseEvent: parseEvent
        ).ConfigureAwait(false)
      );
    }
  }
}