// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKLL64Tests : SkStackClientTestsBase {
  [Test]
  public void SKLL64()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:1234:5678");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IPAddress>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKLL64Async(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 })));

    Assert.That(response, Is.Not.Null, nameof(response));
    Assert.That(response!.Payload, Is.Not.Null);
    Assert.That(response.Status, Is.EqualTo(SkStackResponseStatus.Ok));
    Assert.That(response.StatusText, Is.EqualTo(ReadOnlyMemory<byte>.Empty));
    Assert.That(response.Payload, Is.EqualTo(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKLL64 001D129012345678\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKLL64_IncompleteLine()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
#if false
      stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:1234:5678");
#endif

      stream.ResponseWriter.Write("FE80:0000:0000:0000:021D:1290:1234"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write(":5678"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("OK"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKLL64Async(
      new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 })
    ).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, CompleteResponseAsync())
    );
#pragma warning restore CA2012

    var response = taskSendCommand.Result;

    Assert.That(response.Payload, Is.Not.Null);
    Assert.That(response.Status, Is.EqualTo(SkStackResponseStatus.Ok));
    Assert.That(response.StatusText, Is.EqualTo(ReadOnlyMemory<byte>.Empty));
    Assert.That(response.Payload, Is.EqualTo(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKLL64 001D129012345678\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKLL64_ADDR64_ArgumentNull()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(() => client.SendSKLL64Async(macAddress: null!));
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [Test] public void SKLL64_ADDR64_InvalidLength_0() => SKLL64_ADDR64_InvalidLength(PhysicalAddress.None);
  [Test] public void SKLL64_ADDR64_InvalidLength_1() => SKLL64_ADDR64_InvalidLength(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34 }));

  private void SKLL64_ADDR64_InvalidLength(PhysicalAddress addr64)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.ThrowsAsync<ArgumentException>(async () => await client.SendSKLL64Async(macAddress: addr64));

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [Test, Ignore("not implemented")]
  public void SKLL64_ADDR64_InvalidLength_NonDeferredThrowing_0()
    => SKLL64_ADDR64_InvalidLength_NonDeferredThrowing(PhysicalAddress.None);

  [Test, Ignore("not implemented")]
  public void SKLL64_ADDR64_InvalidLength_NonDeferredThrowing_1()
    => SKLL64_ADDR64_InvalidLength_NonDeferredThrowing(new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34 }));

  private void SKLL64_ADDR64_InvalidLength_NonDeferredThrowing(PhysicalAddress addr64)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKLL64Async(macAddress: addr64));
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }
}
