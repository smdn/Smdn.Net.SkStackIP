// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Formats;
using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Devices.BP35XX;

#pragma warning disable IDE0040
partial class BP35Base {
#pragma warning restore IDE0040
  private static readonly SkStackProtocolSyntax RMCommandSyntax = new BP35CommandSyntax();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  private const byte BP35ERXUDPFormatMask = 0b_0000000_1;

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  private const byte BP35ERXUDPFormatBinary = 0b_0000000_0;

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  private const byte BP35ERXUDPFormatHexAscii = 0b_0000000_1;

  /// <summary>
  ///   <para>Sends a command <c>WOPT</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  private protected async ValueTask SendWOPTAsync(
    byte mode,
    CancellationToken cancellationToken = default
  )
  {
    byte[]? modeBytes = null;

    try {
      modeBytes = ArrayPool<byte>.Shared.Rent(2);

      _ = Hexadecimal.TryEncodeUpperCase(mode, modeBytes.AsSpan(), out var lengthOfMODE);

      _ = await SendCommandAsync(
        command: BP35CommandNames.WOPT,
        writeArguments: writer => writer.WriteToken(modeBytes.AsSpan(0, lengthOfMODE)),
        syntax: RMCommandSyntax,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      ).ConfigureAwait(false);
    }
    finally {
      if (modeBytes is not null)
        ArrayPool<byte>.Shared.Return(modeBytes);
    }
  }

  /// <summary>
  ///   <para>Sends a command <c>ROPT</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.31. ROPT (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  private protected async ValueTask<byte> SendROPTAsync(
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendCommandAsync(
      command: BP35CommandNames.ROPT,
      writeArguments: null,
      syntax: RMCommandSyntax,
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    ).ConfigureAwait(false);

    return Convert.ToByte(Encoding.ASCII.GetString(resp.StatusText.Span), 16);
  }

  /// <summary>
  ///   <para>Sends a command <c>WUART</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  private protected async ValueTask SendWUARTAsync(
    byte mode,
    CancellationToken cancellationToken = default
  )
  {
    byte[]? modeBytes = null;

    try {
      modeBytes = ArrayPool<byte>.Shared.Rent(2);

      _ = Hexadecimal.TryEncodeUpperCase(mode, modeBytes.AsSpan(), out var lengthOfMODE);

      _ = await SendCommandAsync(
        command: BP35CommandNames.WUART,
        writeArguments: writer => writer.WriteToken(modeBytes.AsSpan(0, lengthOfMODE)),
        syntax: RMCommandSyntax,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      ).ConfigureAwait(false);
    }
    finally {
      if (modeBytes is not null)
        ArrayPool<byte>.Shared.Return(modeBytes);
    }
  }

  /// <summary>
  ///   <para>Sends a command <c>RUART</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.33. RUART (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  private protected async ValueTask<byte> SendRUARTAsync(
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendCommandAsync(
      command: BP35CommandNames.RUART,
      writeArguments: null,
      syntax: RMCommandSyntax,
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    ).ConfigureAwait(false);

    return Convert.ToByte(Encoding.ASCII.GetString(resp.StatusText.Span), 16);
  }
}
