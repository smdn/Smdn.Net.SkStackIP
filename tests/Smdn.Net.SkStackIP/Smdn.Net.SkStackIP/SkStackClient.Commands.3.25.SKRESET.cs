// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKRESETTests : SkStackClientTestsBase {
    [Test]
    public void SKRESET()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = new SkStackClient(stream, ServiceProvider);

      Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKRESET\r\n".ToByteSequence())
      );
    }
  }
}