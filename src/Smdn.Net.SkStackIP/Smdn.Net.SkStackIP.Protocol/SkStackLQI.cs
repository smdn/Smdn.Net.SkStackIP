// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStackLQI {
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.6. EEDSCAN' for detailed specifications.</para>
  /// </remarks>
  public static double ToRSSI(int lqi) => (0.275 * lqi) - 104.27;
}
