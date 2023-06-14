// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net;

namespace Smdn.Net.SkStackIP.Protocol;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 4.8. EVENT' for detailed specifications.</para>
/// </remarks>
internal readonly struct SkStackEvent {
  public static SkStackEvent Create(
    SkStackEventNumber number,
    IPAddress senderAddress,
    int parameter,
    SkStackEventCode expectedSubsequentEventCode
  )
    => new(
      number,
      senderAddress ?? throw new ArgumentNullException(nameof(senderAddress)),
      parameter,
      expectedSubsequentEventCode
    );

  public static SkStackEvent CreateWakeupSignalReceived()
    => new(
      SkStackEventNumber.WakeupSignalReceived,
      default,
      default,
      default
    );

  public bool HasSenderAddress => Number != SkStackEventNumber.WakeupSignalReceived;

  public SkStackEventNumber Number { get; }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhen(true, nameof(HasSenderAddress))]
#endif
  public IPAddress? SenderAddress { get; }

  public int Parameter { get; }

  public SkStackEventCode ExpectedSubsequentEventCode { get; }

  private SkStackEvent(
    SkStackEventNumber number,
    IPAddress? senderAddress,
    int parameter,
    SkStackEventCode expectedSubsequentEventCode
  )
  {
    Number = number;
    SenderAddress = senderAddress;
    Parameter = parameter;
    ExpectedSubsequentEventCode = expectedSubsequentEventCode;
  }
}
