// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKLOADTests : SkStackClientTestsBase {
  [Test]
  public async Task SKLOAD()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    var response = await client.SendSKLOADAsync();

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKLOAD\r\n".ToByteSequence())
    );

    Assert.That(response.Success, Is.True);
  }

  [Test]
  public void SKLOAD_FAIL_ER10()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER10");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<SkStackFlashMemoryIOException>(async () => await client.SendSKLOADAsync());

    Assert.That(ex!.ErrorCode, Is.EqualTo(SkStackErrorCode.ER10));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKLOAD\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKLOAD_FAIL_ERXX()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER01");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(async () => await client.SendSKLOADAsync());

    Assert.That(ex!.ErrorCode, Is.EqualTo(SkStackErrorCode.ER01));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKLOAD\r\n".ToByteSequence())
    );
  }
}
