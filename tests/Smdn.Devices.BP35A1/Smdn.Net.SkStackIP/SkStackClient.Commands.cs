// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NUnit.Framework;

using Is = Smdn.NUnitExtensions.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsTests {
    [Test]
    public async Task Command_SKINFO()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("EINFO FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678 21 8888 FFFE");
      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream);
      var ret = await client.SendSKINFOAsync();

      Assert.AreEqual(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), ret.linkLocalAddress, nameof(ret.linkLocalAddress));
      Assert.AreEqual(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 }), ret.macAddress, nameof(ret.macAddress));
      Assert.AreEqual(SkStackChannel.Channel33, ret.channel, nameof(ret.channel));
      Assert.AreEqual(0x8888, ret.panId, nameof(ret.panId));
      Assert.AreEqual(0xFFFE, ret.addr16, nameof(ret.addr16));
    }

    [Test]
    public void Command_SKTERM()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream);

      Assert.DoesNotThrowAsync(async () => await client.SendSKTERMAsync());
    }

    [Test]
    public void Command_SKTERM_FAIL()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL ER10");

      using var client = SkStackClient.Create(stream);

      Assert.ThrowsAsync<SkStackErrorResponseException>(
        async () => await client.SendSKTERMAsync()
      );
    }

    [Test]
    public void Command_SKVER()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("EVER 1.2.10");
      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream);
      Version version = null;

      Assert.DoesNotThrowAsync(async () => version = await client.SendSKVERAsync());

      Assert.AreEqual(new Version(1, 2, 10), version);
    }

    [Test]
    public void Command_SKAPPVER()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("EAPPVER rev26e");
      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream);
      string version = null;

      Assert.DoesNotThrowAsync(async () => version = await client.SendSKAPPVERAsync());

      Assert.AreEqual("rev26e", version);
    }

    [Test]
    public void Command_SKRESET()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream);

      Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());
    }
  }
}