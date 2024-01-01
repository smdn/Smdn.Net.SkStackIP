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
  private const string PrefixCommand = "↦ ";
  private const string PrefixResponse = "↤ ";
  private const string PrefixEchoback = "↩ ";

  private const LogLevel LogLevelReceivingStatusDefault = LogLevel.Trace;

  public static void LogReceivingStatus(this ILogger logger, string prefix, ReadOnlyMemory<byte> command, Exception? exception = null)
  {
    var level = exception is null ? LogLevelReceivingStatusDefault : LogLevel.Error;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdReceivingStatus,
      exception,
      "{Prefix}{Command}",
      prefix,
      command.Span.ToControlCharsPicturizedString()
    );
  }

  public static void LogReceivingStatus(this ILogger logger, string prefix, ReadOnlySequence<byte> sequence, Exception? exception = null)
  {
    var level = exception is null ? LogLevelReceivingStatusDefault : LogLevel.Error;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdReceivingStatus,
      exception,
      "{Prefix}{Sequence}",
      prefix,
      sequence.ToControlCharsPicturizedString()
    );
  }

  public static void LogReceivingStatus(this ILogger logger, string message, Exception? exception = null)
  {
    var level = exception is null ? LogLevelReceivingStatusDefault : LogLevel.Error;

    if (!logger.IsEnabled(level))
      return;

    logger.Log(
      level,
      SkStackClient.EventIdReceivingStatus,
      exception,
      "{Message}",
      message
    );
  }

  private const LogLevel LogLevelCommand = LogLevel.Debug;

  public static bool IsCommandLoggingEnabled(this ILogger logger)
    => logger.IsEnabled(LogLevelCommand);

  public static void LogDebugCommand(this ILogger logger, ReadOnlyMemory<byte> sequence)
  {
    if (!logger.IsEnabled(LogLevelCommand))
      return;

    logger.Log(
      LogLevelCommand,
      SkStackClient.EventIdCommandSequence,
      "{Prefix}{Sequence}",
      PrefixCommand,
      sequence.Span.ToControlCharsPicturizedString()
    );
  }

  public static readonly object EchobackLineMarker = new();

  private const LogLevel LogLevelResponse = LogLevel.Debug;

  public static void LogDebugResponse(this ILogger logger, ReadOnlySequence<byte> sequence, object? marker)
  {
    if (!logger.IsEnabled(LogLevelResponse))
      return;

    logger.Log(
      LogLevelResponse,
      SkStackClient.EventIdResponseSequence,
      "{Prefix}{Sequence}",
      ReferenceEquals(marker, EchobackLineMarker) ? PrefixEchoback : PrefixResponse,
      sequence.ToControlCharsPicturizedString()
    );
  }

  public static void LogInfoIPEventReceived(this ILogger logger, SkStackEvent ev)
  {
    const LogLevel Level = LogLevel.Information;

    if (!logger.IsEnabled(Level))
      return;

    if (ev.Number == SkStackEventNumber.UdpSendCompleted) {
      logger.Log(
        Level,
        SkStackClient.EventIdIPEventReceived,
        "IPv6: {Number} - {Parameter} (EVENT {NumberInHex:X2}, PARAM {Parameter}, {SenderAddress})",
        ev.Number,
        ev.Parameter switch {
          0 => "Successful",
          1 => "Failed",
          2 => "Neighbor Solicitation",
          _ => "Unknown",
        },
        (byte)ev.Number,
        ev.Parameter,
        ev.SenderAddress
      );
    }
    else {
      logger.Log(
        Level,
        SkStackClient.EventIdIPEventReceived,
        "IPv6: {Number} (EVENT {NumberInHex:X2}, {SenderAddress})",
        ev.Number,
        (byte)ev.Number,
        ev.SenderAddress
      );
    }
  }

  public static void LogInfoIPEventReceived(this ILogger logger, SkStackUdpReceiveEvent erxudp, ReadOnlySequence<byte> erxudpData)
  {
    const LogLevel Level = LogLevel.Information;

    if (!logger.IsEnabled(Level))
      return;

    logger.Log(
      Level,
      SkStackClient.EventIdIPEventReceived,
      "{Prefix}: {LocalEndPoint}←{RemoteEndPoint} {RemoteLinkLocalAddress} (secured: {IsSecured}, length: {Length})",
      erxudp.LocalEndPoint.Port switch {
        SkStackKnownPortNumbers.EchonetLite => "ECHONET Lite/IPv6",
        SkStackKnownPortNumbers.Pana => "PANA/IPv6",
        _ => "IPv6",
      },
      erxudp.LocalEndPoint,
      erxudp.RemoteEndPoint,
      erxudp.RemoteLinkLocalAddress,
      erxudp.IsSecured,
      erxudpData.Length
    );
  }

  public static void LogInfoPanaEventReceived(this ILogger logger, SkStackEvent ev)
  {
    const LogLevel Level = LogLevel.Information;

    if (!logger.IsEnabled(Level))
      return;

    logger.Log(
      Level,
      SkStackClient.EventIdPanaEventReceived,
      "PANA: {Number} (EVENT {NumberInHex:X2}, {SenderAddress})",
      ev.Number,
      (byte)ev.Number,
      ev.SenderAddress
    );
  }

  public static void LogInfoAribStdT108EventReceived(this ILogger logger, SkStackEvent ev)
  {
    const LogLevel Level = LogLevel.Information;

    if (!logger.IsEnabled(Level))
      return;

    logger.Log(
      Level,
      SkStackClient.EventIdAribStdT108EventReceived,
      "ARIB STD-T108: {Number} (EVENT {NumberInHex:X2}, {SenderAddress})",
      ev.Number,
      (byte)ev.Number,
      ev.SenderAddress
    );
  }
}
