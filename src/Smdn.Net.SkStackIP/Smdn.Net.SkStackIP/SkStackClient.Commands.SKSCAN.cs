// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  private abstract class SKSCANEventHandler<TScanResult> : SkStackEventHandlerBase {
    public TScanResult ScanResult { get; protected set; }

    public abstract override bool TryProcessEvent(SkStackEvent ev);
    public abstract override void ProcessSubsequentEvent(ISkStackSequenceParserContext context);
  }

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
      commandEventHandler: new SKSCANEnergyDetectScanEventHandler(),
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
      commandEventHandler: new SKSCANEnergyDetectScanEventHandler(),
      cancellationToken: cancellationToken
    );

  private class SKSCANEnergyDetectScanEventHandler : SKSCANEventHandler<IReadOnlyDictionary<SkStackChannel, double>> {
    public override bool TryProcessEvent(SkStackEvent ev)
    {
      if (ev.Number == SkStackEventNumber.EnergyDetectScanCompleted)
        return false; // process subsequent event

      return false;
    }

    public override void ProcessSubsequentEvent(ISkStackSequenceParserContext context)
    {
      var reader = context.CreateReader(); // retain current buffer

      if (SkStackEventParser.ExpectEEDSCAN(context, out var result)) {
        ScanResult = result;
        context.Complete();
      }
      else {
        context.SetAsIncomplete(reader); // revert buffer
      }
    }
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
      commandEventHandler: new SKSCANActiveScanEventHandler(expectPairingId: true),
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
      commandEventHandler: new SKSCANActiveScanEventHandler(expectPairingId: true),
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
      commandEventHandler: new SKSCANActiveScanEventHandler(expectPairingId: false),
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
      commandEventHandler: new SKSCANActiveScanEventHandler(expectPairingId: false),
      cancellationToken: cancellationToken
    );

  private class SKSCANActiveScanEventHandler : SKSCANEventHandler<IReadOnlyList<SkStackPanDescription>> {
    private readonly bool expectPairingId;

    private const int expectedMaxPanDescriptionCount = 1;
    private List<SkStackPanDescription> scanResult = null;

    public SKSCANActiveScanEventHandler(bool expectPairingId)
    {
      this.expectPairingId = expectPairingId;
    }

    public override bool TryProcessEvent(SkStackEvent ev)
    {
      switch (ev.Number) {
        case SkStackEventNumber.BeaconReceived:
          return false; // process subsequent event

        case SkStackEventNumber.ActiveScanCompleted:
          ScanResult = (IReadOnlyList<SkStackPanDescription>)scanResult ?? Array.Empty<SkStackPanDescription>();
          return true; // completed

        default:
          return false;
      }
    }

    public override void ProcessSubsequentEvent(ISkStackSequenceParserContext context)
    {
      var reader = context.CreateReader(); // retain current buffer

      if (SkStackEventParser.ExpectEPANDESC(context, expectPairingId, out var pandesc)) {
        scanResult ??= new(capacity: expectedMaxPanDescriptionCount);
        scanResult.Add(pandesc);
        context.Continue();
      }
      else {
        context.SetAsIncomplete(reader); // revert buffer
      }
    }
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
    => durationFactor is >= SKSCANMinDurationFactor and <= SKSCANMaxDurationFactor
      ? (byte)durationFactor
      : throw new ArgumentOutOfRangeException(paramName, durationFactor, $"must be in range of {SKSCANMinDurationFactor}~{SKSCANMaxDurationFactor}");

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
    SKSCANEventHandler<TScanResult> commandEventHandler,
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
          SkStackCommandArgs.GetHex(durationFactor)
        ),
        commandEventHandler: commandEventHandler,
        throwIfErrorStatus: true,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    finally {
      if (CHANNEL_MASK is not null)
        ArrayPool<byte>.Shared.Return(CHANNEL_MASK);
    }

    return (resp, commandEventHandler.ScanResult);
  }
}
