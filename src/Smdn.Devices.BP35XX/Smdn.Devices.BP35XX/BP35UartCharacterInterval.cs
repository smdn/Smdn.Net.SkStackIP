// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Devices.BP35XX;

/// <summary>
/// An enumeration type representing the configuration values for the inter-character intervals in UART, to be set or get by the <c>WUART</c> and <c>RUART</c> commands.
/// </summary>
/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.33. RUART (プロダクト設定コマンド)'</description></item>
///   </list>
/// </remarks>
#pragma warning disable CA1027
public enum BP35UartCharacterInterval : byte {
#pragma warning restore CA1027
  None            = 0b_0_000_0_000, // default(BP35UartCharacterInterval)
  Microseconds100 = 0b_0_001_0_000,
  Microseconds200 = 0b_0_010_0_000,
  Microseconds300 = 0b_0_011_0_000,
  Microseconds400 = 0b_0_100_0_000,
  Microseconds50  = 0b_0_101_0_000, // or may be 500 microseconds?
}
