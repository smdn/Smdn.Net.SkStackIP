// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKAPPVERTests : SkStackClientTestsBase {
  [Test]
  public void SKAPPVER()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("EAPPVER rev26e");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<string>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKAPPVERAsync());

    Assert.That(response, Is.Not.Null, nameof(response));
    Assert.That(response!.Payload, Is.Not.Null);
    Assert.That(response.Payload, Is.EqualTo("rev26e"));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKAPPVER\r\n".ToByteSequence())
    );
  }
}
