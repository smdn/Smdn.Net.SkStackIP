// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1008

namespace Smdn.Net.SkStackIP;

/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.7. SKSENDTO'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.19. SKUDPPORT'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 4.7. EPORT'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 5. 待ち受けポート番号'</description></item>
///   </list>
/// </remarks>
public enum SkStackUdpPortHandle : byte {
  None = 0,
  Handle1 = 1,
  Handle2 = 2,
  Handle3 = 3,
  Handle4 = 4,
  Handle5 = 5,
  Handle6 = 6,
}
