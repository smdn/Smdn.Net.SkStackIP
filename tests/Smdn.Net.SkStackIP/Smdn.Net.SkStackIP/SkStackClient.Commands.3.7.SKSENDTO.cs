// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKSENDTOTests : SkStackClientTestsBase {
  private void SKSENDTO_IPADDR_PORT(Func<SkStackClient, ValueTask<SkStackResponse>> testAction)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    SkStackResponse response = default;

    Assert.DoesNotThrowAsync(async () => response = await testAction(client));

    Assert.IsNotNull(response, nameof(response));
    Assert.IsTrue(response!.Success);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSENDTO 1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0 0005 01234".ToByteSequence())
    );
  }

  [Test] public void SKSENDTO_IPADDR_PORT_IPEndPoint()
    => SKSENDTO_IPADDR_PORT(client => client.SendSKSENDTOAsync(
      handle: SkStackUdpPortHandle.Handle1,
      destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
      data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
      encryption: SkStackUdpEncryption.ForcePlainText
    ));

  [Test] public void SKSENDTO_IPADDR_PORT_IPAddressAndPort()
    => SKSENDTO_IPADDR_PORT(client => client.SendSKSENDTOAsync(
      handle: SkStackUdpPortHandle.Handle1,
      destinationAddress: IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"),
      destinationPort: 0x0E1A,
      data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
      encryption: SkStackUdpEncryption.ForcePlainText
    ));

  [Test] public void SKSENDTO_IPADDR_PORT_UdpPort()
    => SKSENDTO_IPADDR_PORT(async client => await client.SendSKSENDTOAsync(
      port: await GetUdpPortAsync(),
      destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
      data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
      encryption: SkStackUdpEncryption.ForcePlainText
    ));

  private static async Task<SkStackUdpPort> GetUdpPortAsync()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream);

    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("3610");
    stream.ResponseWriter.WriteLine("1");
    stream.ResponseWriter.WriteLine("2");
    stream.ResponseWriter.WriteLine("3");
    stream.ResponseWriter.WriteLine("4");
    stream.ResponseWriter.WriteLine("5");
    stream.ResponseWriter.WriteLine("OK");

    var response = await client.SendSKTABLEListeningPortListAsync();

    return response.Payload![0];
  }

  [Test]
  public void SKSENDTO_IPADDR_ArgumentNull_IPEndPoint()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<ArgumentNullException>(
      async () => await client.SendSKSENDTOAsync(
        handle: SkStackUdpPortHandle.Handle1,
        destination: (IPEndPoint)null!,
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: SkStackUdpEncryption.ForcePlainText
      )
    );

    Assert.AreEqual("destination", ex!.ParamName);

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSENDTO_IPADDR_ArgumentNull_IPAddress()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<ArgumentNullException>(
      async () => await client.SendSKSENDTOAsync(
        handle: SkStackUdpPortHandle.Handle1,
        destinationAddress: (IPAddress)null!,
        destinationPort: 0x0E1A,
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: SkStackUdpEncryption.ForcePlainText
      )
    );

    Assert.AreEqual("destinationAddress", ex!.ParamName);

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase(-1)]
  [TestCase((int)ushort.MaxValue + 1)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  public void SKSENDTO_PORT_ArgumentOutOfRange(int destinationPort)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
      async () => await client.SendSKSENDTOAsync(
        handle: SkStackUdpPortHandle.Handle1,
        destinationAddress: IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"),
        destinationPort: destinationPort,
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: SkStackUdpEncryption.ForcePlainText
      )
    );

    Assert.AreEqual("destinationPort", ex!.ParamName);

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase(SkStackUdpPortHandle.Handle1, 1)]
  [TestCase(SkStackUdpPortHandle.Handle2, 2)]
  [TestCase(SkStackUdpPortHandle.Handle3, 3)]
  [TestCase(SkStackUdpPortHandle.Handle4, 4)]
  [TestCase(SkStackUdpPortHandle.Handle5, 5)]
  [TestCase(SkStackUdpPortHandle.Handle6, 6)]
  public void SKSENDTO_HANDLE(SkStackUdpPortHandle handle, int expectedHandle)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.DoesNotThrowAsync(
      async () => await client.SendSKSENDTOAsync(
        handle: handle,
        destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: SkStackUdpEncryption.EncryptIfAble
      )
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo($"SKSENDTO {expectedHandle} FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 2 0005 01234".ToByteSequence())
    );
  }

  [TestCase((SkStackUdpPortHandle)0)]
  [TestCase((SkStackUdpPortHandle)7)]
  public void SKSENDTO_HANDLE_OutOfRange(SkStackUdpPortHandle handle)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
      async () => await client.SendSKSENDTOAsync(
        handle: handle,
        destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: SkStackUdpEncryption.ForcePlainText
      )
    );

    Assert.AreEqual("handle", ex!.ParamName);

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase(SkStackUdpEncryption.ForcePlainText, 0)]
  [TestCase(SkStackUdpEncryption.ForceEncrypt, 1)]
  [TestCase(SkStackUdpEncryption.EncryptIfAble, 2)]
  public void SKSENDTO_SEC(SkStackUdpEncryption sec, int expectedSec)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.DoesNotThrowAsync(
      async () => await client.SendSKSENDTOAsync(
        handle: SkStackUdpPortHandle.Handle1,
        destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: sec
      )
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo($"SKSENDTO 1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A {expectedSec} 0005 01234".ToByteSequence())
    );
  }

  [TestCase((SkStackUdpEncryption)3)]
  [TestCase((SkStackUdpEncryption)0xFF)]
  public void SKSENDTO_SEC_Undefined(SkStackUdpEncryption sec)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<ArgumentException>(
      async () => await client.SendSKSENDTOAsync(
        handle: SkStackUdpPortHandle.Handle1,
        destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: sec
      )
    );

    Assert.AreEqual("encryption", ex!.ParamName);

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase(0)]
  [TestCase(0x04D0 + 1)]
  public void SKSENDTO_DATALEN_OutOfRange(int datalen)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    var ex = Assert.ThrowsAsync<ArgumentException>(
      async () => await client.SendSKSENDTOAsync(
        handle: SkStackUdpPortHandle.Handle1,
        destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
        data: new byte[datalen],
        encryption: SkStackUdpEncryption.ForcePlainText
      )
    );

    Assert.AreEqual("data", ex!.ParamName);

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSENDTO_FAIL_ER10()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER10");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.SendSKSENDTOAsync(
        handle: SkStackUdpPortHandle.Handle1,
        destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
        data: new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34 },
        encryption: SkStackUdpEncryption.ForcePlainText
      )
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSENDTO 1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0 0005 01234".ToByteSequence())
    );
  }

  [Test]
  public async Task SKSENDTO_EchobackLine_Incomplete()
  {
    var stream = new PseudoSkStackStream();

    async Task CompleteResponseAsync()
    {
      stream.ResponseWriter.Write("S"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("KSENDTO 1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0 0002 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("OK");
    }

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKSENDTOAsync(
      handle: SkStackUdpPortHandle.Handle1,
      destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
      data: new byte[] { (byte)'\r', (byte)'\n', },
      encryption: SkStackUdpEncryption.ForcePlainText
    ).AsTask();

    await Task.WhenAll(taskSendCommand, CompleteResponseAsync());
#pragma warning restore CA2012

    var resp = taskSendCommand.Result;

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSENDTO 1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0 0002 \r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }

  [Test]
  public async Task SKSENDTO_EchobackLine_OnlyWithCRLF()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.Write("\r\n"); // echoback line only with CRLF
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    var resp = await client.SendSKSENDTOAsync(
      handle: SkStackUdpPortHandle.Handle1,
      destination: new IPEndPoint(IPAddress.Parse("FE80:0000:0000:0000:021D:1290:1234:5678"), 0x0E1A),
      data: new byte[] { (byte)'\r', (byte)'\n', },
      encryption: SkStackUdpEncryption.ForcePlainText
    );

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSENDTO 1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0 0002 \r\n".ToByteSequence())
    );

    Assert.IsTrue(resp.Success);
  }
}
