// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1028

namespace Smdn.Devices.BP35XX;

/// <summary>
/// An enumeration type representing the configuration values for the display format of the data part in ERXUDP event, to be set or get by the <c>WOPT</c> and <c>ROPT</c> commands.
/// </summary>
/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.31. ROPT (プロダクト設定コマンド)'</description></item>
///   </list>
/// </remarks>
#pragma warning disable CA1027
public enum BP35UdpReceiveDataFormat : byte {
#pragma warning restore CA1027
  Binary = 0b_0000_0000,
  HexAscii = 0b_0000_0001,
}
