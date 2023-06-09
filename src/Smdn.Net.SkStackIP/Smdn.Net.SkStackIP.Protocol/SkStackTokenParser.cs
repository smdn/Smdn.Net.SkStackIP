// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;

using Smdn.Buffers;
using Smdn.Formats;
using Smdn.Text.Unicode.ControlPictures;

namespace Smdn.Net.SkStackIP.Protocol;

public static class SkStackTokenParser {
  private delegate (bool Success, TResult Result) TryConvertTokenFunc<TArg, TResult>(ReadOnlySequence<byte> token, TArg arg);

  private static OperationStatus TryConvertToken<TArg, TResult>(
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
    if (0 < length && reader.Remaining < length)
      return OperationStatus.NeedMoreData;

    var readerEOL = reader;
    var readerSP = reader;
    ReadOnlySequence<byte> tokenDelimitByEOL;
    ReadOnlySequence<byte> tokenDelimitBySP;
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
      return OperationStatus.NeedMoreData;

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
      return throwIfUnexpected
        ? throw SkStackUnexpectedResponseException.CreateInvalidToken(
            token,
            $"invalid length of token (expected {length} but was {token.Length})"
          )
        : OperationStatus.InvalidData;
    }

    bool converted;

    (converted, result) = tryConvert(token, arg);

    if (!converted)
      return OperationStatus.InvalidData;

    reader = consumedReader;

