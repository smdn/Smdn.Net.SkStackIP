// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Devices.BP35XX;

/// <summary>
/// An enumeration type representing the configuration values for the UART baud rate, to be set or get by the <c>WUART</c> and <c>RUART</c> commands.
/// </summary>
/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.33. RUART (プロダクト設定コマンド)'</description></item>
///   </list>
/// </remarks>
#pragma warning disable CA1027
public enum BP35UartBaudRate : byte {
#pragma warning restore CA1027
  Baud115200  = 0b_0_000_0_000, // default(BP35UartBaudRate)
  Baud2400    = 0b_0_000_0_001,
  Baud4800    = 0b_0_000_0_010,
  Baud9600    = 0b_0_000_0_011,
  Baud19200   = 0b_0_000_0_100,
  Baud38400   = 0b_0_000_0_101,
  Baud57600   = 0b_0_000_0_110,
}
