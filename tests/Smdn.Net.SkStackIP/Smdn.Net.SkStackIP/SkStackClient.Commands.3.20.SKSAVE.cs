// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKSAVETests : SkStackClientTestsBase {
  [Test]
  public async Task SKSAVE()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    var response = await client.SendSKSAVEAsync();

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSAVE\r\n".ToByteSequence())
    );

    Assert.IsTrue(response.Success);
  }

  [Test]
  public void SKSAVE_FAIL_ER10()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER10");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<SkStackFlashMemoryIOException>(async () => await client.SendSKSAVEAsync());

    Assert.AreEqual(ex!.ErrorCode, SkStackErrorCode.ER10);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSAVE\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSAVE_FAIL_ERXX()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER01");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(async () => await client.SendSKSAVEAsync());

    Assert.AreEqual(ex!.ErrorCode, SkStackErrorCode.ER01);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSAVE\r\n".ToByteSequence())
    );
  }
}
