// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKINFOTests : SkStackClientTestsBase {
  [Test]
  public async Task SKINFO()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("EINFO FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678 21 8888 FFFE");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    var response = await client.SendSKINFOAsync();

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKINFO\r\n".ToByteSequence())
    );

    var (linkLocalAddress, macAddress, channel, panId, addr16) = response.Payload;

    Assert.That(linkLocalAddress, Is.EqualTo(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")), nameof(linkLocalAddress));
    Assert.That(macAddress, Is.EqualTo(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 })), nameof(macAddress));
    Assert.That(channel, Is.EqualTo(SkStackChannel.Channel33), nameof(channel));
    Assert.That(panId, Is.EqualTo(0x8888), nameof(panId));
    Assert.That(addr16, Is.EqualTo(0xFFFE), nameof(addr16));
  }

  [Test]
  public void SKINFO_IncompleteLine()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
#if false
      stream.ResponseWriter.WriteLine("EINFO FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678 21 8888 FFFE");
#endif

      stream.ResponseWriter.Write("EINFO"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write(" FE80:0000:0000:0000:021D:1290:1234:5678"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write(" 001D129012345678 21 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("8888 FFFE"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("OK");
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKINFOAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, CompleteResponseAsync())
    );
#pragma warning restore CA2012

    var response = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKINFO\r\n".ToByteSequence())
    );

    var (linkLocalAddress, macAddress, channel, panId, addr16) = response.Payload;

    Assert.That(linkLocalAddress, Is.EqualTo(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")), nameof(linkLocalAddress));
    Assert.That(macAddress, Is.EqualTo(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 })), nameof(macAddress));
    Assert.That(channel, Is.EqualTo(SkStackChannel.Channel33), nameof(channel));
    Assert.That(panId, Is.EqualTo(0x8888), nameof(panId));
    Assert.That(addr16, Is.EqualTo(0xFFFE), nameof(addr16));
  }
}
