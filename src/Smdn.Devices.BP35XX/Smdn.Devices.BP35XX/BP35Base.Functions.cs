// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.BP35XX;

#pragma warning disable IDE0040
partial class BP35Base {
#pragma warning restore IDE0040
  public ValueTask SetUdpDataFormatAsync(
    BP35UdpReceiveDataFormat format,
    CancellationToken cancellationToken = default
  )
  {
    return SendWOPTAsync(
      mode: format switch {
        BP35UdpReceiveDataFormat.Binary => BP35ERXUDPFormatBinary,
        BP35UdpReceiveDataFormat.HexAscii => BP35ERXUDPFormatHexAscii,
        _ => throw new ArgumentException($"undefined value of {nameof(BP35UdpReceiveDataFormat)}", nameof(format)),
      },
      cancellationToken: cancellationToken
    );
  }

  public async ValueTask<BP35UdpReceiveDataFormat> GetUdpDataFormatAsync(
    CancellationToken cancellationToken = default
  )
  {
    var mode = await SendROPTAsync(
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return (mode & BP35ERXUDPFormatMask) switch {
      BP35ERXUDPFormatBinary => BP35UdpReceiveDataFormat.Binary,
      BP35ERXUDPFormatHexAscii => BP35UdpReceiveDataFormat.HexAscii,
      _ => BP35UdpReceiveDataFormat.Binary, // XXX
    };
  }

  public ValueTask SetUartOptionsAsync(
    BP35UartBaudRate baudRate,
    BP35UartCharacterInterval characterInterval = default,
    BP35UartFlowControl flowControl = default,
    CancellationToken cancellationToken = default
  )
    => SetUartOptionsAsync(
      uartConfigurations: new(baudRate, characterInterval, flowControl),
      cancellationToken: cancellationToken
    );

  public ValueTask SetUartOptionsAsync(
    BP35UartConfigurations uartConfigurations,
    CancellationToken cancellationToken = default
  )
    => SendWUARTAsync(
      mode: uartConfigurations.Mode,
      cancellationToken: cancellationToken
    );

  public async ValueTask<BP35UartConfigurations> GetUartOptionsAsync(
    CancellationToken cancellationToken = default
  )
  {
    var mode = await SendRUARTAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    return new(mode);
  }
}
