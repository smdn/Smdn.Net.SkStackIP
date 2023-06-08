// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP;

public static class SkStackKnownPortNumbers {
  /// <summary>The port number 3610, assigned to ECHONET Lite.</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 5.1. UDP ポート</remarks>
  public const int EchonetLite = 3610;

  /// <summary>The port number 716, assigned to PANA.</summary>
  /// <remarks>
  /// reference: BP35A1コマンドリファレンス 5.1. UDP ポート
  /// reference: [RFC5191] Protocol for Carrying Authentication for Network Access (PANA) 6.1. IP and UDP Headers
  /// </remarks>
  public const int Pana = 716;

  /// <summary>The port number 0, to set to be unused port.</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.19. SKUDPPORT</remarks>
  internal const int SetUnused = 0;
}
