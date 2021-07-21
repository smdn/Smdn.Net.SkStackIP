// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKREJOINTests : SkStackClientTestsBase {
    [Test]
    public void SKREJOIN()
    {
      const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var senderAddress = IPAddress.Parse(senderAddressString);

      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      async Task RaisePanaSessionEstablishmentEventsAsync()
      {
        stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddressString} 02");
        stream.ResponseWriter.WriteLine($"EVENT 02 {senderAddressString}");
        stream.ResponseWriter.WriteLine($"ERXUDP {senderAddressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddressString} 00");
        stream.ResponseWriter.WriteLine($"ERXUDP {senderAddressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.Write($"EVENT 2"); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.WriteLine($"5 {senderAddressString}");
      }

      using var client = SkStackClient.Create(stream, ServiceProvider);
      var taskSendCommand = client.SendSKREJOINAsync();

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionEstablishmentEventsAsync());
      });

      var (response, address) = taskSendCommand.Result;

      Assert.IsTrue(response.Success);
      Assert.AreEqual(senderAddress, address);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKREJOIN\r\n".ToByteSequence())
      );
    }

    [Test]
    public void SKREJOIN_FailedByEVENT24()
    {
      const string senderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var senderAddress = IPAddress.Parse(senderAddressString);

      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      async Task RaisePanaSessionEstablishmentEventsAsync()
      {
        stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddressString} 02");
        stream.ResponseWriter.WriteLine($"EVENT 02 {senderAddressString}");
        stream.ResponseWriter.WriteLine($"ERXUDP {senderAddressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddressString} 00");
        stream.ResponseWriter.WriteLine($"ERXUDP {senderAddressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.Write($"EVEN"); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.Write($"T 2"); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.WriteLine($"4 {senderAddressString}");
      }

      using var client = SkStackClient.Create(stream, ServiceProvider);
      var taskSendCommand = client.SendSKREJOINAsync();

      var ex = Assert.ThrowsAsync<SkStackPanaSessionEstablishmentException>(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionEstablishmentEventsAsync());
      });

      Assert.AreEqual(SkStackEventNumber.PanaSessionEstablishmentError, ex.EventNumber);
      Assert.AreEqual(senderAddress, ex.Address);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKREJOIN\r\n".ToByteSequence())
      );
    }

    [Test]
    public void SKREJOIN_Fail()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("FAIL ER10");

      using var client = SkStackClient.Create(stream, ServiceProvider);

      var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(async () => await client.SendSKREJOINAsync());

      Assert.AreEqual(SkStackErrorCode.ER10, ex.ErrorCode);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKREJOIN\r\n".ToByteSequence())
      );
    }
  }
}