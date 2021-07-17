// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;

using Smdn.Buffers;
using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP.Protocol {
  public static class SkStackTokenParser {
    private delegate (bool, TResult) TryConvertTokenFunc<TArg, TResult>(ReadOnlySequence<byte> token, TArg arg);

    private static bool TryConvertToken<TArg, TResult>(
      ref SequenceReader<byte> reader,
      int length,
      bool throwIfUnexpected,
      TArg arg,
      TryConvertTokenFunc<TArg, TResult> tryConvert,
      out TResult result
    )
    {
      result = default;

      // if fixed length
      if (0 < length && reader.GetUnreadSequence().Length < length)
        return false; // incomplete

      var readerEOL = reader;
      var readerSP = reader;
      ReadOnlySequence<byte> tokenDelimitByEOL = default;
      ReadOnlySequence<byte> tokenDelimitBySP = default;
      var foundTokenDelimitByEOL = readerEOL.TryReadTo(out tokenDelimitByEOL, SkStack.CRLFSpan);
      var foundTokenDelimitBySP = readerSP.TryReadTo(out tokenDelimitBySP, SkStack.SP);

      if (foundTokenDelimitByEOL && foundTokenDelimitBySP) {
        // select shorter one
        if (tokenDelimitByEOL.Length < tokenDelimitBySP.Length) {
          foundTokenDelimitByEOL = true;
          foundTokenDelimitBySP = false;
        }
        else {
          foundTokenDelimitBySP = true;
          foundTokenDelimitByEOL = false;
        }
      }

      if (!(foundTokenDelimitByEOL || foundTokenDelimitBySP))
        return false; // incomplete

      ReadOnlySequence<byte> token;
      var consumedReader = reader;

      if (foundTokenDelimitByEOL) {
        token = tokenDelimitByEOL;
        consumedReader.Advance(tokenDelimitByEOL.Length); // keep EOL
      }
      else {
        token = tokenDelimitBySP;
        consumedReader.Advance(tokenDelimitBySP.Length + 1); // consume SP
      }

      if (0 < length && token.Length != length) {
        if (throwIfUnexpected) {
          throw SkStackUnexpectedResponseException.CreateInvalidToken(
            token,
            $"invalid length of token (expected {length} but was {token.Length})"
          );
        }
        else {
          return false;
        }
      }

      bool converted;

      (converted, result) = tryConvert(token, arg);

      if (!converted)
        return false;

      reader = consumedReader;

      return true;
    }

    public static bool Expect<TResult>(
      ref SequenceReader<byte> reader,
      int length,
      Converter<ReadOnlySequence<byte>, TResult> converter,
      out TResult result
    )
      => TryConvertToken(
        reader: ref reader,
        length: length,
        throwIfUnexpected: true,
        arg: converter,
        tryConvert: static (token, conv) => (true, conv(token)),
        result: out result
      );

    public static bool TryExpectToken(
      ref SequenceReader<byte> reader,
      ReadOnlyMemory<byte> expectedToken
    )
      => ExpectTokenCore(
        reader: ref reader,
        expectedToken: expectedToken,
        throwIfUnexpected: false
      );

    public static bool ExpectToken(
      ref SequenceReader<byte> reader,
      ReadOnlyMemory<byte> expectedToken
    )
      => ExpectTokenCore(
        reader: ref reader,
        expectedToken: expectedToken,
        throwIfUnexpected: true
      );

    private static bool ExpectTokenCore(
      ref SequenceReader<byte> reader,
      ReadOnlyMemory<byte> expectedToken,
      bool throwIfUnexpected
    )
    {
      try {
        return TryConvertToken(
          reader: ref reader,
          length: expectedToken.Length,
          throwIfUnexpected: throwIfUnexpected,
          arg: (expectedToken, throwIfUnexpected),
          tryConvert: static (seq, arg) => {
            var r = new SequenceReader<byte>(seq);

            for (var i = 0; i < arg.expectedToken.Length; i++) {
              if (r.TryRead(out var b) && (char)b != arg.expectedToken.Span[i]) {
                if (arg.throwIfUnexpected)
                  throw SkStackUnexpectedResponseException.CreateInvalidToken(seq, "unexpected token");
                return (false, default(int));
              }
            }

            return (true, default(int));
          },
          result: out var _ // discard
        );
      }
      catch (SkStackUnexpectedResponseException ex) {
        throw SkStackUnexpectedResponseException.CreateInvalidToken(causedText: ex.CausedText, $"expected: '{expectedToken.Span.ToControlCharsPicturizedString()}'", ex);
      }
    }

    public static bool ExpectEndOfLine(
      ref SequenceReader<byte> reader
    )
      => ExpectSequenceCore(
        reader: ref reader,
        expectedSequence: SkStack.CRLFMemory,
        throwIfUnexpected: true,
        createUnexpectedExceptionMessage: static _ => $"expected EOL, but not"
      );

    public static bool ExpectSequence(
      ref SequenceReader<byte> reader,
      ReadOnlyMemory<byte> expectedSequence
    )
      => ExpectSequenceCore(
        reader: ref reader,
        expectedSequence: expectedSequence,
        throwIfUnexpected: true,
        createUnexpectedExceptionMessage: static seq => $"expected sequence '{seq.Span.ToControlCharsPicturizedString()}', but not"
      );

    private static bool ExpectSequenceCore(
      ref SequenceReader<byte> reader,
      ReadOnlyMemory<byte> expectedSequence,
      bool throwIfUnexpected,
      Func<ReadOnlyMemory<byte>, string> createUnexpectedExceptionMessage
    )
    {
      if (reader.GetUnreadSequence().Length < expectedSequence.Length)
        return false; // incomplete

      var consumedReader = reader;

      if (!consumedReader.IsNext(expectedSequence.Span, advancePast: true)) {
        if (throwIfUnexpected) {
          throw SkStackUnexpectedResponseException.CreateInvalidToken(
            consumedReader.GetUnreadSequence().Slice(0, expectedSequence.Length),
            createUnexpectedExceptionMessage(expectedSequence)
          );
        }
        else {
          return false;
        }
      }

      reader = consumedReader;

      return true;
    }

    public static bool ExpectCharArray(
      ref SequenceReader<byte> reader,
      out ReadOnlyMemory<byte> charArray
    )
      => Expect(
        reader: ref reader,
        length: 0,
        converter: static seq => seq.ToArray().AsMemory(),
        result: out charArray
      );

    public static bool ExpectCharArray(
      ref SequenceReader<byte> reader,
      out string charArray
    )
      => Expect(
        reader: ref reader,
        length: 0,
        converter: SkStack.GetString,
        result: out charArray
      );

    [CLSCompliant(false)]
    public static bool ExpectDecimalNumber(
      ref SequenceReader<byte> reader,
      out uint number
    )
      => Expect(ref reader, 0, ToDecimalNumber, out number);

    [CLSCompliant(false)]
    public static bool ExpectDecimalNumber(
      ref SequenceReader<byte> reader,
      int length,
      out uint number
    )
      => Expect(ref reader, length, ToDecimalNumber, out number);

    public static bool ExpectUINT8(
      ref SequenceReader<byte> reader,
      out byte uint8
    )
      => Expect(ref reader, length: 1 * 2, ToUINT8, out uint8);

    [CLSCompliant(false)]
    public static bool ExpectUINT16(
      ref SequenceReader<byte> reader,
      out ushort uint16
    )
      => Expect(ref reader, length: 2 * 2, ToUINT16, out uint16);

    [CLSCompliant(false)]
    public static bool ExpectUINT32(
      ref SequenceReader<byte> reader,
      out uint uint32
    )
      => Expect(ref reader, length: 4 * 2, ToUINT32, out uint32);

    [CLSCompliant(false)]
    public static bool ExpectUINT64(
      ref SequenceReader<byte> reader,
      out ulong uint64
    )
      => Expect(ref reader, length: 8 * 2, ToUINT64, out uint64);

    public static bool ExpectBinary(
      ref SequenceReader<byte> reader,
      out bool binary
    )
      => Expect(ref reader, length: 1, ToBinary, out binary);

    private static bool ExpectUINT8Array<TResult>(
      ref SequenceReader<byte> reader,
      int length,
      Converter<Memory<byte>, TResult> converter,
      out TResult result
    )
    {
      result = default;

      byte[] buffer = null;

      try {
        var lengthOfUINT8Array = length * 2;

        buffer = ArrayPool<byte>.Shared.Rent(lengthOfUINT8Array);

        return TryConvertToken(
          reader: ref reader,
          length: lengthOfUINT8Array,
          throwIfUnexpected: true,
          arg: (converter: converter, memory: buffer.AsMemory(0, length)),
          tryConvert: static (token, arg) => {
            var span = arg.memory.Span;

            for (var i = 0; i < span.Length; i++) {
              span[i] = ToUINT8(token.Slice(i * 2, 2));
            }

            return (true, arg.converter(arg.memory));
          },
          result: out result
        );
      }
      finally {
        if (buffer is not null)
          ArrayPool<byte>.Shared.Return(buffer);
      }
    }

    public static bool ExpectIPADDR(
      ref SequenceReader<byte> reader,
      out IPAddress ipv6address
    )
      => Expect(
        reader: ref reader,
        length: 39, // "XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX".Length
        converter: static seq => IPAddress.Parse(SkStack.GetString(seq)),
        result: out ipv6address
      );

    public static bool ExpectADDR64(
      ref SequenceReader<byte> reader,
      out PhysicalAddress macAddress
    )
      => ExpectUINT8Array(
        reader: ref reader,
        length: 8,
        converter: static array => new PhysicalAddress(array.ToArray()), // XXX: cannot pass ReadOnlySpan<byte>
        result: out macAddress
      );

    [CLSCompliant(false)]
    public static bool ExpectADDR16(
      ref SequenceReader<byte> reader,
      out ushort addr16
    )
      => ExpectUINT16(
        reader: ref reader,
        uint16: out addr16
      );

    public static bool ExpectCHANNEL(
      ref SequenceReader<byte> reader,
      out SkStackChannel channel
    )
    {
      channel = default;

      if (ExpectUINT8(ref reader, out var ch)) {
        channel = SkStackChannel.FindByChannelNumber(ch);
        return true;
      }

      return false;
    }

    private static byte FromHex(byte b)
    {
      if ((byte)'0' <= b && b <= (byte)'9')
        return (byte)(b - '0');
      if ((byte)'A' <= b && b <= (byte)'F')
        return (byte)(0xA + b - 'A');
      if ((byte)'a' <= b && b <= (byte)'a')
        return (byte)(0xA + b - 'a');

      throw SkStackUnexpectedResponseException.CreateInvalidToken(stackalloc byte[1] { b }, "HEX");
    }

    private static byte ToUINT8(ReadOnlySequence<byte> token)
    {
      var reader = new SequenceReader<byte>(token);

      try {
        Span<byte> hex = stackalloc byte[2];

        reader.TryCopyTo(hex);

        return (byte)(
          (FromHex(hex[0]) << 4) |
           FromHex(hex[1])
        );
      }
      catch (SkStackUnexpectedResponseException ex) {
        throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "UINT8", ex);
      }
    }

    private static ushort ToUINT16(ReadOnlySequence<byte> token)
    {
      var reader = new SequenceReader<byte>(token);

      try {
        Span<byte> hex = stackalloc byte[4];

        reader.TryCopyTo(hex);

        return (ushort)(
          (FromHex(hex[0]) << 12) |
          (FromHex(hex[1]) << 8) |
          (FromHex(hex[2]) << 4) |
           FromHex(hex[3])
        );
      }
      catch (Exception ex) {
        throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "UINT16", ex);
      }
    }

    private static uint ToUINT32(ReadOnlySequence<byte> token)
    {
      var reader = new SequenceReader<byte>(token);

      try {
        Span<byte> hex = stackalloc byte[8];

        reader.TryCopyTo(hex);

        return (uint)(
          ((uint)FromHex(hex[0]) << 28) |
          ((uint)FromHex(hex[1]) << 24) |
          ((uint)FromHex(hex[2]) << 20) |
          ((uint)FromHex(hex[3]) << 16) |
          ((uint)FromHex(hex[4]) << 12) |
          ((uint)FromHex(hex[5]) << 8) |
          ((uint)FromHex(hex[6]) << 4) |
           (uint)FromHex(hex[7])
        );
      }
      catch (Exception ex) {
        throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "UINT32", ex);
      }
    }

    private static ulong ToUINT64(ReadOnlySequence<byte> token)
    {
      var reader = new SequenceReader<byte>(token);

      try {
        Span<byte> hex = stackalloc byte[16];

        reader.TryCopyTo(hex);

        return (ulong)(
          ((ulong)FromHex(hex[ 0]) << 60) |
          ((ulong)FromHex(hex[ 1]) << 56) |
          ((ulong)FromHex(hex[ 2]) << 52) |
          ((ulong)FromHex(hex[ 3]) << 48) |
          ((ulong)FromHex(hex[ 4]) << 44) |
          ((ulong)FromHex(hex[ 5]) << 40) |
          ((ulong)FromHex(hex[ 6]) << 36) |
          ((ulong)FromHex(hex[ 7]) << 32) |
          ((ulong)FromHex(hex[ 8]) << 28) |
          ((ulong)FromHex(hex[ 9]) << 24) |
          ((ulong)FromHex(hex[10]) << 20) |
          ((ulong)FromHex(hex[11]) << 16) |
          ((ulong)FromHex(hex[12]) << 12) |
          ((ulong)FromHex(hex[13]) << 8) |
          ((ulong)FromHex(hex[14]) << 4) |
           (ulong)FromHex(hex[15])
        );
      }
      catch (Exception ex) {
        throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "UINT32", ex);
      }
    }

    private static uint ToDecimalNumber(ReadOnlySequence<byte> token)
    {
      const int maxLength = 10; // uint.MaxValue.ToString("D").Length

      if (maxLength < token.Length)
        throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "decimal number with max 10 digit");

      var reader = new SequenceReader<byte>(token);
      Span<char> str = stackalloc char[(int)reader.Length];

      for (var i = 0; i < token.Length; i++) {
        reader.TryRead(out var d);
        str[i] = (char)d;
      }

      try {
        return uint.Parse(str);
      }
      catch (Exception ex) {
        throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "decimal number", ex);
      }
    }

    private static bool ToBinary(ReadOnlySequence<byte> token)
    {
      try {
        return FromHex(token.FirstSpan[0]) switch {
          0 => false,
          1 => true,
          _ => throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "0 or 1"),
        };
      }
      catch (SkStackUnexpectedResponseException ex) {
        throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "Binary", ex);
      }
    }

    public static bool TryExpectStatusLine(
      ref SequenceReader<byte> reader,
      out SkStackResponseStatus status
    )
    {
      status = default;

      var readerOk = reader;
      var readerFail = reader;

      if (
        readerOk.IsNext(SkStackResponseStatusCodes.OK, advancePast: true) &&
        (readerOk.IsNext(SkStack.SP, advancePast: true) || readerOk.IsNext(SkStack.CRLFSpan, advancePast: true))
      ) {
        reader = readerOk;
        status = SkStackResponseStatus.Ok;
        return true;
      }
      else if (
        readerFail.IsNext(SkStackResponseStatusCodes.FAIL, advancePast: true) &&
        (readerFail.IsNext(SkStack.SP, advancePast: true) || readerFail.IsNext(SkStack.CRLFSpan, advancePast: true))
      ) {
        reader = readerFail;
        status = SkStackResponseStatus.Fail;
        return true;
      }
      else {
        return false; // is incomplete or is not status line
      }
    }
  }
}