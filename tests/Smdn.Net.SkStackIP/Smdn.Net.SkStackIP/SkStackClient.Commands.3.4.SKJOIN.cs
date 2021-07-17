// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientCommandsSKJOINTests : SkStackClientCommandsTestsBase {
    [Test]
    public void SKJOIN()
    {
      const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var address = IPAddress.Parse(addressString);

      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      async Task RaisePanaSessionEstablishmentEventsAsync()
      {
        stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
        stream.ResponseWriter.WriteLine($"EVENT 02 {addressString}");
        stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.Write($"EVENT 21 "); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.Write($"{addressString} 00"); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.WriteLine();
        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");
        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.Write($"EVENT "); await Task.Delay(ResponseDelayInterval);
        stream.ResponseWriter.WriteLine($"25 {addressString}");
      }

      using var client = SkStackClient.Create(stream, ServiceProvider);
      var taskSendCommand = client.SendSKJOINAsync(address);

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionEstablishmentEventsAsync());
      });

      var response = taskSendCommand.Result;

      Assert.IsTrue(response.Success);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKJOIN FE80:0000:0000:0000:021D:1290:1234:5678\r\n".ToByteSequence())
      );
    }

    [Test]
    public void SKJOIN_FailedByEVENT24()
    {
      const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
      var address = IPAddress.Parse(addressString);

      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      async Task RaisePanaSessionEstablishmentEventsAsync()
      {
        stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
        stream.ResponseWriter.WriteLine($"EVENT 02 {addressString}");
        stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 00");
        stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.Write($"EVENT 24 ");

        await Task.Delay(ResponseDelayInterval);

        stream.ResponseWriter.WriteLine($"{addressString}");
      }

      using var client = SkStackClient.Create(stream, ServiceProvider);
      var taskSendCommand = client.SendSKJOINAsync(address);

      var ex = Assert.ThrowsAsync<SkStackPanaSessionEstablishmentException>(async () => {
        await Task.WhenAll(taskSendCommand.AsTask(), RaisePanaSessionEstablishmentEventsAsync());
      });

      Assert.AreEqual(SkStackEventNumber.PanaSessionEstablishmentError, ex.EventNumber);
      Assert.AreEqual(address, ex.Address);

      Assert.That(
        stream.ReadSentData(),
        Is.EqualTo("SKJOIN FE80:0000:0000:0000:021D:1290:1234:5678\r\n".ToByteSequence())
      );
    }

    [Test]
    public void SKJOIN_AddressNull()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, ServiceProvider);
      Assert.Throws<ArgumentNullException>(() => client.SendSKJOINAsync(ipv6address: null));

      Assert.IsEmpty(stream.ReadSentData());
    }

    [Test]
    public void SKJOIN_InvalidAddressFamily()
    {
      var stream = new PseudoSkStackStream();

      stream.ResponseWriter.WriteLine("OK");

      using var client = SkStackClient.Create(stream, ServiceProvider);
      Assert.Throws<ArgumentException>(() => client.SendSKJOINAsync(ipv6address: IPAddress.Loopback));

      Assert.IsEmpty(stream.ReadSentData());
    }
  }
}