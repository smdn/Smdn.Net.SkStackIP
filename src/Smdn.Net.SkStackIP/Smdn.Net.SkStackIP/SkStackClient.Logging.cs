// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;

using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP.Protocol;
using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  internal static class EventIds {
    public const int ReceivingStatus = 1;
    public const int CommandSequence = 2;
    public const int ResponseSequence = 3;
    public const int IPEventReceived = 6;
    public const int PanaEventReceived = 7;
    public const int AribStdT108EventReceived = 8;
  }

  private const string PrefixCommand = "↦ ";
  private const string PrefixResponse = "↤ ";
  private const string PrefixEchoback = "↩ ";

  private const LogLevel LogLevelReceivingStatus = LogLevel.Trace;
  private const LogLevel LogLevelReceivingStatusUnexpectedResponseException = LogLevel.Error;
  private const LogLevel LogLevelCommand = LogLevel.Debug;
  private const LogLevel LogLevelResponse = LogLevel.Debug;
  private const LogLevel LogLevelIPEventReceived = LogLevel.Information;
  private const LogLevel LogLevelPanaEventReceived = LogLevel.Information;
  private const LogLevel LogLevelAribStdT108EventReceived = LogLevel.Information;

  private static bool IsReceivingStatusLoggingEnabled(ILogger logger)
    => logger.IsEnabled(LogLevelReceivingStatus);

  [LoggerMessage(
    EventId = EventIds.ReceivingStatus,
    Level = LogLevelReceivingStatus,
    Message = "{Message}"
  )]
  private static partial void LogReceivingStatus(
    ILogger logger,
    string message
  );

  private static void LogReceivingStatus(ILogger logger, string prefix, ReadOnlyMemory<byte> command)
#pragma warning disable CA1873
    => LogReceivingStatus(logger, prefix, command.Span.ToControlCharsPicturizedString());
#pragma warning restore CA1873

  private static void LogReceivingStatus(ILogger logger, string prefix, ReadOnlySequence<byte> sequence)
#pragma warning disable CA1873
    => LogReceivingStatus(logger, prefix, sequence.ToControlCharsPicturizedString());
#pragma warning restore CA1873

  [LoggerMessage(
    EventId = EventIds.ReceivingStatus,
    Level = LogLevelReceivingStatus,
    Message = "{Prefix}{Sequence}"
  )]
  private static partial void LogReceivingStatus(
    ILogger logger,
    string prefix,
    string sequence
  );

  private static void LogReceivingStatusUnexpectedResponseException(ILogger logger, ReadOnlySequence<byte> sequence, Exception exception)
#pragma warning disable CA1873
    => LogReceivingStatusUnexpectedResponseException(logger, exception, sequence.ToControlCharsPicturizedString());
