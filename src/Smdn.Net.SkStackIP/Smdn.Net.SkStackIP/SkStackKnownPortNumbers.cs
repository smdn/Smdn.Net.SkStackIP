// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP;

public static class SkStackKnownPortNumbers {
  /// <summary>Represents the port number <c>3610</c>, assigned to ECHONET Lite.</summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 5.1. UDP ポート' for detailed specifications.</para>
  /// </remarks>
  public const int EchonetLite = 3610;

  /// <summary>Represents the  port number <c>716</c>, assigned to PANA.</summary>
  /// <remarks>
  ///   <para>See below for detailed specifications.</para>
  ///   <list type="bullet">
  ///     <item><description>'BP35A1コマンドリファレンス 5.1. UDP ポート'</description></item>
  ///     <item><description><see href="https://datatracker.ietf.org/doc/html/rfc5191">[RFC5191] Protocol for Carrying Authentication for Network Access (PANA) 6.1. IP and UDP Headers</see></description></item>
  ///   </list>
  /// </remarks>
  public const int Pana = 716;

  /// <summary>Represents the  port number <c>0</c>, to set to be unused port.</summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.19. SKUDPPORT' for detailed specifications.</para>
  /// </remarks>
  internal const int SetUnused = 0;
}
