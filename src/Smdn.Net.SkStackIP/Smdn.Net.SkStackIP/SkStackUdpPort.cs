// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP {
  /// <remarks>reference: BP35A1コマンドリファレンス 5. 待ち受けポート番号</remarks>
  public readonly struct SkStackUdpPort {
    internal const int NumberOfPorts = 6;
    internal static readonly SkStackUdpPortHandle HandleMin = SkStackUdpPortHandle.Handle1;
    internal static readonly SkStackUdpPortHandle HandleMax = SkStackUdpPortHandle.Handle6;

    public SkStackUdpPortHandle Handle { get; }
    public int Port { get; }

    internal SkStackUdpPort(SkStackUdpPortHandle handle, int port)
    {
      this.Handle = handle;
      this.Port = port;
    }

    public override string ToString()
      => $"{Port} (#{(byte)Handle})";

    internal static void ThrowIfPortHandleIsNotDefined(SkStackUdpPortHandle handle, string paramName)
    {
#if NET5_0_OR_GREATER
      if (!Enum.IsDefined(handle))
#else
      if (!Enum.IsDefined(typeof(SkStackUdpPortHandle), handle))
#endif
        throw new ArgumentOutOfRangeException(paramName, handle, $"undefined value of {nameof(SkStackUdpPortHandle)}");
    }

    internal static void ThrowIfPortNumberIsOutOfRange(int portNumber, string paramName)
    {
      if (!(ushort.MinValue <= portNumber && portNumber <= ushort.MaxValue)) // UINT16
        throw new ArgumentOutOfRangeException(paramName, portNumber, $"must be in range of {ushort.MinValue}~{ushort.MaxValue}");
    }

    internal static void ThrowIfPortNumberIsOutOfRangeOrUnused(int portNumber, string paramName)
    {
      if (portNumber == SkStackKnownPortNumbers.SetUnused)
        throw new ArgumentOutOfRangeException(paramName, portNumber, $"can not use port number {SkStackKnownPortNumbers.SetUnused}");

      ThrowIfPortNumberIsOutOfRange(portNumber, paramName);
    }
  }
}