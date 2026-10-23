// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKRESETTests : SkStackClientTestsBase {
  [Test]
  public void SKRESET()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
      async () => await client.SendSKRESETAsync(),
      Throws.Nothing
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKRESET\r\n".ToByteSequence())
    );
  }
}
