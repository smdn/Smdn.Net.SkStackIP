// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.NetworkInformation;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKADDNBRTests : SkStackClientTestsBase {
  [Test]
  public void SKADDNBR()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());
    SkStackResponse response = null;

    Assert.DoesNotThrowAsync(async () =>
      response = await client.SendSKADDNBRAsync(
        ipv6Address: new IPAddress(new byte[] { 0xFE, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 }),
        macAddress: new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 })
      )
    );

    Assert.IsNotNull(response);
    Assert.IsTrue(response!.Success);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKADDNBR FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKADDNBR_IPADDR_Null()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(() =>
      client.SendSKADDNBRAsync(
        ipv6Address: null!,
        macAddress: new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 })
      )
    );
#pragma warning restore CA2012
  }

  [Test]
  public void SKADDNBR_IPADDR_InvalidAddressFamily()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() =>
      client.SendSKADDNBRAsync(
        ipv6Address: IPAddress.Loopback,
        macAddress: new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 })
      )
    );
#pragma warning restore CA2012
  }

  [Test]
  public void SKADDNBR_MACADDR_Null()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(() =>
      client.SendSKADDNBRAsync(
        ipv6Address: new IPAddress(new byte[] { 0xFE, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 }),
        macAddress: null!
      )
    );
#pragma warning restore CA2012
  }

  [Test] public void SKADDNBR_MACADDR_InvalidLength_0() => SKADDNBR_MACADDR_InvalidLength(PhysicalAddress.None);
  [Test] public void SKADDNBR_MACADDR_InvalidLength_1() => SKADDNBR_MACADDR_InvalidLength(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34 }));

  private void SKADDNBR_MACADDR_InvalidLength(PhysicalAddress macAddress)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.ThrowsAsync<ArgumentException>(async () => await client.SendSKADDNBRAsync(
      ipv6Address: new IPAddress(new byte[] { 0xFE, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 }),
      macAddress: macAddress
    ));

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test, Ignore("not implemented")]
  public void SKADDNBR_MACADDR_InvalidLength_NonDeferredThrowing_0()
    => SKADDNBR_MACADDR_InvalidLength_NonDeferredThrowing(PhysicalAddress.None);

  [Test, Ignore("not implemented")]
  public void SKADDNBR_MACADDR_InvalidLength_NonDeferredThrowing_1()
    => SKADDNBR_MACADDR_InvalidLength_NonDeferredThrowing(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34 }));

  private void SKADDNBR_MACADDR_InvalidLength_NonDeferredThrowing(PhysicalAddress macAddress)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKADDNBRAsync(
      ipv6Address: new IPAddress(new byte[] { 0xFE, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 }),
      macAddress: macAddress
    ));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }
}
