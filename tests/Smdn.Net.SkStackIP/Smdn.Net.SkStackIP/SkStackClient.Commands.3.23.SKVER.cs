// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKVERTests : SkStackClientTestsBase {
  [Test]
  public void SKVER()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("EVER 1.2.10");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, ServiceProvider);
    SkStackResponse<Version> response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKVERAsync());

    Assert.IsNotNull(response, nameof(response));
    Assert.IsNotNull(response!.Payload);
    Assert.AreEqual(new Version(1, 2, 10), response.Payload);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKVER\r\n".ToByteSequence())
    );
  }
}
