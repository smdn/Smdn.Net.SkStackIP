// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

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

    Assert.IsNotNull(response);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSETRBID 00112233445566778899AABBCCDDEEFF\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSETRBID_RBID_String_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(id: string.Empty.AsMemory()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }


  [Test]
  public void SKSETRBID_RBID_ReadOnlyMemoryOfByte_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(id: ReadOnlyMemory<byte>.Empty));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase("0")]
  [TestCase("00112233445566778899AABBCCDDEEF")]
  [TestCase("00112233445566778899AABBCCDDEEFFF")]
  public void SKSETRBID_InvalidLengthOfRBID_ReadOnlyMemoryOfChar(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(id: rbid.AsMemory()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }


  [TestCase("0")]
  [TestCase("00112233445566778899AABBCCDDEEF")]
  [TestCase("00112233445566778899AABBCCDDEEFFF")]
  public void SKSETRBID_InvalidLengthOfRBID_ReadOnlyMemoryOfByte(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(id: rbid.ToByteSequence()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }
}
