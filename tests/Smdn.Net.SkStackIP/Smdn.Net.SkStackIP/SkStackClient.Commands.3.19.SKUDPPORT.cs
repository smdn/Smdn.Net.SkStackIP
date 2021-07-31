// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKUDPPORTTests : SkStackClientTestsBase {
    [Test]
    public void SKUDPPORT()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = new SkStackClient(stream, ServiceProvider);
      SkStackResponse response = null;

      Assert.DoesNotThrowAsync(async () => (response, _) = await client.SendSKUDPPORTAsync(SkStackUdpPortHandle.Handle3, 0x0050));

      Assert.IsNotNull(response);
      Assert.IsTrue(response.Success);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKUDPPORT 3 0050\r\n".ToByteSequence())
      );
    }

    [Test]
    public void SKUDPPORTUnset()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = new SkStackClient(stream, ServiceProvider);
      SkStackResponse response = null;

      Assert.DoesNotThrowAsync(async () => response = await client.SendSKUDPPORTUnsetAsync(SkStackUdpPortHandle.Handle3));

      Assert.IsNotNull(response);
      Assert.IsTrue(response.Success);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKUDPPORT 3 0000\r\n".ToByteSequence())
      );
    }

    [TestCase(-1)]
    [TestCase(0x0000)]
    [TestCase(0xFFFF + 1)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void SKUDPPORT_PortOutOfRange(int port)
    {
      var stream = new PseudoSkStackStream();

      using var client = new SkStackClient(stream, ServiceProvider);

      Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTAsync(SkStackUdpPortHandle.Handle1, port));

      Assert.IsEmpty(stream.ReadSentData());
    }

    [TestCase((SkStackUdpPortHandle)0)]
    [TestCase((SkStackUdpPortHandle)7)]
    [TestCase((SkStackUdpPortHandle)0xFF)]
    public void SKUDPPORT_HandleUndefined(SkStackUdpPortHandle handle)
    {
      var stream = new PseudoSkStackStream();

      using var client = new SkStackClient(stream, ServiceProvider);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTAsync(handle, 0x0001));

      Assert.AreEqual(ex.ParamName, "handle");

      Assert.IsEmpty(stream.ReadSentData());
    }

    [TestCase((SkStackUdpPortHandle)0)]
    [TestCase((SkStackUdpPortHandle)7)]
    [TestCase((SkStackUdpPortHandle)0xFF)]
    public void SKUDPPORTUnset_HandleUndefined(SkStackUdpPortHandle handle)
    {
      var stream = new PseudoSkStackStream();

      using var client = new SkStackClient(stream, ServiceProvider);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTUnsetAsync(handle));

      Assert.AreEqual(ex.ParamName, "handle");

      Assert.IsEmpty(stream.ReadSentData());
    }
  }
}