#pragma warning restore CA1873

  [LoggerMessage(
    EventId = EventIds.ReceivingStatus,
    Level = LogLevelReceivingStatusUnexpectedResponseException,
    Message = "Unexpected response: {Sequence}"
  )]
  private static partial void LogReceivingStatusUnexpectedResponseException(
    ILogger logger,
    Exception exception,
    string sequence
  );

  private static bool IsCommandLoggingEnabled(ILogger logger)
    => logger.IsEnabled(LogLevelCommand);

  [LoggerMessage(
    EventId = EventIds.CommandSequence,
    Level = LogLevelCommand,
    Message = $"{PrefixCommand}{{Sequence}}"
  )]
  private static partial void LogDebugCommand(
    ILogger logger,
    string sequence
  );

  private static void LogDebugResponse(ILogger logger, ReadOnlySequence<byte> response, object? result)
  {
#pragma warning disable CA1873
    if (ReferenceEquals(result, EchobackLineMarker))
      LogDebugEchoback(logger, response.ToControlCharsPicturizedString());
    else
      LogDebugResponse(logger, response.ToControlCharsPicturizedString());
#pragma warning restore CA1873
  }

  [LoggerMessage(
    EventId = EventIds.ResponseSequence,
    Level = LogLevelResponse,
    Message = $"{PrefixResponse}{{Sequence}}"
  )]
  private static partial void LogDebugResponse(
    ILogger logger,
    string sequence
  );

  [LoggerMessage(
    EventId = EventIds.ResponseSequence,
    Level = LogLevelResponse,
    Message = $"{PrefixEchoback}{{Sequence}}"
  )]
  private static partial void LogDebugEchoback(
    ILogger logger,
    string sequence
  );

  private static void LogInfoIPEventReceived(ILogger logger, SkStackEvent ev)
  {
#pragma warning disable CA1873
    if (ev.Number == SkStackEventNumber.UdpSendCompleted) {
      LogInfoIPEventReceivedUdpSendCompleted(
        logger,
        ev.Number,
        $"{(byte)ev.Number:X2}",
        ev.Parameter,
        ev.Parameter switch {
          0 => "Successful",
          1 => "Failed",
          2 => "Neighbor Solicitation",
          _ => "Unknown",
        },
        ev.SenderAddress
      );
    }
    else {
      LogInfoIPEventReceived(
        logger,
        ev.Number,
        $"{(byte)ev.Number:X2}",
        ev.SenderAddress
      );
    }
#pragma warning restore CA1873
  }

  [LoggerMessage(
    EventId = EventIds.IPEventReceived,
    Level = LogLevelIPEventReceived,
    Message = "IPv6: {EventNumber} (EVENT {EventNumberInHexFormat}, {SenderAddress})"
  )]
  private static partial void LogInfoIPEventReceived(
    ILogger logger,
    SkStackEventNumber eventNumber,
    string eventNumberInHexFormat,
    IPAddress? senderAddress
  );

  [LoggerMessage(
    EventId = EventIds.IPEventReceived,
    Level = LogLevelIPEventReceived,
    Message = "IPv6: {EventNumber} - {ParameterInString} (EVENT {EventNumberInHexFormat:X2}, PARAM {Parameter}, {SenderAddress})"
  )]
  private static partial void LogInfoIPEventReceivedUdpSendCompleted(
    ILogger logger,
    SkStackEventNumber eventNumber,
    string eventNumberInHexFormat,
    int parameter,
    string parameterInString,
    IPAddress? senderAddress
  );

  private static void LogInfoIPEventReceived(ILogger logger, SkStackUdpReceiveEvent erxudp, ReadOnlySequence<byte> erxudpData)
    => LogInfoIPEventReceivedUdpReceived(
      logger,
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

  [LoggerMessage(
    EventId = EventIds.IPEventReceived,
    Level = LogLevelIPEventReceived,
    Message = "{Prefix}: {LocalEndPoint}←{RemoteEndPoint} {RemoteLinkLocalAddress} (secured: {IsSecured}, length: {Length})"
  )]
  private static partial void LogInfoIPEventReceivedUdpReceived(
    ILogger logger,
    string prefix,
    IPEndPoint localEndPoint,
    IPEndPoint remoteEndPoint,
    PhysicalAddress remoteLinkLocalAddress,
    bool isSecured,
    long length
  );

  private static void LogInfoAribStdT108EventReceived(ILogger logger, SkStackEvent ev)
#pragma warning disable CA1873
    => LogInfoAribStdT108EventReceived(
      logger,
      ev.Number,
      $"{(byte)ev.Number:X2}",
      ev.SenderAddress
    );
#pragma warning restore CA1873

  [LoggerMessage(
    EventId = EventIds.AribStdT108EventReceived,
    Level = LogLevelAribStdT108EventReceived,
    Message = "ARIB STD-T108: {EventNumber} (EVENT {EventNumberInHexFormat}, {SenderAddress})"
  )]
  private static partial void LogInfoAribStdT108EventReceived(
    ILogger logger,
    SkStackEventNumber eventNumber,
    string eventNumberInHexFormat,
    IPAddress? senderAddress
  );

  private static void LogInfoPanaEventReceived(ILogger logger, SkStackEvent ev)
#pragma warning disable CA1873
    => LogInfoPanaEventReceived(
      logger,
      ev.Number,
      $"{(byte)ev.Number:X2}",
      ev.SenderAddress
    );
#pragma warning restore CA1873

  [LoggerMessage(
    EventId = EventIds.PanaEventReceived,
    Level = LogLevelPanaEventReceived,
    Message = "PANA: {EventNumber} (EVENT {EventNumberInHexFormat}, {SenderAddress})"
  )]
  private static partial void LogInfoPanaEventReceived(
    ILogger logger,
    SkStackEventNumber eventNumber,
    string eventNumberInHexFormat,
    IPAddress? senderAddress
  );
}
