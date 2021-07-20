// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKERASETests : SkStackClientCommandsTestsBase {
    [Test]
    public async Task SKERASE()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, ServiceProvider);
      var response = await client.SendSKERASEAsync();

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKERASE\r\n".ToByteSequence())
      );

      Assert.IsTrue(response.Success);
    }
  }
}