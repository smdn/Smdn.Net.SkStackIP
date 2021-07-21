// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKAPPVERTests : SkStackClientTestsBase {
    [Test]
    public void SKAPPVER()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("EAPPVER rev26e");
      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, ServiceProvider);
      SkStackResponse<string> response = null;

      Assert.DoesNotThrowAsync(async () => response = await client.SendSKAPPVERAsync());

      Assert.IsNotNull(response.Payload);
      Assert.AreEqual("rev26e", response.Payload);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKAPPVER\r\n".ToByteSequence())
      );
    }
  }
}