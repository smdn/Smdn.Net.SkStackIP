// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using Smdn.Formats;

namespace Smdn.Net.SkStackIP.Protocol;

internal static class ISkStackCommandLineWriterExtensions {
  private const int LengthOfADDR64 = 16; // "0123456789ABCDEF".Length
  private const int LengthOfIPADDR = 39; // "XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX".Length

  private static readonly ReadOnlyMemory<byte> hexNumbers = SkStack.ToByteSequence("0123456789ABCDEF");

  public static void WriteTokenHex(this ISkStackCommandLineWriter writer, byte value)
    => writer.WriteToken(hexNumbers.Span.Slice(value, 1));

  public static void WriteTokenBinary(this ISkStackCommandLineWriter writer, bool value)
    => WriteTokenUINT8(writer, value ? (byte)1 : (byte)0, zeroPadding: false);

  public static void WriteTokenUINT8(this ISkStackCommandLineWriter writer, byte value, bool zeroPadding = false)
    => WriteTokenUnsignedNumber(writer, value, length: zeroPadding ? 2 : 0);

  public static void WriteTokenUINT16(this ISkStackCommandLineWriter writer, ushort value, bool zeroPadding = false)
    => WriteTokenUnsignedNumber(writer, value, length: zeroPadding ? 4 : 0);

  public static void WriteTokenUINT32(this ISkStackCommandLineWriter writer, uint value, bool zeroPadding = false)
    => WriteTokenUnsignedNumber(writer, value, length: zeroPadding ? 8 : 0);

  public static void WriteTokenUINT64(this ISkStackCommandLineWriter writer, ulong value, bool zeroPadding = false)
    => WriteTokenUnsignedNumber(writer, value, length: zeroPadding ? 16 : 0);

  private static void WriteTokenUnsignedNumber(ISkStackCommandLineWriter writer, ulong value, int length)
  {
    if (16 < length)
      throw new NotSupportedException("length too long");

    Span<char> charsSpan = stackalloc char[
      length == 0
        ? 16 // ulong.MaxValue.ToString("X").Length
        : length
    ];

    // TODO: use IUtf8SpanFormattable on .NET 8
    if (!value.TryFormat(
      charsSpan,
      out var charsWritten,
      length == 0 ? "X" : stackalloc char[2] { 'X', (char)('0' + length) },
      provider: null
    )) {
      throw new InvalidOperationException("unexpected error in conversion");
    }

    Span<byte> bytesSpan = stackalloc byte[charsWritten];

    var bytesWritten = SkStack.DefaultEncoding.GetBytes(charsSpan.Slice(0, charsWritten), bytesSpan);

    writer.WriteToken(bytesSpan.Slice(0, bytesWritten));
  }

  public static void WriteTokenADDR64(this ISkStackCommandLineWriter writer, PhysicalAddress macAddress)
  {
    if (macAddress is null)
      throw new ArgumentNullException(nameof(macAddress));

    var macAddressBytes = macAddress.GetAddressBytes();

    if (macAddressBytes.Length != 8)
      throw new ArgumentException("address length must be exactly 64 bits", nameof(macAddress));

    Span<byte> addr64 = stackalloc byte[LengthOfADDR64];

    _ = Hexadecimal.TryEncodeUpperCase(macAddressBytes.AsSpan(), addr64, out _);

    writer.WriteToken(addr64);
  }

  public static void WriteTokenIPADDR(this ISkStackCommandLineWriter writer, IPAddress ipv6address)
  {
    if (ipv6address is null)
      throw new ArgumentNullException(nameof(ipv6address));
    if (ipv6address.AddressFamily != AddressFamily.InterNetworkV6)
      throw new ArgumentException($"`{nameof(ipv6address)}.{nameof(IPAddress.AddressFamily)}` must be {nameof(AddressFamily.InterNetworkV6)}");

    const int lengthOfIPv6Address = 16;

    Span<byte> addressBytes = stackalloc byte[lengthOfIPv6Address];

    if (!ipv6address.TryWriteBytes(addressBytes, out _))
      throw new InvalidOperationException($"{nameof(IPAddress)}.{nameof(IPAddress.TryWriteBytes)} failed unexpectedly");

    Span<byte> ipaddr = stackalloc byte[LengthOfIPADDR];
    var bytesWritten = 0;

    for (var i = 0; i < lengthOfIPv6Address; i += 2) {
      if (0 < i)
        ipaddr[bytesWritten++] = (byte)':';

      Hexadecimal.TryEncodeUpperCase(addressBytes.Slice(i, 2), ipaddr.Slice(bytesWritten), out var bytesEncoded);

      bytesWritten += bytesEncoded;
    }

    writer.WriteToken(ipaddr);
  }

  public static void WriteToken(this ISkStackCommandLineWriter writer, ReadOnlySpan<char> token)
    => WriteDefaultEncodingToken(
      writer,
      token,
      write: static (t, w) => w.WriteToken(t)
    );

  public static void WriteMaskedToken(this ISkStackCommandLineWriter writer, ReadOnlySpan<char> token)
    => WriteDefaultEncodingToken(
      writer,
      token,
      write: static (t, w) => w.WriteMaskedToken(t)
    );

  private static void WriteDefaultEncodingToken(
    ISkStackCommandLineWriter writer,
    ReadOnlySpan<char> token,
    SpanAction<byte, ISkStackCommandLineWriter> write
  )
  {
    if (token.IsEmpty)
      throw new ArgumentException("cannot be empty", paramName: nameof(token));

    byte[]? tokenBytes = null;

    try {
      tokenBytes = ArrayPool<byte>.Shared.Rent(token.Length);

      var lengthOfToken = SkStack.DefaultEncoding.GetBytes(token, tokenBytes.AsSpan());

      write(tokenBytes.AsSpan(0, lengthOfToken), writer);
    }
    finally {
      if (tokenBytes is not null)
        ArrayPool<byte>.Shared.Return(tokenBytes, clearArray: true);
    }
  }
}
