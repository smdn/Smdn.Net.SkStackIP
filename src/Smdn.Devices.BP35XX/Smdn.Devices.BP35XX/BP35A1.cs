// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP;

namespace Smdn.Devices.BP35XX;

public class BP35A1 : BP35Base {
  /// <summary>
  /// Refer to the initial value of baud rate for UART setting in the BP35A1.
  /// </summary>
  /// <remarks>
  /// See 'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)' for detailed specifications.
  /// </remarks>
  internal const BP35UartBaudRate DefaultValueForBP35UartBaudRate = BP35UartBaudRate.Baud115200;

  public static ValueTask<BP35A1> CreateAsync(
    string? serialPortName,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    => CreateAsync(
      configurations: new BP35A1Configurations() {
        SerialPortName = serialPortName,
      },
      serviceProvider: serviceProvider,
      cancellationToken: cancellationToken
    );

  public static ValueTask<BP35A1> CreateAsync(
    BP35A1Configurations configurations,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    => InitializeAsync(
#pragma warning disable CA2000
      device: new BP35A1(
        configurations: configurations ?? throw new ArgumentNullException(nameof(configurations)),
        serviceProvider: serviceProvider
      ),
#pragma warning restore CA2000
      tryLoadFlashMemory: configurations.TryLoadFlashMemory,
      serviceProvider: serviceProvider,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Initializes a new instance of the <see cref="BP35A1"/> class with specifying configurations.
  /// </summary>
  /// <param name="configurations">
  /// A <see cref="BP35A1Configurations"/> that holds the configurations to the <see cref="BP35A1"/> instance.
  /// </param>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/>.
  /// This constructor overload attempts to get a service of <see cref="IBP35SerialPortStreamFactory"/>, to create an <see cref="System.IO.Ports.SerialPort"/>.
  /// </param>
  private BP35A1(
    BP35A1Configurations configurations,
    IServiceProvider? serviceProvider = null
  )
    : base(
      serialPortName: configurations.SerialPortName,
#pragma warning disable CA2000
      serialPortStreamFactory: serviceProvider?.GetService<IBP35SerialPortStreamFactory>() ?? new BP35A1SerialPortStreamFactory(configurations),
#pragma warning restore CA2000
      erxudpDataFormat: SkStackERXUDPDataFormat.Binary,
      logger: serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<BP35A1>()
    )
  {
  }

  private protected class BP35A1SerialPortStreamFactory(BP35A1Configurations configurations) : SerialPortStreamFactory {
    public override BP35UartBaudRate BaudRate { get; } = configurations.BaudRate;
  }
}
