// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.BP35XX;

/// <summary>
/// A read-only structure that represents the configuration values relevant to UART, set and get by the <c>WUART</c> and <c>RUART</c> commands.
/// </summary>
/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.33. RUART (プロダクト設定コマンド)'</description></item>
///   </list>
/// </remarks>
public readonly struct BP35UartConfigurations {
  private const byte BaudRateMask = 0b_0_000_0_111;
  // private const byte ReservedBitMask = 0b_0_000_1_000;
  private const byte CharacterIntervalMask = 0b_0_111_0_000;
  private const byte FlowControlMask = 0b_1_000_0_000;

  internal byte Mode { get; }

  public BP35UartBaudRate BaudRate => (BP35UartBaudRate)(Mode & BaudRateMask);
  public BP35UartCharacterInterval CharacterInterval => (BP35UartCharacterInterval)(Mode & CharacterIntervalMask);
  public BP35UartFlowControl FlowControl => (BP35UartFlowControl)(Mode & FlowControlMask);

  internal BP35UartConfigurations(
    byte mode
  )
  {
    Mode = mode;
  }

  public BP35UartConfigurations(
    BP35UartBaudRate baudRate,
    BP35UartCharacterInterval characterInterval,
    BP35UartFlowControl flowControl
  )
  {
#if SYSTEM_ENUM_ISDEFINED_OF_TENUM
    if (!Enum.IsDefined(baudRate))
#else
    if (!Enum.IsDefined(typeof(BP35UartBaudRate), baudRate))
#endif
      throw new ArgumentException($"undefined value of {nameof(BP35UartBaudRate)}", nameof(baudRate));

#if SYSTEM_ENUM_ISDEFINED_OF_TENUM
    if (!Enum.IsDefined(flowControl))
#else
    if (!Enum.IsDefined(typeof(BP35UartFlowControl), flowControl))
#endif
      throw new ArgumentException($"undefined value of {nameof(BP35UartFlowControl)}", nameof(flowControl));

#if SYSTEM_ENUM_ISDEFINED_OF_TENUM
    if (!Enum.IsDefined(characterInterval))
#else
    if (!Enum.IsDefined(typeof(BP35UartCharacterInterval), characterInterval))
#endif
      throw new ArgumentException($"undefined value of {nameof(BP35UartCharacterInterval)}", nameof(characterInterval));

    Mode = (byte)((byte)baudRate | (byte)characterInterval | (byte)flowControl);
  }

  public void Deconstruct(
    out BP35UartBaudRate baudRate,
    out BP35UartCharacterInterval characterInterval,
    out BP35UartFlowControl flowControl
  )
  {
    baudRate = BaudRate;
    characterInterval = CharacterInterval;
    flowControl = FlowControl;
  }
}
