// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public partial class SkStackClientFunctionsPanaTests : SkStackClientTestsBase {
  private static readonly TimeSpan DefaultTimeOut = TimeSpan.FromSeconds(5);

  [Test]
  public void TerminatePanaSessionAsync_PanaSessionNotEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

#pragma warning disable CA2012
    Assert.Throws<InvalidOperationException>(() => client.TerminatePanaSessionAsync());
#pragma warning restore CA2012
    Assert.ThrowsAsync<InvalidOperationException>(async () => await client.TerminatePanaSessionAsync());
  }

  [TestCase(true)]
  [TestCase(false)]
  public void TerminatePanaSessionAsync(bool timeout)
  {
    const string AddressString = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    stream.ClearSentData();

    // SKTERM
    stream.ResponseWriter.WriteLine($"EVENT 21 {AddressString} 02");
    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionTerminationEventsAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"EVENT {(timeout ? 28 : 27)} {AddressString}");
    }

    var taskSendTerminatePanaSessionAsync = client.TerminatePanaSessionAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendTerminatePanaSessionAsync, RaisePanaSessionTerminationEventsAsync())
    );

    Assert.That(
      taskSendTerminatePanaSessionAsync.Result,
      Is.EqualTo(expected: !timeout)
    );

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKTERM\r\n".ToByteSequence())
    );
  }
}
