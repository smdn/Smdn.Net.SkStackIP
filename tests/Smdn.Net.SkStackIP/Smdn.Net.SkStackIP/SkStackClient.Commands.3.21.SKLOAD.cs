// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKLOADTests : SkStackClientTestsBase {
  [Test]
  public async Task SKLOAD()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, ServiceProvider);
    var response = await client.SendSKLOADAsync();

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKLOAD\r\n".ToByteSequence())
    );

    Assert.IsTrue(response.Success);
  }

  [Test]
  public void SKLOAD_FAIL_ER10()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER10");

    using var client = new SkStackClient(stream, ServiceProvider);

    var ex = Assert.ThrowsAsync<SkStackFlashMemoryIOException>(async () => await client.SendSKLOADAsync());

    Assert.AreEqual(ex!.ErrorCode, SkStackErrorCode.ER10);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKLOAD\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKLOAD_FAIL_ERXX()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER01");

    using var client = new SkStackClient(stream, ServiceProvider);

    var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(async () => await client.SendSKLOADAsync());

    Assert.AreEqual(ex!.ErrorCode, SkStackErrorCode.ER01);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKLOAD\r\n".ToByteSequence())
    );
  }
}
