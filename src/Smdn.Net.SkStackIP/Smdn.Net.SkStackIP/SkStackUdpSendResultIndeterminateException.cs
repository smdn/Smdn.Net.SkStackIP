// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Net.SkStackIP;

#pragma warning disable CS0419
/// <summary>
/// The exception that is thrown when the <c>EVENT 21</c> was not raised or received after performing <c>SKSENDTO</c>.
/// </summary>
/// <seealso cref="SkStackClient.SendSKSENDTOAsync"/>
#pragma warning restore CS0419
public class SkStackUdpSendResultIndeterminateException : InvalidOperationException {
  private const string DefaultMessage = "Unable to confirm the send results since the EVENT 21 was not raised or received after performing SKSENDTO.";

  public SkStackUdpSendResultIndeterminateException()
    : base(message: DefaultMessage)
  {
  }

  public SkStackUdpSendResultIndeterminateException(string message)
    : base(message: message)
  {
  }

  public SkStackUdpSendResultIndeterminateException(string message, Exception? innerException = null)
    : base(message: message, innerException: innerException)
  {
  }
}
