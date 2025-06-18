// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKREGTests : SkStackClientTestsBase {
  private async Task SKSREG_Set(
    string expectedSentSequence,
    Func<SkStackClient, ValueTask<SkStackResponse>> sendSKSREGSetAsync
  )
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    var response = await sendSKSREGSetAsync(client);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(expectedSentSequence.ToByteSequence())
    );
  }
  [Test] public Task SKSREG_Set_UINT8_S02() => SKSREG_Set("SKSREG S02 21\r\n", (client) => client.SendSKSREGAsync(SkStackRegister.S02, SkStackChannel.Channel33));
  [Test] public Task SKSREG_Set_UINT16_S03() => SKSREG_Set("SKSREG S03 8888\r\n", (client) => client.SendSKSREGAsync(SkStackRegister.S03, (ushort)0x8888));
  [Test] public Task SKSREG_Set_UINT32_S16() => SKSREG_Set("SKSREG S16 384\r\n", (client) => client.SendSKSREGAsync(SkStackRegister.S16, TimeSpan.FromSeconds(0x384)));
  [Test] public Task SKSREG_Set_Binary_S15_True() => SKSREG_Set("SKSREG S15 1\r\n", (client) => client.SendSKSREGAsync(SkStackRegister.S15, true));
  [Test] public Task SKSREG_Set_Binary_S15_False() => SKSREG_Set("SKSREG S15 0\r\n", (client) => client.SendSKSREGAsync(SkStackRegister.S15, false));
  [Test] public Task SKSREG_Set_CHARArray_S0A() => SKSREG_Set("SKSREG S0A CCDDEEFF\r\n", (client) => client.SendSKSREGAsync(SkStackRegister.S0A, "CCDDEEFF".ToByteSequence()));

  private async Task SKSREG_Get<TValue>(
    string responsePayload,
    Func<SkStackClient, ValueTask<SkStackResponse<TValue>>> sendSKSREGGetAsync,
    string expectedSentSequence,
    TValue expectedValue
  )
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine(responsePayload);
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    var response = await sendSKSREGGetAsync(client);

    Assert.That(response.Payload, Is.Not.Null);

    if (expectedValue is ReadOnlyMemory<byte> expectedByteSequence)
      Assert.That(response.Payload, SequenceIs.EqualTo(expectedByteSequence)); // XXX
    else
      Assert.That(response.Payload, SequenceIs.EqualTo(expectedValue));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(expectedSentSequence.ToByteSequence())
    );
  }

  [Test]
  public Task SKSREG_Get_UINT8_S02() => SKSREG_Get(
    responsePayload: "ESREG 21",
    sendSKSREGGetAsync: (client) => client.SendSKSREGAsync(SkStackRegister.S02),
    expectedSentSequence: "SKSREG S02\r\n",
    expectedValue: SkStackChannel.Channel33
  );
  [Test]
  public Task SKSREG_Get_UINT16_S03() => SKSREG_Get(
    responsePayload: "ESREG 8888",
    sendSKSREGGetAsync: (client) => client.SendSKSREGAsync(SkStackRegister.S03),
    expectedSentSequence: "SKSREG S03\r\n",
    expectedValue: (ushort)0x8888
  );
  [Test]
  public Task SKSREG_Get_UINT32_S07() => SKSREG_Get(
    responsePayload: "ESREG 01234567",
    sendSKSREGGetAsync: (client) => client.SendSKSREGAsync(SkStackRegister.S07),
    expectedSentSequence: "SKSREG S07\r\n",
    expectedValue: (uint)0x01234567
  );
  [Test]
  public Task SKSREG_Get_UINT64_SFD() => SKSREG_Get(
    responsePayload: "ESREG FFFFFFFFFFFFFFFF",
    sendSKSREGGetAsync: (client) => client.SendSKSREGAsync(SkStackRegister.SFD),
    expectedSentSequence: "SKSREG SFD\r\n",
    expectedValue: 0xFFFFFFFFFFFFFFFF
  );
  [Test]
  public Task SKSREG_Get_Binary_SFB_False() => SKSREG_Get(
    responsePayload: "ESREG 0",
    sendSKSREGGetAsync: (client) => client.SendSKSREGAsync(SkStackRegister.SFB),
    expectedSentSequence: "SKSREG SFB\r\n",
    expectedValue: false
  );
  [Test]
  public Task SKSREG_Get_Binary_SFB_True() => SKSREG_Get(
    responsePayload: "ESREG 1",
    sendSKSREGGetAsync: (client) => client.SendSKSREGAsync(SkStackRegister.SFB),
    expectedSentSequence: "SKSREG SFB\r\n",
    expectedValue: true
  );
  [Test]
  public Task SKSREG_Get_CHARArray_S0A() => SKSREG_Get(
    responsePayload: "ESREG CCDDEEFF",
    sendSKSREGGetAsync: (client) => client.SendSKSREGAsync(SkStackRegister.S0A),
    expectedSentSequence: "SKSREG S0A\r\n",
    expectedValue: "CCDDEEFF".ToByteSequence()
  );

  [Test]
  public void SKSREG_Set_RegisterNull()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => client.SendSKSREGAsync(null!, (ushort)0x8888), Throws.ArgumentNullException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }


  [Test]
  public void SKSREG_Get_RegisterNull()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => client.SendSKSREGAsync<ushort>(null!), Throws.ArgumentNullException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  private void SKSREG_Set_RegisterReadOnly(Action<SkStackClient> sendSKSREGSetAsync)
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => sendSKSREGSetAsync(client), Throws.InvalidOperationException);
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

