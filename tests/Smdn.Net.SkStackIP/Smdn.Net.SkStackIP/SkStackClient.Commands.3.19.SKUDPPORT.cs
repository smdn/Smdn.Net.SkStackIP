// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKUDPPORTTests : SkStackClientTestsBase {
  [Test]
  public void SKUDPPORT()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());
    SkStackResponse response = null;
    SkStackUdpPort port = default;

    Assert.DoesNotThrowAsync(async () => (response, port) = await client.SendSKUDPPORTAsync(SkStackUdpPortHandle.Handle3, 0x0050));

    Assert.IsNotNull(response);
    Assert.IsTrue(response!.Success);

    Assert.IsFalse(port.IsNull, nameof(port.IsNull));
    Assert.IsFalse(port.IsUnused, nameof(port.IsUnused));
    Assert.AreEqual(port.Handle, SkStackUdpPortHandle.Handle3, nameof(port.Handle));
    Assert.AreEqual(port.Port, 0x0050, nameof(port.Port));

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

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());
    SkStackResponse response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKUDPPORTUnsetAsync(SkStackUdpPortHandle.Handle3));

    Assert.IsNotNull(response);
    Assert.IsTrue(response!.Success);

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

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTAsync(SkStackUdpPortHandle.Handle1, port));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase((SkStackUdpPortHandle)0)]
  [TestCase((SkStackUdpPortHandle)7)]
  [TestCase((SkStackUdpPortHandle)0xFF)]
  public void SKUDPPORT_HandleUndefined(SkStackUdpPortHandle handle)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTAsync(handle, 0x0001));
#pragma warning restore CA2012

    Assert.AreEqual(ex!.ParamName, "handle");

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase((SkStackUdpPortHandle)0)]
  [TestCase((SkStackUdpPortHandle)7)]
  [TestCase((SkStackUdpPortHandle)0xFF)]
  public void SKUDPPORTUnset_HandleUndefined(SkStackUdpPortHandle handle)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTUnsetAsync(handle));
#pragma warning restore CA2012

    Assert.AreEqual(ex!.ParamName, "handle");

    Assert.IsEmpty(stream.ReadSentData());
  }
}
