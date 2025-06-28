// Smdn.Devices.BP35XX.dll (Smdn.Devices.BP35XX-2.2.0)
//   Name: Smdn.Devices.BP35XX
//   AssemblyVersion: 2.2.0.0
//   InformationalVersion: 2.2.0+be87e7d6640b81d5e5e0c22af1c2491d40cd8c28
//   TargetFramework: .NETCoreApp,Version=v8.0
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.Fundamental.PrintableEncoding.Hexadecimal, Version=3.0.0.0, Culture=neutral
//     Smdn.Net.SkStackIP, Version=1.0.0.0, Culture=neutral
//     System.ComponentModel, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.IO.Ports, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Memory, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Net.NetworkInformation, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Net.Primitives, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
#nullable enable annotations

using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Smdn.Devices.BP35XX;
using Smdn.Net.SkStackIP;

namespace Smdn.Devices.BP35XX {
  public interface IBP35SerialPortStreamFactory : IDisposable {
    Stream CreateSerialPortStream(string? serialPortName);
  }

  public enum BP35UartBaudRate : byte {
    Baud115200 = 0,
    Baud19200 = 4,
    Baud2400 = 1,
    Baud38400 = 5,
    Baud4800 = 2,
    Baud57600 = 6,
    Baud9600 = 3,
  }

  public enum BP35UartCharacterInterval : byte {
    Microseconds100 = 16,
    Microseconds200 = 32,
    Microseconds300 = 48,
    Microseconds400 = 64,
    Microseconds50 = 80,
    None = 0,
  }

  public enum BP35UartFlowControl : byte {
    Disabled = 0,
    Enabled = 128,
  }

  public enum BP35UdpReceiveDataFormat : byte {
    Binary = 0,
    HexAscii = 1,
  }

  public class BP35A1 : BP35Base {
    [Obsolete("Use BP35A1Options instead.")]
    public static ValueTask<BP35A1> CreateAsync(BP35A1Configurations configurations, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default) {}
    public static ValueTask<BP35A1> CreateAsync(BP35A1Options options, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default) {}
    public static ValueTask<BP35A1> CreateAsync(string? serialPortName, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default) {}
  }

  [Obsolete("Use BP35A1Options instead.")]
  public sealed class BP35A1Configurations : BP35A1Options {
    public BP35A1Configurations() {}
  }

  public class BP35A1Options {
    public BP35A1Options() {}

    public BP35UartBaudRate BaudRate { get; set; }
    public string? SerialPortName { get; set; }
    public bool TryLoadFlashMemory { get; set; }
    public bool UseFlowControl { get; set; }

    public BP35A1Options Configure(BP35A1Options baseOptions) {}
  }

  public abstract class BP35Base : SkStackClient {
    public IPAddress LinkLocalAddress { get; }
    public PhysicalAddress MacAddress { get; }
    public string RohmPassword { get; }
    public string RohmUserId { get; }
    public string SkStackAppVersion { get; }
    public Version SkStackVersion { get; }

    public async ValueTask<BP35UartConfigurations> GetUartOptionsAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask<BP35UdpReceiveDataFormat> GetUdpDataFormatAsync(CancellationToken cancellationToken = default) {}
    public ValueTask SetUartOptionsAsync(BP35UartBaudRate baudRate, BP35UartCharacterInterval characterInterval = BP35UartCharacterInterval.None, BP35UartFlowControl flowControl = BP35UartFlowControl.Disabled, CancellationToken cancellationToken = default) {}
    public ValueTask SetUartOptionsAsync(BP35UartConfigurations uartConfigurations, CancellationToken cancellationToken = default) {}
    public ValueTask SetUdpDataFormatAsync(BP35UdpReceiveDataFormat format, CancellationToken cancellationToken = default) {}
  }

  public class BP35SerialPortException : IOException {
    public BP35SerialPortException() {}
    public BP35SerialPortException(string message) {}
    public BP35SerialPortException(string message, Exception? innerException = null) {}
  }

  public readonly struct BP35UartConfigurations {
    public BP35UartConfigurations(BP35UartBaudRate baudRate, BP35UartCharacterInterval characterInterval, BP35UartFlowControl flowControl) {}

    public BP35UartBaudRate BaudRate { get; }
    public BP35UartCharacterInterval CharacterInterval { get; }
    public BP35UartFlowControl FlowControl { get; }

    public void Deconstruct(out BP35UartBaudRate baudRate, out BP35UartCharacterInterval characterInterval, out BP35UartFlowControl flowControl) {}
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.6.0.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.4.0.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
