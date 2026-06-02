// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientLoggingTests : SkStackClientTestsBase {
  [Test]
  public void LogDebugCommandSequence()
  {
    const int EventIdCommandSequence = 2;

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Debug && record.Id.Id == EventIdCommandSequence
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(logSnapshot[0].Message, Is.EqualTo("↦ SKRESET␍␊"));
  }

  [Test]
  public void LogDebugResponseSequence()
  {
    const int EventIdResponseSequence = 3;

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Debug && record.Id.Id == EventIdResponseSequence
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(logSnapshot[0].Message, Is.EqualTo("↤ OK␍␊"));
  }

  [Test]
  public void LogDebugResponseSequence_Echoback()
  {
    const int EventIdResponseSequence = 3;

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Debug && record.Id.Id == EventIdResponseSequence
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("SKRESET");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(2));
    Assert.That(logSnapshot[0].Message, Is.EqualTo("↩ SKRESET␍␊"));
    Assert.That(logSnapshot[1].Message, Is.EqualTo("↤ OK␍␊"));
  }

  [TestCase(SkStackEventNumber.NeighborSolicitationReceived)]
  [TestCase(SkStackEventNumber.NeighborAdvertisementReceived)]
  [TestCase(SkStackEventNumber.EchoRequestReceived)]
  public void LogInfoIPEventReceived(SkStackEventNumber ipEventNumber)
  {
    const int EventIdIPEventReceived = 6;
    const string SenderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Information && record.Id.Id == EventIdIPEventReceived
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"EVENT {(byte)ipEventNumber:X2} {SenderAddressString}");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(
      logSnapshot[0].Message,
      Is.EqualTo($"IPv6: {ipEventNumber} (EVENT {(byte)ipEventNumber:X2}, {IPAddress.Parse(SenderAddressString)})")
    );
  }

  [TestCase(0, "Successful")]
  [TestCase(1, "Failed")]
  [TestCase(2, "Neighbor Solicitation")]
  [TestCase(3, "Unknown")]
  [TestCase(9, "Unknown")]
  public void LogInfoIPEventReceived_UdpSendCompleted(byte parameter, string parameterString)
  {
    const SkStackEventNumber UdpSendCompletedEventNumber = SkStackEventNumber.UdpSendCompleted;
    const int EventIdIPEventReceived = 6;
    const string SenderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Information && record.Id.Id == EventIdIPEventReceived
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"EVENT {(byte)UdpSendCompletedEventNumber:X2} {SenderAddressString} {parameter:X2}");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(
      logSnapshot[0].Message,
      Is.EqualTo($"IPv6: {UdpSendCompletedEventNumber} - {parameterString} (EVENT {(byte)UdpSendCompletedEventNumber:X2}, PARAM {parameter}, {IPAddress.Parse(SenderAddressString)})")
    );
  }

  [Test]
  public void LogInfoIPEventReceived_UdpReceived_EchonetLite()
  {
    const int EventIdIPEventReceived = 6;
    const string RemoteAddressString = "FE80:0000:0000:0000:021D:1290:1111:2222";
    const string RemoteLinkLocalAddress = "021D129011112222";
    const string LocalAddressString = "FE80:0000:0000:0000:021D:1290:3333:4444";

    var expectedRemoteEndPoint = new IPEndPoint(
      IPAddress.Parse(RemoteAddressString),
      SkStackKnownPortNumbers.EchonetLite
    );
    var expectedLocalEndPoint = new IPEndPoint(
      IPAddress.Parse(LocalAddressString),
      SkStackKnownPortNumbers.EchonetLite
    );

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Information && record.Id.Id == EventIdIPEventReceived
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"ERXUDP {RemoteAddressString} {LocalAddressString} {SkStackKnownPortNumbers.EchonetLite:X4} {SkStackKnownPortNumbers.EchonetLite:X4} {RemoteLinkLocalAddress} 0 000C ECHONET-LITE");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(
      logSnapshot[0].Message,
      Is.EqualTo($"ECHONET Lite/IPv6: {expectedLocalEndPoint}←{expectedRemoteEndPoint} {RemoteLinkLocalAddress} (secured: False, length: 12)")
    );
  }

  [Test]
  public void LogInfoIPEventReceived_UdpReceived_Pana()
  {
    const int EventIdIPEventReceived = 6;
    const string RemoteAddressString = "FE80:0000:0000:0000:021D:1290:1111:2222";
    const string RemoteLinkLocalAddress = "021D129011112222";
    const string LocalAddressString = "FE80:0000:0000:0000:021D:1290:3333:4444";

    var expectedRemoteEndPoint = new IPEndPoint(
      IPAddress.Parse(RemoteAddressString),
      SkStackKnownPortNumbers.Pana
    );
    var expectedLocalEndPoint = new IPEndPoint(
      IPAddress.Parse(LocalAddressString),
      SkStackKnownPortNumbers.Pana
    );

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Information && record.Id.Id == EventIdIPEventReceived
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"ERXUDP {RemoteAddressString} {LocalAddressString} {SkStackKnownPortNumbers.Pana:X4} {SkStackKnownPortNumbers.Pana:X4} {RemoteLinkLocalAddress} 0 0004 PANA");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(
      logSnapshot[0].Message,
      Is.EqualTo($"PANA/IPv6: {expectedLocalEndPoint}←{expectedRemoteEndPoint} {RemoteLinkLocalAddress} (secured: False, length: 4)")
    );
  }

  [Test]
  public void LogInfoIPEventReceived_UdpReceived_OtherPort()
  {
    const int EventIdIPEventReceived = 6;
    const int PortNumber = 1234;
    const string RemoteAddressString = "FE80:0000:0000:0000:021D:1290:1111:2222";
    const string RemoteLinkLocalAddress = "021D129011112222";
    const string LocalAddressString = "FE80:0000:0000:0000:021D:1290:3333:4444";

    var expectedRemoteEndPoint = new IPEndPoint(
      IPAddress.Parse(RemoteAddressString),
      PortNumber
    );
    var expectedLocalEndPoint = new IPEndPoint(
      IPAddress.Parse(LocalAddressString),
      PortNumber
    );

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Information && record.Id.Id == EventIdIPEventReceived
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"ERXUDP {RemoteAddressString} {LocalAddressString} {PortNumber:X4} {PortNumber:X4} {RemoteLinkLocalAddress} 1 0003 UDP");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(
      logSnapshot[0].Message,
      Is.EqualTo($"IPv6: {expectedLocalEndPoint}←{expectedRemoteEndPoint} {RemoteLinkLocalAddress} (secured: True, length: 3)")
    );
  }

  [TestCase(SkStackEventNumber.PanaSessionEstablishmentError)]
  [TestCase(SkStackEventNumber.PanaSessionEstablishmentCompleted)]
  [TestCase(SkStackEventNumber.PanaSessionTerminationRequestReceived)]
  [TestCase(SkStackEventNumber.PanaSessionTerminationCompleted)]
  [TestCase(SkStackEventNumber.PanaSessionTerminationTimedOut)]
  [TestCase(SkStackEventNumber.PanaSessionExpired)]
  public void LogInfoPanaEventReceived(SkStackEventNumber panaEventNumber)
  {
    const int EventIdPanaEventReceived = 7;
    const string SenderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Information && record.Id.Id == EventIdPanaEventReceived
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"EVENT {(byte)panaEventNumber:X2} {SenderAddressString}");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(
      logSnapshot[0].Message,
      Is.EqualTo($"PANA: {panaEventNumber} (EVENT {(byte)panaEventNumber:X2}, {IPAddress.Parse(SenderAddressString)})")
    );
  }

  [TestCase(SkStackEventNumber.TransmissionTimeControlLimitationActivated)]
  [TestCase(SkStackEventNumber.TransmissionTimeControlLimitationDeactivated)]
  public void LogInfoAribStdT108EventReceived(SkStackEventNumber aribStdT108EventNumber)
  {
    const int EventIdAribStdT108EventReceived = 8;
    const string SenderAddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var loggerOptions = new FakeLogCollectorOptions() {
      CustomFilter = static record
        => record.Level == LogLevel.Information && record.Id.Id == EventIdAribStdT108EventReceived
    };

    var logCollector = FakeLogCollector.Create(loggerOptions);
    var logger = new FakeLogger<SkStackClient>(logCollector);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine($"EVENT {(byte)aribStdT108EventNumber:X2} {SenderAddressString}");
    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(
      stream: stream,
      logger: logger
    );

    Assert.DoesNotThrowAsync(async () => await client.SendSKRESETAsync());

    var logSnapshot = logCollector.GetSnapshot();

    Assert.That(logSnapshot.Count, Is.EqualTo(1));
    Assert.That(
      logSnapshot[0].Message,
      Is.EqualTo($"ARIB STD-T108: {aribStdT108EventNumber} (EVENT {(byte)aribStdT108EventNumber:X2}, {IPAddress.Parse(SenderAddressString)})")
    );
  }
}
