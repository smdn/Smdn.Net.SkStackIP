// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Net;
using System.IO;
using System.Threading;

using NUnit.Framework;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientReceivingTests : SkStackClientTestsBase {
  private static System.Collections.IEnumerable YieldTestCases_ReceiveResponseDelay_Set()
  {
    yield return new object?[] { TimeSpan.FromMilliseconds(1) };
    yield return new object?[] { TimeSpan.FromSeconds(1) };
    yield return new object?[] { TimeSpan.MaxValue };
  }

  [TestCaseSource(nameof(YieldTestCases_ReceiveResponseDelay_Set))]
  public void ReceiveResponseDelay_Set(TimeSpan newValue)
  {
    using var client = new SkStackClient(Stream.Null);

    Assert.DoesNotThrow(() => client.ReceiveResponseDelay = newValue);

    Assert.That(newValue, Is.EqualTo(client.ReceiveResponseDelay), nameof(client.ReceiveResponseDelay));
  }

  private static System.Collections.IEnumerable YieldTestCases_ReceiveResponseDelay_Set_InvalidValue()
  {
    yield return new object?[] { TimeSpan.Zero };
    yield return new object?[] { TimeSpan.MinValue };
    yield return new object?[] { TimeSpan.FromMilliseconds(-1) };
    yield return new object?[] { TimeSpan.FromSeconds(-1) };
    yield return new object?[] { Timeout.InfiniteTimeSpan };
  }

  [TestCaseSource(nameof(YieldTestCases_ReceiveResponseDelay_Set_InvalidValue))]
  public void ReceiveResponseDelay_Set_InvalidValue(TimeSpan newValue)
  {
    using var client = new SkStackClient(Stream.Null);

    var initialValue = client.ReceiveResponseDelay;

    Assert.Throws<ArgumentOutOfRangeException>(() => client.ReceiveResponseDelay = newValue);

    Assert.That(initialValue, Is.EqualTo(client.ReceiveResponseDelay), nameof(client.ReceiveResponseDelay));
  }

  private static System.Collections.IEnumerable YieldTestCases_ResponseParser_ERXUDP_InvalidTokenFormat()
  {
    // IPADDR
    yield return new object?[] {
      "ERXUDP 192.168.0.1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567",
      "192.168.0.1",
      null, // token must have length of IPv6 address string
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111: FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567",
      "FE80:0000:0000:0000:021D:1290:1111:",
      null, // token must have length of IPv6 address string
    };
    yield return new object[] {
      "ERXUDP XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567",
      "XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX",
      typeof(FormatException) // invalid format, thrown by IPAddress.Parse
    };

    // ADDR64
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D12 0 0008 01234567",
      "001D12",
      null, // token must have 8 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D1290123456790000 0 0008 01234567",
      "001D1290123456790000",
      null, // token must have 8 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A XXXXXXXXXXXXXXXX 0 0008 01234567",
      "X",
      null // invalid format
    };

    // INT16
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E 0E1A 001D129012345679 0 0008 01234567",
      "0E",
      null, // token must have 2 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A00 001D129012345679 0 0008 01234567",
      "0E1A00",
      null, // token must have 2 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A GHIK 001D129012345679 0 0008 01234567",
      "GHIK",
      typeof(Exception), // invalid format, thrown by internal method
    };

    // binary (0/1)
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 01 0008 01234567",
      "01",
      null, // token must have exact 1 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 2 0008 01234567",
      "2",
      typeof(SkStackUnexpectedResponseException), // token must '0' or '1', thrown by internal method
    };
  }

  [TestCaseSource(nameof(YieldTestCases_ResponseParser_ERXUDP_InvalidTokenFormat))]
  public void ResponseParser_ERXUDP_InvalidToken(
    string erxudp,
    string expectedCausedText,
    Type? expectedTypeOfInnerException
  )
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.ERXUDPDataFormat, Is.EqualTo(SkStackERXUDPDataFormat.Binary));

    stream.ResponseWriter.WriteLine(erxudp);

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    var buffer = new ArrayBufferWriter<byte>();
    IPAddress? remoteAddress1 = null;

    var ex = Assert.ThrowsAsync<SkStackUnexpectedResponseException>(
      async () => remoteAddress1 = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: buffer,
        cts.Token
      )
    );

    Assert.That(ex, Is.Not.Null);
    Assert.That(ex!.CausedText, Is.EqualTo(expectedCausedText), nameof(ex.CausedText));

    if (expectedTypeOfInnerException is null)
      Assert.That(ex.InnerException, Is.Null, nameof(ex.InnerException));
    else
      Assert.That(ex.InnerException, Is.AssignableTo(expectedTypeOfInnerException), nameof(ex.InnerException));
  }
}
