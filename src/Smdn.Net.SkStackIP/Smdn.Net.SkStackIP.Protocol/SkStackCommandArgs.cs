// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Smdn.Net.SkStackIP.Protocol {
  internal static class SkStackCommandArgs {
    private static readonly ReadOnlyMemory<byte> hexNumbers = SkStack.ToByteSequence("0123456789ABCDEF");
    public static ReadOnlyMemory<byte> GetHex(int hexNumber) => hexNumbers.Slice(hexNumber, 1);
    private static byte GetHexByte(int hexNumber) => hexNumbers.Span[hexNumber];

    public static Memory<byte> ConvertToUINT8(byte number, bool zeroPadding = false)
    {
      var mem = new byte[2];

      if (!TryConvertToUINT8(mem, number, out var bytesWritten, zeroPadding))
        throw new InvalidOperationException("unexpected error in conversion");

      return mem.AsMemory(0, bytesWritten);
    }

    public static bool TryConvertToUINT8(Memory<byte> memory, byte number, out int bytesWritten, bool zeroPadding = false)
      => TryConvertToHex(memory, number, out bytesWritten, length: zeroPadding ? 2 : 0);

    public static Memory<byte> ConvertToUINT16(ushort number, bool zeroPadding = false)
    {
      var mem = new byte[4];

      if (!TryConvertToUINT16(mem, number, out var bytesWritten, zeroPadding))
        throw new InvalidOperationException("unexpected error in conversion");

      return mem.AsMemory(0, bytesWritten);
    }

    public static bool TryConvertToUINT16(Memory<byte> memory, ushort number, out int bytesWritten, bool zeroPadding = false)
      => TryConvertToHex(memory, number, out bytesWritten, length: zeroPadding ? 4 : 0);

    public static Memory<byte> ConvertToUINT32(uint number, bool zeroPadding = false)
    {
      var mem = new byte[8];

      if (!TryConvertToUINT32(mem, number, out var bytesWritten, zeroPadding))
        throw new InvalidOperationException("unexpected error in conversion");

      return mem.AsMemory(0, bytesWritten);
    }

    public static bool TryConvertToUINT32(Memory<byte> memory, uint number, out int bytesWritten, bool zeroPadding = false)
      => TryConvertToHex(memory, number, out bytesWritten, length: zeroPadding ? 8 : 0);

    public static Memory<byte> ConvertToUINT64(ulong number, bool zeroPadding = false)
    {
      var mem = new byte[16];

      if (!TryConvertToUINT64(mem, number, out var bytesWritten, zeroPadding))
        throw new InvalidOperationException("unexpected error in conversion");

      return mem.AsMemory(0, bytesWritten);
    }

    public static bool TryConvertToUINT64(Memory<byte> memory, ulong number, out int bytesWritten, bool zeroPadding = false)
      => TryConvertToHex(memory, number, out bytesWritten, length: zeroPadding ? 16 : 0);

    public static bool TryConvertToHex(Memory<byte> memory, ulong number, out int bytesWritten, int length = 0)
    {
      bytesWritten = default;

      if (8 < length)
        throw new NotSupportedException("length too long");

      Span<char> charsSpan = stackalloc char[length == 0 ? 20 /*ulong.MaxValue.ToString("D").Length*/ : length];

      if (!number.TryFormat(
        charsSpan,
        out var charsWritten,
        length == 0 ? "X" : stackalloc char[2] { 'X', (char)('0' + length) }
      )) {
        return false;
      }

      if (memory.Length < charsWritten)
        return false;

      bytesWritten = SkStack.DefaultEncoding.GetBytes(charsSpan.Slice(0, charsWritten), memory.Span);

      return true;
    }

    public static bool TryConvertToHex(Memory<byte> memory, ReadOnlyMemory<byte> sequence, out int bytesWritten)
    {
      bytesWritten = 0;

      if (memory.Length < sequence.Length * 2)
        return false;

      var seq = sequence.Span;
      var span = memory.Span;

      for (var i = 0; i < sequence.Length; i++) {
        span[bytesWritten++] = GetHexByte(seq[i] >> 4);
        span[bytesWritten++] = GetHexByte(seq[i] & 0xF);
      }

      return true;
    }

    public static readonly int LengthOfADDR64 = 16; // "0123456789ABCDEF".Length

    public static bool TryConvertToADDR64(Memory<byte> memory, PhysicalAddress macAddress, out int bytesWritten)
    {
      bytesWritten = default;

      if (memory.Length < LengthOfADDR64)
        return false;

      if (macAddress is null)
        throw new ArgumentNullException(nameof(macAddress));

      var macAddressBytes = macAddress.GetAddressBytes();

      if (macAddressBytes.Length != 8)
        throw new ArgumentException("address length must be exactly 64 bits", nameof(macAddress));

      return TryConvertToHex(memory, macAddressBytes, out bytesWritten);
    }

    public static readonly int LengthOfIPADDR = 39; // "XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX".Length

    public static bool TryConvertToIPADDR(Memory<byte> memory, IPAddress ipv6address, out int bytesWritten)
    {
      bytesWritten = default;

      if (memory.Length < LengthOfIPADDR)
        return false;

      if (ipv6address is null)
        throw new ArgumentNullException(nameof(ipv6address));
      if (ipv6address.AddressFamily != AddressFamily.InterNetworkV6)
        throw new ArgumentException($"`{nameof(ipv6address)}.{nameof(IPAddress.AddressFamily)}` must be {nameof(AddressFamily.InterNetworkV6)}");

      Span<byte> addressBytes = stackalloc byte[16];

      if (!ipv6address.TryWriteBytes(addressBytes, out _))
        throw new InvalidOperationException("IPAddress.TryWriteBytes failed");

      var span = memory.Span;

      for (var i = 0; i < 16; i += 2) {
        if (0 < i)
          span[bytesWritten++] = (byte)':';

        span[bytesWritten++] = GetHexByte(addressBytes[i] >> 4);
        span[bytesWritten++] = GetHexByte(addressBytes[i] & 0xF);
        span[bytesWritten++] = GetHexByte(addressBytes[i + 1] >> 4);
        span[bytesWritten++] = GetHexByte(addressBytes[i + 1] & 0xF);
      }

      return true;
    }

    public static IEnumerable<ReadOnlyMemory<byte>> CreateEnumerable(
      ReadOnlyMemory<byte> first
    )
    {
      yield return first;
    }

    public static IEnumerable<ReadOnlyMemory<byte>> CreateEnumerable(
      ReadOnlyMemory<byte> first,
      ReadOnlyMemory<byte> second
    )
    {
      yield return first;
      yield return second;
    }

    public static IEnumerable<ReadOnlyMemory<byte>> CreateEnumerable(
      ReadOnlyMemory<byte> first,
      ReadOnlyMemory<byte> second,
      ReadOnlyMemory<byte> third
    )
    {
      yield return first;
      yield return second;
      yield return third;
    }

    public static IEnumerable<ReadOnlyMemory<byte>> CreateEnumerable(
      ReadOnlyMemory<byte> first,
      ReadOnlyMemory<byte> second,
      ReadOnlyMemory<byte> third,
      ReadOnlyMemory<byte> fourth,
      ReadOnlyMemory<byte> fifth,
      ReadOnlyMemory<byte> sixth
    )
    {
      yield return first;
      yield return second;
      yield return third;
      yield return fourth;
      yield return fifth;
      yield return sixth;
    }
  }
}