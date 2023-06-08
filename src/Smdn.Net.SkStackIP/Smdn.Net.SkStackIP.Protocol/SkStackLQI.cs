// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStackLQI {
  /// <remarks>reference: BP35A1コマンドリファレンス 4.6. EEDSCAN</remarks>
  public static double ToRSSI(int lqi) => 0.275 * lqi - 104.27;
}
