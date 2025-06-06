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

  /// <summary>
  /// Refer to the initial value of the flow control for UART setting in the BP35A1.
  /// </summary>
  /// <remarks>
  /// See 'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)' for detailed specifications.
  /// </remarks>
  internal const bool DefaultValueForUseFlowControl = false;

  public static ValueTask<BP35A1> CreateAsync(
    string? serialPortName,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    => CreateAsync(
      options: new BP35A1Options() {
        SerialPortName = serialPortName,
      },
      serviceProvider: serviceProvider,
      cancellationToken: cancellationToken
    );

  [Obsolete($"Use {nameof(BP35A1Options)} instead.")]
  public static ValueTask<BP35A1> CreateAsync(
    BP35A1Configurations configurations,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    => CreateAsync(
      options: configurations ?? throw new ArgumentNullException(nameof(configurations)),
      serviceProvider: serviceProvider,
      cancellationToken: cancellationToken
    );

  public static ValueTask<BP35A1> CreateAsync(
    BP35A1Options options,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    => InitializeAsync(
#pragma warning disable CA2000
      device: new BP35A1(
        options: options ?? throw new ArgumentNullException(nameof(options)),
        serviceProvider: serviceProvider
      ),
#pragma warning restore CA2000
      tryLoadFlashMemory: options.TryLoadFlashMemory,
      serviceProvider: serviceProvider,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Initializes a new instance of the <see cref="BP35A1"/> class with specifying options.
  /// </summary>
  /// <param name="options">
  /// A <see cref="BP35A1Options"/> that holds the options to configure the new instance.
  /// </param>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/>.
  /// This constructor overload attempts to get a service of <see cref="IBP35SerialPortStreamFactory"/>, to create an <see cref="System.IO.Ports.SerialPort"/>.
  /// </param>
  private BP35A1(
    BP35A1Options options,
    IServiceProvider? serviceProvider = null
  )
    : base(
      serialPortName: options.SerialPortName,
#pragma warning disable CA2000
      serialPortStreamFactory: serviceProvider?.GetService<IBP35SerialPortStreamFactory>() ?? new BP35A1SerialPortStreamFactory(options),
#pragma warning restore CA2000
      erxudpDataFormat: SkStackERXUDPDataFormat.Binary,
      logger: serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<BP35A1>()
    )
  {
  }

  private sealed class BP35A1SerialPortStreamFactory(BP35A1Options options) : SerialPortStreamFactory {
    public override BP35UartBaudRate BaudRate { get; } = options.BaudRate;
    public override bool UseFlowControl { get; } = options.UseFlowControl;
  }
}