    return OperationStatus.Done;
  }

  public static bool Expect<TValue>(
    ref SequenceReader<byte> reader,
    int length,
    Converter<ReadOnlySequence<byte>, TValue> converter,
    out TValue value
  )
    => OperationStatus.Done == TryConvertToken(
      reader: ref reader,
      length: length,
      throwIfUnexpected: true,
      arg: converter,
      tryConvert: static (token, conv) => (true, conv(token)),
      result: out value
    );

  public static OperationStatus TryExpectToken(
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
    => OperationStatus.Done == ExpectTokenCore(
      reader: ref reader,
      expectedToken: expectedToken,
      throwIfUnexpected: true
    );

  private static OperationStatus ExpectTokenCore(
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
              return arg.throwIfUnexpected
                ? throw SkStackUnexpectedResponseException.CreateInvalidToken(seq, "unexpected token")
                : (false, default(int));
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
    if (reader.Remaining < expectedSequence.Length)
      return false; // incomplete

    var consumedReader = reader;

    if (!consumedReader.IsNext(expectedSequence.Span, advancePast: true)) {
      return throwIfUnexpected
        ? throw SkStackUnexpectedResponseException.CreateInvalidToken(
            consumedReader.GetUnreadSequence().Slice(0, expectedSequence.Length),
            createUnexpectedExceptionMessage(expectedSequence)
          )
        : false;
    }

    reader = consumedReader;

    return true;
  }

  public static bool ExpectCharArray(
    ref SequenceReader<byte> reader,
    out ReadOnlyMemory<byte> value
  )
    => Expect(
      reader: ref reader,
      length: 0,
      converter: static seq => seq.ToArray().AsMemory(),
      value: out value
    );

  public static bool ExpectCharArray(
    ref SequenceReader<byte> reader,
    out string value
  )
    => Expect(
      reader: ref reader,
      length: 0,
      converter: SkStack.GetString,
      value: out value
    );

  [CLSCompliant(false)]
  public static bool ExpectDecimalNumber(
    ref SequenceReader<byte> reader,
    out uint value
  )
    => Expect(ref reader, 0, ToDecimalNumber, out value);

  [CLSCompliant(false)]
  public static bool ExpectDecimalNumber(
    ref SequenceReader<byte> reader,
    int length,
    out uint value
  )
    => Expect(ref reader, length, ToDecimalNumber, out value);

  public static bool ExpectUINT8(
    ref SequenceReader<byte> reader,
    out byte value
  )
    => Expect(ref reader, length: 1 * 2, ToUINT8, out value);

  [CLSCompliant(false)]
  public static bool ExpectUINT16(
    ref SequenceReader<byte> reader,
    out ushort value
  )
    => Expect(ref reader, length: 2 * 2, ToUINT16, out value);

  [CLSCompliant(false)]
  public static bool ExpectUINT32(
    ref SequenceReader<byte> reader,
    out uint value
  )
    => Expect(ref reader, length: 4 * 2, ToUINT32, out value);

  [CLSCompliant(false)]
  public static bool ExpectUINT64(
    ref SequenceReader<byte> reader,
    out ulong value
  )
    => Expect(ref reader, length: 8 * 2, ToUINT64, out value);

  public static bool ExpectBinary(
    ref SequenceReader<byte> reader,
    out bool value
  )
    => Expect(ref reader, length: 1, ToBinary, out value);

  private static bool ExpectUINT8Array<TValue>(
    ref SequenceReader<byte> reader,
    int length,
    Converter<Memory<byte>, TValue> converter,
    out TValue value
  )
  {
    value = default;

    byte[] buffer = null;

    try {
      var lengthOfUINT8Array = length * 2;

      buffer = ArrayPool<byte>.Shared.Rent(lengthOfUINT8Array);

      return OperationStatus.Done == TryConvertToken(
        reader: ref reader,
        length: lengthOfUINT8Array,
        throwIfUnexpected: true,
        arg: (converter, memory: buffer.AsMemory(0, length)),
        tryConvert: static (token, arg) => {
          ToByteSequence(token, arg.memory.Length, arg.memory.Span);

          return (true, arg.converter(arg.memory));
        },
        result: out value
      );
    }
    finally {
      if (buffer is not null)
        ArrayPool<byte>.Shared.Return(buffer);
    }
  }

  public static bool ExpectIPADDR(
    ref SequenceReader<byte> reader,
    out IPAddress value
  )
    => Expect(
      reader: ref reader,
      length: 39, // "XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX".Length
      converter: static seq => IPAddress.Parse(SkStack.GetString(seq)),
      value: out value
    );

  public static bool ExpectADDR64(
    ref SequenceReader<byte> reader,
    out PhysicalAddress value
  )
    => ExpectUINT8Array(
      reader: ref reader,
      length: 8,
      converter: static array => new PhysicalAddress(array.ToArray()), // XXX: cannot pass ReadOnlySpan<byte>
      value: out value
    );

  [CLSCompliant(false)]
  public static bool ExpectADDR16(
    ref SequenceReader<byte> reader,
    out ushort value
  )
    => ExpectUINT16(
      reader: ref reader,
      value: out value
    );

  public static bool ExpectCHANNEL(
    ref SequenceReader<byte> reader,
    out SkStackChannel value
  )
  {
    value = default;

    if (ExpectUINT8(ref reader, out var ch)) {
      value = SkStackChannel.FindByChannelNumber(ch);
      return true;
    }

    return false;
  }

  public static void ToByteSequence(ReadOnlySequence<byte> hexTextSequence, int byteSequenceLength, Span<byte> destination)
  {
    if ((hexTextSequence.Length & 0x1L) != 0L)
      throw SkStackUnexpectedResponseException.CreateInvalidToken(hexTextSequence, "HEX ASCII");
    if (destination.Length < byteSequenceLength)
      throw new ArgumentException($"buffer too short. expected at least {byteSequenceLength} but was {destination.Length}.", nameof(destination));

    var reader = new SequenceReader<byte>(hexTextSequence);

    Span<byte> hexTextOneByte = stackalloc byte[2];

    for (var index = 0; index < byteSequenceLength; index++) {
      reader.TryCopyTo(hexTextOneByte);

      if (!Hexadecimal.TryDecode(hexTextOneByte, out var decodedbyte))
        throw SkStackUnexpectedResponseException.CreateInvalidToken(hexTextOneByte.Slice(0, 1), "HEX ASCII");

      destination[index] = decodedbyte;

      reader.Advance(2);
    }
  }

  private static byte ToUINT8(ReadOnlySequence<byte> token)
  {
    try {
      Span<byte> uint8 = stackalloc byte[1];

      ToByteSequence(token, 1, uint8);

      return uint8[0];
    }
    catch (SkStackUnexpectedResponseException ex) {
      throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "UINT8", ex);
    }
  }

  private static ushort ToUINT16(ReadOnlySequence<byte> token)
  {
    try {
      Span<byte> uint16 = stackalloc byte[2];

      ToByteSequence(token, 2, uint16);

      return BinaryPrimitives.ReadUInt16BigEndian(uint16);
    }
    catch (Exception ex) {
      throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "UINT16", ex);
    }
  }

  private static uint ToUINT32(ReadOnlySequence<byte> token)
  {
    try {
      Span<byte> uint32 = stackalloc byte[4];

      ToByteSequence(token, 4, uint32);

      return BinaryPrimitives.ReadUInt32BigEndian(uint32);
    }
    catch (Exception ex) {
      throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "UINT32", ex);
    }
  }

  private static ulong ToUINT64(ReadOnlySequence<byte> token)
  {
    try {
      Span<byte> uint64 = stackalloc byte[8];

      ToByteSequence(token, 8, uint64);

      return BinaryPrimitives.ReadUInt64BigEndian(uint64);
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
      return uint.Parse(str, provider: null);
    }
    catch (Exception ex) {
      throw SkStackUnexpectedResponseException.CreateInvalidToken(token, "decimal number", ex);
    }
  }

  private static bool ToBinary(ReadOnlySequence<byte> token)
  {
    try {
      return token.FirstSpan[0] switch {
        (byte)'0' => false,
        (byte)'1' => true,
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
