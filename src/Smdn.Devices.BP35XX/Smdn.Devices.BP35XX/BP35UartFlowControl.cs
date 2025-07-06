// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1028

namespace Smdn.Devices.BP35XX;

/// <summary>
/// An enumeration type representing the configuration values for the flow control in UART, to be set or get by the <c>WUART</c> and <c>RUART</c> commands.
/// </summary>
/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.33. RUART (プロダクト設定コマンド)'</description></item>
///   </list>
/// </remarks>
#pragma warning disable CA1027
public enum BP35UartFlowControl : byte {
#pragma warning restore CA1027
  Disabled = 0b_0_000_0_000, // default(BP35UartFlowControl)
  Enabled  = 0b_1_000_0_000,
}
