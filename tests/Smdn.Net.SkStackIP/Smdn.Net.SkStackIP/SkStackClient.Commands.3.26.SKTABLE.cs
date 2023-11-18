// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKTABLETests : SkStackClientTestsBase {
  [Test]
  public void SKTABLE_AvailableAddressList()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("EADDR");
    stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:0003:AFE0");
    stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:0003:AFE1");
    stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:0003:AFE2");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IReadOnlyList<IPAddress>>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKTABLEAvailableAddressListAsync());

    Assert.IsNotNull(response, nameof(response));
    Assert.IsNotNull(response!.Payload);
    Assert.AreEqual(3, response.Payload!.Count);
    Assert.AreEqual(
      IPAddress.Parse("FE80:0000:0000:0000:021D:1290:0003:AFE0"),
      response.Payload[0]
    );
    Assert.AreEqual(
      IPAddress.Parse("FE80:0000:0000:0000:021D:1290:0003:AFE1"),
      response.Payload[1]
    );
    Assert.AreEqual(
      IPAddress.Parse("FE80:0000:0000:0000:021D:1290:0003:AFE2"),
      response.Payload[2]
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 1\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKTABLE_AvailableAddressList_IncompleteLine()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
#if false
      stream.ResponseWriter.WriteLine("EADDR");
      stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:0003:AFE0");
      stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:0003:AFE1");
#endif
      stream.ResponseWriter.Write("EADDR"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("FE80:0000:0000:0000:021D:1290:0003:AFE0"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("FE80:0000:0000:0000:021D:1290:0003:AFE"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine("1");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("OK"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKTABLEAvailableAddressListAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, CompleteResponseAsync())
    );
#pragma warning restore CA2012

    var response = taskSendCommand.Result;

    Assert.IsNotNull(response.Payload);
    Assert.AreEqual(2, response.Payload!.Count);
    Assert.AreEqual(
      IPAddress.Parse("FE80:0000:0000:0000:021D:1290:0003:AFE0"),
      response.Payload[0]
    );
    Assert.AreEqual(
      IPAddress.Parse("FE80:0000:0000:0000:021D:1290:0003:AFE1"),
      response.Payload[1]
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 1\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKTABLE_AvailableAddressList_Empty()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("EADDR");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IReadOnlyList<IPAddress>>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKTABLEAvailableAddressListAsync());

    Assert.IsNotNull(response, nameof(response));
    Assert.IsNotNull(response!.Payload);
    Assert.AreEqual(0, response.Payload!.Count);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 1\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKTABLE_AvailableAddressList_NoEADDR()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IReadOnlyList<IPAddress>>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKTABLEAvailableAddressListAsync());

    Assert.IsNotNull(response, nameof(response));
    Assert.IsNotNull(response!.Payload);
    Assert.AreEqual(0, response.Payload!.Count);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 1\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKTABLE_NeighborCacheList()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("ENEIGHBOR");
    stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678 FFFF");
    stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:1234:5679 001D129012345679 FFFF");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IReadOnlyDictionary<IPAddress, PhysicalAddress>>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKTABLENeighborCacheListAsync());

    Assert.IsNotNull(response, nameof(response));
    Assert.IsNotNull(response!.Payload);
    Assert.AreEqual(2, response.Payload!.Count);

    Assert.IsTrue(response.Payload.ContainsKey(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")));
    Assert.IsTrue(response.Payload.ContainsKey(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5679")));

    Assert.AreEqual(
      new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 }),
      response.Payload[IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")]
    );
    Assert.AreEqual(
      new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x79 }),
      response.Payload[IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5679")]
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 2\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKTABLE_NeighborCacheList_IncompleteLine()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
#if false
      stream.ResponseWriter.WriteLine("ENEIGHBOR");
      stream.ResponseWriter.WriteLine("FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678 FFFF");
#endif
      stream.ResponseWriter.Write("ENEIGHBOR"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("FE80:0000:0000:0000:021D:1290:1234:5678"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write(" 001D129012345678 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("FFFF"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("OK"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKTABLENeighborCacheListAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, CompleteResponseAsync())
    );
#pragma warning restore CA2012

    var response = taskSendCommand.Result;

    Assert.IsNotNull(response.Payload);
    Assert.AreEqual(1, response.Payload!.Count);

    Assert.IsTrue(response.Payload.ContainsKey(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")));

    Assert.AreEqual(
      new PhysicalAddress(new byte[] { 0x00, 0x1D, 0x12, 0x90, 0x12, 0x34, 0x56, 0x78 }),
      response.Payload[IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678")]
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 2\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKTABLE_NeighborCacheList_Empty()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("ENEIGHBOR");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IReadOnlyDictionary<IPAddress, PhysicalAddress>>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKTABLENeighborCacheListAsync());

    Assert.IsNotNull(response, nameof(response));
    Assert.IsNotNull(response!.Payload);
    Assert.AreEqual(0, response.Payload!.Count);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 2\r\n".ToByteSequence())
    );
  }


  [Test]
  public void SKTABLE_NeighborCacheList_NoENEIGHBOR()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IReadOnlyDictionary<IPAddress, PhysicalAddress>>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKTABLENeighborCacheListAsync());

    Assert.IsNotNull(response, nameof(response));
    Assert.IsNotNull(response!.Payload);
    Assert.AreEqual(0, response.Payload!.Count);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE 2\r\n".ToByteSequence())
    );
  }

  [TestCase(false)]
  [TestCase(true)]
  public void SKTABLE_ListeningPortList(bool extraResponse)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("3610");
    stream.ResponseWriter.WriteLine("1");
    stream.ResponseWriter.WriteLine("2");
    stream.ResponseWriter.WriteLine("3");
    stream.ResponseWriter.WriteLine("4");
    stream.ResponseWriter.WriteLine("5");

    // [VER 1.2.10, APPVER rev26e] EPORT responds extra CRLF and PORT_UDPs?
    // "EPORT␍␊3610␍␊716␍␊0␍␊0␍␊0␍␊0␍␊␍␊3610␍␊0␍␊0␍␊0␍␊OK␍␊"
    if (extraResponse) {
      stream.ResponseWriter.WriteLine();
      stream.ResponseWriter.WriteLine("3601");
      stream.ResponseWriter.WriteLine("0");
      stream.ResponseWriter.WriteLine("0");
      stream.ResponseWriter.WriteLine("0");
    }

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse<IReadOnlyList<SkStackUdpPort>>? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKTABLEListeningPortListAsync());

    Assert.IsNotNull(response, nameof(response));

    var ports = response!.Payload;

    Assert.IsNotNull(ports);
    Assert.AreEqual(6, ports!.Count);

    Assert.AreEqual(3610, ports[0].Port, $"{nameof(SkStackUdpPort.Port)} #0");
    Assert.AreEqual(SkStackUdpPortHandle.Handle1, ports[0].Handle, $"{nameof(SkStackUdpPort.Handle)} #0");

    Assert.AreEqual(1, ports[1].Port, $"{nameof(SkStackUdpPort.Port)} #1");
    Assert.AreEqual(SkStackUdpPortHandle.Handle2, ports[1].Handle, $"{nameof(SkStackUdpPort.Handle)} #1");

    Assert.AreEqual(2, ports[2].Port, $"{nameof(SkStackUdpPort.Port)} #2");
    Assert.AreEqual(SkStackUdpPortHandle.Handle3, ports[2].Handle, $"{nameof(SkStackUdpPort.Handle)} #2");

    Assert.AreEqual(3, ports[3].Port, $"{nameof(SkStackUdpPort.Port)} #3");
    Assert.AreEqual(SkStackUdpPortHandle.Handle4, ports[3].Handle, $"{nameof(SkStackUdpPort.Handle)} #3");

    Assert.AreEqual(4, ports[4].Port, $"{nameof(SkStackUdpPort.Port)} #4");
    Assert.AreEqual(SkStackUdpPortHandle.Handle5, ports[4].Handle, $"{nameof(SkStackUdpPort.Handle)} #4");

    Assert.AreEqual(5, ports[5].Port, $"{nameof(SkStackUdpPort.Port)} #5");
    Assert.AreEqual(SkStackUdpPortHandle.Handle6, ports[5].Handle, $"{nameof(SkStackUdpPort.Handle)} #5");

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKTABLE E\r\n".ToByteSequence())
    );
  }
}
