// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that is thrown when the <see cref="SkStackClient"/> attempted to perform <c>SKSENDTO</c> and raised <c>EVENT 21</c> with <c>PARAM 1</c> ('Failed to send UDP').
/// </summary>
/// <seealso cref="SkStackEventNumber.UdpSendCompleted"/>
/// <seealso cref="SkStackClient.SendUdpEchonetLiteAsync"/>
public class SkStackUdpSendFailedException : InvalidOperationException {
  public SkStackUdpPortHandle PortHandle { get; }
  public IPAddress? PeerAddress { get; }

  public SkStackUdpSendFailedException()
    : base()
  {
  }

  public SkStackUdpSendFailedException(string message)
    : base(message: message)
  {
  }

  public SkStackUdpSendFailedException(string message, Exception? innerException = null)
    : base(message: message, innerException: innerException)
  {
  }

  public SkStackUdpSendFailedException(
    string message,
    SkStackUdpPortHandle portHandle,
    IPAddress peerAddress,
    Exception? innerException = null
  )
    : base(message: message, innerException: innerException)
  {
    PortHandle = portHandle;
    PeerAddress = peerAddress;
  }
}