#pragma warning disable CA2012
  [Test] public void SKSREG_Set_RegisterReadOnly_S07() => SKSREG_Set_RegisterReadOnly((client) => client.SendSKSREGAsync(SkStackRegister.S07, default(uint)));
  [Test] public void SKSREG_Set_RegisterReadOnly_SFB() => SKSREG_Set_RegisterReadOnly((client) => client.SendSKSREGAsync(SkStackRegister.SFB, default(bool)));
  [Test] public void SKSREG_Set_RegisterReadOnly_SFD() => SKSREG_Set_RegisterReadOnly((client) => client.SendSKSREGAsync(SkStackRegister.SFD, default(ulong)));
#pragma warning restore CA2012

  private void SKSREG_Set_ValueOutOfRange<TArgumentException>(Action<SkStackClient> sendSKSREGSetAsync)
    where TArgumentException : ArgumentException
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(() => sendSKSREGSetAsync(client), Throws.TypeOf<TArgumentException>());
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

#pragma warning disable CA2012
  [Test] public void SKSREG_Set_RegisterValueOutOfRange_S02() => SKSREG_Set_ValueOutOfRange<ArgumentOutOfRangeException>((client) => client.SendSKSREGAsync(SkStackRegister.S02, default(SkStackChannel)));
  [Test] public void SKSREG_Set_RegisterValueOutOfRange_S0A_Empty() => SKSREG_Set_ValueOutOfRange<ArgumentException>((client) => client.SendSKSREGAsync(SkStackRegister.S0A, ReadOnlyMemory<byte>.Empty));
  [Test] public void SKSREG_Set_RegisterValueOutOfRange_S0A_TooShort() => SKSREG_Set_ValueOutOfRange<ArgumentOutOfRangeException>((client) => client.SendSKSREGAsync(SkStackRegister.S0A, new byte[] { 0x00 }.AsMemory()));
  [Test] public void SKSREG_Set_RegisterValueOutOfRange_S0A_TooLong() => SKSREG_Set_ValueOutOfRange<ArgumentOutOfRangeException>((client) => client.SendSKSREGAsync(SkStackRegister.S0A, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }.AsMemory()));
  [Test] public void SKSREG_Set_RegisterValueOutOfRange_S16_TooShort() => SKSREG_Set_ValueOutOfRange<ArgumentOutOfRangeException>((client) => client.SendSKSREGAsync(SkStackRegister.S16, TimeSpan.FromSeconds(59)));
  [Test] public void SKSREG_Set_RegisterValueOutOfRange_S16_TooLong() => SKSREG_Set_ValueOutOfRange<ArgumentOutOfRangeException>((client) => client.SendSKSREGAsync(SkStackRegister.S16, TimeSpan.FromSeconds(4294967296)));
#pragma warning restore CA2012
}
