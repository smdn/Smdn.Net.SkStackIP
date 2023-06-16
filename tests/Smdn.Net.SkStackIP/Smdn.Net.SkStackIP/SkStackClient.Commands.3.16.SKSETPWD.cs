// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKSETPWDTests : SkStackClientTestsBase {
  [Test]
  public void SKSETPWD()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());
    SkStackResponse response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKSETPWDAsync("0123456789AB".AsMemory()));

    Assert.IsNotNull(response);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSETPWD C 0123456789AB\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfChar_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: string.Empty.AsMemory()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfByte_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: ReadOnlyMemory<byte>.Empty));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfChar_TooLong()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: "012345678901234567890123456789012".AsMemory()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfByte_TooLong()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: "012345678901234567890123456789012".ToByteSequence()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }
}
