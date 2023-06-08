// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP.Protocol;
using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  internal static readonly EventId EventIdReceivingStatus = new(1, "receiving status");
  internal static readonly EventId EventIdCommandSequence = new(2, "sent command sequence");
  internal static readonly EventId EventIdResponseSequence = new(3, "received response sequence");

  internal static readonly EventId EventIdIPEventReceived = new(6, "IP event received");
  internal static readonly EventId EventIdPanaEventReceived = new(7, "PANA event received");
  internal static readonly EventId EventIdAribStdT108EventReceived = new(8, "ARIB STD-T108 event received");
}

internal static class SkStackClientLoggerExtensions {
  private const string prefixCommand = "↦ ";
  private const string prefixResponse = "↤ ";
  private const string prefixEchoback = "↩ ";

  private const LogLevel logLevelReceivingStatusDefault = LogLevel.Trace;

  public static void LogReceivingStatus(this ILogger logger, string prefix, ReadOnlyMemory<byte> command, Exception exception = null)
  {
    var level = exception is null ? logLevelReceivingStatusDefault : LogLevel.Error;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdReceivingStatus,
      exception,
      CreateLogMessage(prefix, command)
    );
  }

  public static void LogReceivingStatus(this ILogger logger, string prefix, ReadOnlySequence<byte> sequence, Exception exception = null)
  {
    var level = exception is null ? logLevelReceivingStatusDefault : LogLevel.Error;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdReceivingStatus,
      exception,
      CreateLogMessage(prefix, sequence)
    );
  }

  public static void LogReceivingStatus(this ILogger logger, string message, Exception exception = null)
  {
    var level = exception is null ? logLevelReceivingStatusDefault : LogLevel.Error;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdReceivingStatus,
      exception,
      message
    );
  }

  private const LogLevel logLevelCommand = LogLevel.Debug;

  public static bool IsCommandLoggingEnabled(this ILogger logger)
    => logger.IsEnabled(logLevelCommand);

  public static void LogDebugCommand(this ILogger logger, ReadOnlyMemory<byte> sequence)
  {
    if (!logger.IsEnabled(logLevelCommand))
      return;

    logger.Log(
      logLevelCommand,
      SkStackClient.EventIdCommandSequence,
      CreateLogMessage(prefixCommand, sequence)
    );
  }

  public static readonly object EchobackLineMarker = new object();

  private const LogLevel logLevelResponse = LogLevel.Debug;

  public static void LogDebugResponse(this ILogger logger, ReadOnlySequence<byte> sequence, object marker)
  {
    if (!logger.IsEnabled(logLevelResponse))
      return;

    logger.Log(
      logLevelResponse,
      SkStackClient.EventIdResponseSequence,
      CreateLogMessage(ReferenceEquals(marker, EchobackLineMarker) ? prefixEchoback : prefixResponse, sequence)
    );
  }

  internal static string CreateLogMessage(string prefix, ReadOnlyMemory<byte> sequence)
    => string.Create(
      length: prefix.Length + sequence.Length,
      state: (pfx: prefix, seq: sequence),
      action: static (span, arg) => {
        // copy prefix
        arg.pfx.AsSpan().CopyTo(span);

        // copy sequence
        arg.seq.Span.TryPicturizeControlChars(span.Slice(arg.pfx.Length));
      }
    );

  internal static string CreateLogMessage(string prefix, ReadOnlySequence<byte> sequence)
    => string.Create(
      length: (int)Math.Min(int.MaxValue, prefix.Length + sequence.Length),
      state: (pfx: prefix, seq: sequence),
      action: static (span, arg) => {
        // copy prefix
        arg.pfx.AsSpan().CopyTo(span);

        // copy sequence
        arg.seq.TryPicturizeControlChars(span.Slice(arg.pfx.Length));
      }
    );

  public static void LogInfoIPEventReceived(this ILogger logger, SkStackEvent ev)
  {
    const LogLevel level = LogLevel.Information;

    if (!logger.IsEnabled(level))
      return;

    const string param0 = "Successful";
    const string param1 = "Failed";
    const string param2 = "Neighbor Solicitation";

    logger.Log(
      level,
      SkStackClient.EventIdIPEventReceived,
      ev.Number == SkStackEventNumber.UdpSendCompleted
        ? $"IPv6: {ev.Number} - {ev.Parameter switch { 0 => param0, 1 => param1, 2 => param2, _ => "Unknown" }} (EVENT {(byte)ev.Number:X2}, PARAM {ev.Parameter}, {ev.SenderAddress})"
        : $"IPv6: {ev.Number} (EVENT {(byte)ev.Number:X2}, {ev.SenderAddress})",
      ev
    );
  }

  public static void LogInfoIPEventReceived(this ILogger logger, SkStackUdpReceiveEvent erxudp, ReadOnlySequence<byte> erxudpData)
  {
    const LogLevel level = LogLevel.Information;

    if (!logger.IsEnabled(level))
      return;

    var prefix = erxudp.LocalEndPoint.Port switch {
      SkStackKnownPortNumbers.EchonetLite => "ECHONET Lite/IPv6",
      SkStackKnownPortNumbers.Pana => "PANA/IPv6",
      _ => "IPv6",
    };

    logger.Log(
      level,
      SkStackClient.EventIdIPEventReceived,
      $"{prefix}: {erxudp.LocalEndPoint}←{erxudp.RemoteEndPoint} {erxudp.RemoteLinkLocalAddress} (secured: {erxudp.IsSecured}, length: {erxudpData.Length})",
      erxudpData
    );
  }

  public static void LogInfoPanaEventReceived(this ILogger logger, SkStackEvent ev)
  {
    const LogLevel level = LogLevel.Information;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdPanaEventReceived,
       $"PANA: {ev.Number} (EVENT {(byte)ev.Number:X2}, {ev.SenderAddress})",
       ev
    );
  }

  public static void LogInfoAribStdT108EventReceived(this ILogger logger, SkStackEvent ev)
  {
    const LogLevel level = LogLevel.Information;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdAribStdT108EventReceived,
       $"ARIB STD-T108: {ev.Number} (EVENT {(byte)ev.Number:X2}, {ev.SenderAddress})",
       ev
    );
  }
}
