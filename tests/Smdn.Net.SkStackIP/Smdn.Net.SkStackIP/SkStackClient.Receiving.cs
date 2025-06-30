// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Threading;

using NUnit.Framework;
using NUnit.Framework.Constraints;

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

    Assert.That(
      () => client.ReceiveResponseDelay = newValue,
      Throws.TypeOf<ArgumentOutOfRangeException>()
    );

    Assert.That(initialValue, Is.EqualTo(client.ReceiveResponseDelay), nameof(client.ReceiveResponseDelay));
  }

  private static System.Collections.IEnumerable YieldTestCases_ResponseParser_ERXUDP_InvalidTokenFormat()
  {
    // IPADDR
    yield return new object?[] {
      "ERXUDP 192.168.0.1 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567",
      "192.168.0.1",
      Is.Null, // token must have length of IPv6 address string
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111: FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567",
      "FE80:0000:0000:0000:021D:1290:1111:",
      Is.Null, // token must have length of IPv6 address string
    };
    yield return new object[] {
      "ERXUDP XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567",
      "XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX",
      Is.TypeOf<FormatException>() // invalid format, thrown by IPAddress.Parse
    };

    // ADDR64
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D12 0 0008 01234567",
      "001D12",
      Is.Null, // token must have 8 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D1290123456790000 0 0008 01234567",
      "001D1290123456790000",
      Is.Null, // token must have 8 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A XXXXXXXXXXXXXXXX 0 0008 01234567",
      "X",
      Is.Null // invalid format
    };

    // INT16
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E 0E1A 001D129012345679 0 0008 01234567",
      "0E",
      Is.Null, // token must have 2 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A00 001D129012345679 0 0008 01234567",
      "0E1A00",
      Is.Null, // token must have 2 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A GHIK 001D129012345679 0 0008 01234567",
      "GHIK",
      Is.InstanceOf<Exception>(), // invalid format, thrown by internal method
    };

    // binary (0/1)
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 01 0008 01234567",
      "01",
      Is.Null, // token must have exact 1 byte length
    };
    yield return new object?[] {
      "ERXUDP FE80:0000:0000:0000:021D:1290:1111:2222 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 2 0008 01234567",
      "2",
      Is.TypeOf<SkStackUnexpectedResponseException>(), // token must '0' or '1', thrown by internal method
    };
  }

  [TestCaseSource(nameof(YieldTestCases_ResponseParser_ERXUDP_InvalidTokenFormat))]
  public void ResponseParser_ERXUDP_InvalidToken(
    string erxudp,
    string expectedCausedText,
    Constraint innerExceptionConstraint
  )
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.ERXUDPDataFormat, Is.EqualTo(SkStackERXUDPDataFormat.Binary));

    stream.ResponseWriter.WriteLine(erxudp);

    using var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(1.0));
    var buffer = new ArrayBufferWriter<byte>();
    IPAddress? remoteAddress1 = null;

    Assert.That(
      async () => remoteAddress1 = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: buffer,
        cts.Token
      ),
      Throws
        .TypeOf<SkStackUnexpectedResponseException>()
        .And.Property(nameof(SkStackUnexpectedResponseException.CausedText)).EqualTo(expectedCausedText)
        .And.InnerException.Append(innerExceptionConstraint)
    );
  }
}
