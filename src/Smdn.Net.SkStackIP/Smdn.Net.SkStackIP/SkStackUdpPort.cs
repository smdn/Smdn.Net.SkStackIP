// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System;

namespace Smdn.Net.SkStackIP;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 5. 待ち受けポート番号' for detailed specifications.</para>
/// </remarks>
public readonly struct SkStackUdpPort {
  internal const int NumberOfPorts = 6;
  internal static readonly SkStackUdpPortHandle HandleMin = SkStackUdpPortHandle.Handle1;
  internal static readonly SkStackUdpPortHandle HandleMax = SkStackUdpPortHandle.Handle6;

  public static readonly SkStackUdpPort Null = default; // Null.Handle will be invalid handle

  public SkStackUdpPortHandle Handle { get; }
  public int Port { get; }

  public bool IsNull => Handle == Null.Handle;
  public bool IsUnused => Port == 0;

  internal SkStackUdpPort(SkStackUdpPortHandle handle, int port)
  {
    Handle = handle;
    Port = port;
  }

  public override string ToString()
    => $"{Port} (#{(byte)Handle})";

  internal static bool IsPortHandleIsOutOfRange(SkStackUdpPortHandle handle)
    => handle is < SkStackUdpPortHandle.Handle1 or > SkStackUdpPortHandle.Handle6;

  internal static void ThrowIfPortHandleIsOutOfRange(SkStackUdpPortHandle handle, string paramName)
  {
    if (IsPortHandleIsOutOfRange(handle))
      throw new ArgumentOutOfRangeException(paramName: paramName, actualValue: handle, message: $"invalid value of {nameof(SkStackUdpPortHandle)}");
  }

  internal static void ThrowIfPortNumberIsOutOfRange(int portNumber, string paramName)
  {
    if (portNumber is not (>= ushort.MinValue and <= ushort.MaxValue)) // UINT16
      throw new ArgumentOutOfRangeException(paramName, portNumber, $"must be in range of {ushort.MinValue}~{ushort.MaxValue}");
  }

  internal static void ThrowIfPortNumberIsOutOfRangeOrUnused(int portNumber, string paramName)
  {
    if (portNumber == SkStackKnownPortNumbers.SetUnused)
      throw new ArgumentOutOfRangeException(paramName, portNumber, $"can not use port number {SkStackKnownPortNumbers.SetUnused}");

    ThrowIfPortNumberIsOutOfRange(portNumber, paramName);
  }
}
