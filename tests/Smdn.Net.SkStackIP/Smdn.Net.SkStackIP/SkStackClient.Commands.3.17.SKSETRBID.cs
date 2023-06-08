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

    using var client = new SkStackClient(stream, ServiceProvider);
    SkStackResponse response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKSETRBIDAsync(routeBID: "00112233445566778899AABBCCDDEEFF"));

    Assert.IsNotNull(response);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSETRBID 00112233445566778899AABBCCDDEEFF\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSETRBID_RBID_String_Null()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

    Assert.Throws<ArgumentNullException>(() => client.SendSKSETRBIDAsync(routeBID: (string)null));

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSETRBID_RBID_String_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(routeBID: string.Empty));

    Assert.IsEmpty(stream.ReadSentData());
  }


  [Test]
  public void SKSETRBID_RBID_ReadOnlyByteMemory_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(routeBID: ReadOnlyMemory<byte>.Empty));

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase("0")]
  [TestCase("00112233445566778899AABBCCDDEEF")]
  [TestCase("00112233445566778899AABBCCDDEEFFF")]
  public void SKSETRBID_InvalidLengthOfRBID_String(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(routeBID: rbid));

    Assert.IsEmpty(stream.ReadSentData());
  }


  [TestCase("0")]
  [TestCase("00112233445566778899AABBCCDDEEF")]
  [TestCase("00112233445566778899AABBCCDDEEFFF")]
  public void SKSETRBID_InvalidLengthOfRBID_ReadOnlyByteMemory(string rbid)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, ServiceProvider);

    Assert.Throws<ArgumentException>(() => client.SendSKSETRBIDAsync(routeBID: rbid.ToByteSequence()));

    Assert.IsEmpty(stream.ReadSentData());
  }
}
