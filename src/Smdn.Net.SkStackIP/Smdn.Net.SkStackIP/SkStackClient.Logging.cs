// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP.Protocol;
using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    internal static readonly EventId EventIdReceivingStatus = new EventId(1, "receiving status");
    internal static readonly EventId EventIdCommandSequence = new EventId(2, "sent command sequence");
    internal static readonly EventId EventIdResponseSequence = new EventId(3, "received response sequence");

    internal static readonly EventId EventIdIPEventReceived = new EventId(6, "IP event received");
    internal static readonly EventId EventIdPanaEventReceived = new EventId(7, "PANA event received");
    internal static readonly EventId EventIdAribStdT108EventReceived = new EventId(8, "ARIB STD-T108 event received");
  }

  internal static class SkStackClientLoggerExtensions {
    private const string prefixCommand = "↦ ";
    private const string prefixResponse = "↤ ";
    private const string prefixEchoback = "↩ ";

    public static bool IsCommandEnabled(this ILogger logger)
    {
      const LogLevel level = LogLevel.Trace;

      return logger.IsEnabled(level);
    }

    public static void LogReceivingStatus(this ILogger logger, string prefix, ReadOnlyMemory<byte> command, Exception exception = null)
    {
      var level = exception is null ? LogLevel.Debug : LogLevel.Error;

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
      var level = exception is null ? LogLevel.Debug : LogLevel.Error;

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
      var level = exception is null ? LogLevel.Debug : LogLevel.Error;

      if (!logger.IsEnabled(level))
        return;

      logger.Log(
        level,
        SkStackClient.EventIdReceivingStatus,
        exception,
        message
      );
    }

    public static void LogTraceCommand(this ILogger logger, ReadOnlyMemory<byte> sequence)
    {
      const LogLevel level = LogLevel.Trace;

      if (!logger.IsEnabled(level))
        return;

      logger.Log(
        level,
        SkStackClient.EventIdCommandSequence,
        CreateLogMessage(prefixCommand, sequence)
      );
    }

    public static readonly object EchobackLineMarker = new object();

    public static void LogTraceResponse(this ILogger logger, ReadOnlySequence<byte> sequence, object marker)
    {
      const LogLevel level = LogLevel.Trace;

      if (!logger.IsEnabled(level))
        return;

      logger.Log(
        level,
        SkStackClient.EventIdResponseSequence,
        CreateLogMessage(Object.ReferenceEquals(marker, EchobackLineMarker) ? prefixEchoback : prefixResponse, sequence)
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

    public static void LogInfoIPEventReceived(this ILogger logger, SkStackUdpReceiveEvent erxudp)
    {
      const LogLevel level = LogLevel.Information;

      if (!logger.IsEnabled(level))
        return;

      var prefix = erxudp.LocalEndPoint.Port switch {
        SkStackUdpPort.PortEchonetLite => "ECHONET Lite/IPv6",
        SkStackUdpPort.PortPana => "PANA/IPv6",
        _ => "IPv6",
      };

      logger.Log(
        level,
        SkStackClient.EventIdIPEventReceived,
        $"{prefix}: {erxudp.LocalEndPoint}←{erxudp.RemoteEndPoint} {erxudp.RemoteLinkLocalAddress} (secured: {erxudp.IsSecured}, length: {erxudp.Data.Length})",
        erxudp
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
}