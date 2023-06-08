// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1008

namespace Smdn.Net.SkStackIP;

/// <remarks>
/// reference: BP35A1コマンドリファレンス
/// 3.7. SKSENDTO
/// 3.19. SKUDPPORT
/// 4.7. EPORT
/// 5. 待ち受けポート番号
/// </remarks>
public enum SkStackUdpPortHandle : byte {
  Handle1 = 1,
  Handle2 = 2,
  Handle3 = 3,
  Handle4 = 4,
  Handle5 = 5,
  Handle6 = 6,
}
