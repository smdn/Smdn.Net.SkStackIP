// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKVERTests : SkStackClientTestsBase {
  [Test]
  public void SKVER()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("EVER 1.2.10");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<Version>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKVERAsync());

    Assert.That(response, Is.Not.Null, nameof(response));
    Assert.That(response!.Payload, Is.Not.Null);
    Assert.That(response.Payload, Is.EqualTo(new Version(1, 2, 10)));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKVER\r\n".ToByteSequence())
    );
  }
}
