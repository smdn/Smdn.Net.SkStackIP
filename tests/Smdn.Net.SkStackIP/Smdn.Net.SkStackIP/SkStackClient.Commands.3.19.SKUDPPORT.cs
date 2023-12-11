// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKUDPPORTTests : SkStackClientTestsBase {
  [Test]
  public void SKUDPPORT()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse? response = null;
    SkStackUdpPort port = default;

    Assert.DoesNotThrowAsync(async () => (response, port) = await client.SendSKUDPPORTAsync(SkStackUdpPortHandle.Handle3, 0x0050));

    Assert.That(response, Is.Not.Null);
    Assert.That(response!.Success, Is.True);

    Assert.That(port.IsNull, Is.False, nameof(port.IsNull));
    Assert.That(port.IsUnused, Is.False, nameof(port.IsUnused));
    Assert.That(port.Handle, Is.EqualTo(SkStackUdpPortHandle.Handle3), nameof(port.Handle));
    Assert.That(port.Port, Is.EqualTo(0x0050), nameof(port.Port));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKUDPPORT 3 0050\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKUDPPORTUnset()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKUDPPORTUnsetAsync(SkStackUdpPortHandle.Handle3));

    Assert.That(response, Is.Not.Null);
    Assert.That(response!.Success, Is.True);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKUDPPORT 3 0000\r\n".ToByteSequence())
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

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTAsync(SkStackUdpPortHandle.Handle1, port));
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [TestCase((SkStackUdpPortHandle)0)]
  [TestCase((SkStackUdpPortHandle)7)]
  [TestCase((SkStackUdpPortHandle)0xFF)]
  public void SKUDPPORT_HandleUndefined(SkStackUdpPortHandle handle)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTAsync(handle, 0x0001));
#pragma warning restore CA2012

    Assert.That(ex!.ParamName, Is.EqualTo("handle"));

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [TestCase((SkStackUdpPortHandle)0)]
  [TestCase((SkStackUdpPortHandle)7)]
  [TestCase((SkStackUdpPortHandle)0xFF)]
  public void SKUDPPORTUnset_HandleUndefined(SkStackUdpPortHandle handle)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendSKUDPPORTUnsetAsync(handle));
#pragma warning restore CA2012

    Assert.That(ex!.ParamName, Is.EqualTo("handle"));

    Assert.That(stream.ReadSentData(), Is.Empty);
  }
}
