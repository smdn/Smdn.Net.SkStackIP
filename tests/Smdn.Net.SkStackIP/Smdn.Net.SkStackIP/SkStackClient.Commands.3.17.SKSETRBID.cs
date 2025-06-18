// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKSETRBIDTests : SkStackClientTestsBase {
  [Test]
  public void SKSETRBID()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKSETRBIDAsync(id: "00112233445566778899AABBCCDDEEFF".AsMemory()));

    Assert.That(response, Is.Not.Null);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSETRBID 00112233445566778899AABBCCDDEEFF\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSETRBID_RBID_String_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => client.SendSKSETRBIDAsync(id: string.Empty.AsMemory()), Throws.ArgumentException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }


  [Test]
  public void SKSETRBID_RBID_ReadOnlyMemoryOfByte_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => client.SendSKSETRBIDAsync(id: ReadOnlyMemory<byte>.Empty), Throws.ArgumentException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [TestCase("0")]
  [TestCase("00112233445566778899AABBCCDDEEF")]
  [TestCase("00112233445566778899AABBCCDDEEFFF")]
  public void SKSETRBID_InvalidLengthOfRBID_ReadOnlyMemoryOfChar(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => client.SendSKSETRBIDAsync(id: rbid.AsMemory()), Throws.ArgumentException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [TestCase("0")]
  [TestCase("00112233445566778899AABBCCDDEEF")]
  [TestCase("00112233445566778899AABBCCDDEEFFF")]
  public void SKSETRBID_InvalidLengthOfRBID_ReadOnlyMemoryOfByte(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => client.SendSKSETRBIDAsync(id: rbid.ToByteSequence()), Throws.ArgumentException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

#if SYSTEM_TEXT_ASCII
  [TestCase("０0112233445566778899AABBCCDDEEFF")]
  [TestCase("00112233445566778899AABBCCDDEEFＦ")]
  public void SKSETRBID_NonAsciiRBID_ReadOnlyMemoryOfChar(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => client.SendSKSETRBIDAsync(id: rbid.AsMemory()), Throws.ArgumentException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }
#endif

  [Test]
  public void SKSETRBID_ActionOfIBufferWriterOfByte()
  {
    const string RBID = "0123456789ABCDEF0123456789ABCDEF";

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    SkStackResponse? response = null;

    Assert.That(
      async () => response = await client.SendSKSETRBIDAsync(
        writer => writer.Write(RBID.ToByteSequence().Span)
      ),
      Throws.Nothing
    );

    Assert.That(response, Is.Not.Null);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKSETRBID {RBID}\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSETRBID_ActionOfIBufferWriterOfByte_ArgumentNull()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
      () => client.SendSKSETRBIDAsync(writeRBID: null!),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writeRBID")
    );
    Assert.That(
      async () => await client.SendSKSETRBIDAsync(writeRBID: null!),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writeRBID")
    );

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [TestCase("")]
  [TestCase("0123456789ABCDEF0123456789ABCDE")]
  [TestCase("0123456789ABCDEF0123456789ABCDEF0")]
  public void SKSETRBID_ActionOfIBufferWriterOfByte_WrittenBufferLengthInvalid(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
      async () => await client.SendSKSETRBIDAsync(
        writer => writer.Write(rbid.ToByteSequence().Span)
      ),
      Throws.InvalidOperationException
    );

    Assert.That(stream.ReadSentData(), Is.Empty);
  }
}
