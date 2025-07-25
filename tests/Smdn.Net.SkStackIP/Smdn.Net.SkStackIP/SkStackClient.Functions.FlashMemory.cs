// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientFunctionsFlashMemoryTests : SkStackClientTestsBase {
  [Test]
  public void SaveFlashMemoryAsync_AlwaysGranted()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKSAVE
    stream.ResponseWriter.WriteLine("OK");
    // SKSAVE
    stream.ResponseWriter.WriteLine("OK");

    var restriction = SkStackFlashMemoryWriteRestriction.DangerousCreateAlwaysGrant();

    Assert.DoesNotThrowAsync(
      async () => await client.SaveFlashMemoryAsync(restriction),
      "#1"
    );

    Assert.DoesNotThrowAsync(
      async () => await client.SaveFlashMemoryAsync(restriction),
      "#2"
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSAVE\r\nSKSAVE\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SaveFlashMemoryAsync_ER10()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKSAVE
    stream.ResponseWriter.WriteLine("FAIL ER10");

    Assert.ThrowsAsync<SkStackFlashMemoryIOException>(
      async () => await client.SaveFlashMemoryAsync(SkStackFlashMemoryWriteRestriction.DangerousCreateAlwaysGrant())
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSAVE\r\n".ToByteSequence())
    );
  }

  [TestCase(-1, typeof(ArgumentOutOfRangeException))]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1000, null)]
  public async Task SaveFlashMemoryAsync_GrantIfTimeRestrictionHasElapsed(int intervalMilliseconds, Type? typeOfExpectedException)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    SkStackFlashMemoryWriteRestriction? restriction = null;
    var interval = TimeSpan.FromMilliseconds(intervalMilliseconds);

    if (typeOfExpectedException is null) {
      Assert.That(
        () => restriction = SkStackFlashMemoryWriteRestriction.CreateGrantIfElapsed(interval: interval),
        Throws.Nothing
      );
    }
    else {
      Assert.That(
        () => restriction = SkStackFlashMemoryWriteRestriction.CreateGrantIfElapsed(interval: interval),
        Throws.TypeOf(typeOfExpectedException)
      );
      return;
    }

    Assert.That(restriction, Is.Not.Null);

    // SKSAVE
    stream.ResponseWriter.WriteLine("OK");
    // SKSAVE
    stream.ResponseWriter.WriteLine("OK");

    Assert.DoesNotThrowAsync(
      async () => await client.SaveFlashMemoryAsync(
        restriction: restriction!
      ),
      message: "initial write must be granted always"
    );

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.SaveFlashMemoryAsync(
        restriction: restriction!
      ),
      message: "subsequent write must no be granted if specific interval have not elapsed (#1)"
    );

    await Task.Delay(interval);
    await Task.Delay(TimeSpan.FromMilliseconds(500));

    Assert.DoesNotThrowAsync(
      async () => await client.SaveFlashMemoryAsync(
        restriction: restriction!
      ),
      message: "subsequent write should be granted if specific interval has elapsed"
    );

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.SaveFlashMemoryAsync(
        restriction: restriction!
      ),
      message: "subsequent write must no be granted if specific interval have not elapsed (#2)"
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSAVE\r\nSKSAVE\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SaveFlashMemoryAsync_RestrictionNull()
  {
    using var client = new SkStackClient(Stream.Null, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(
      () => client.SaveFlashMemoryAsync(restriction: null!),
      Throws.ArgumentNullException
    );
#pragma warning restore CA2012
  }

  private sealed class AlwaysDenySkStackFlashMemoryWriteRestriction : SkStackFlashMemoryWriteRestriction {
    protected override bool IsRestricted() => true;
  }

  [Test]
  public void SaveFlashMemoryAsync_IsRestricted()
  {
    using var client = new SkStackClient(Stream.Null, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(
      () => client.SaveFlashMemoryAsync(restriction: new AlwaysDenySkStackFlashMemoryWriteRestriction()),
      Throws.InvalidOperationException
    );
#pragma warning restore CA2012
  }

  [Test]
  public void SaveFlashMemoryAsync_CancellationRequested()
  {
    using var client = new SkStackClient(Stream.Null, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.That(
      () => client.SaveFlashMemoryAsync(
        restriction: SkStackFlashMemoryWriteRestriction.DangerousCreateAlwaysGrant(),
        cancellationToken: new CancellationToken(canceled: true)
      ),
      Throws.InstanceOf<OperationCanceledException>()
    );
#pragma warning restore CA2012
  }

  [Test]
  public void LoadFlashMemoryAsync()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKLOAD
    stream.ResponseWriter.WriteLine("OK");

    Assert.DoesNotThrowAsync(
      async () => await client.LoadFlashMemoryAsync()
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKLOAD\r\n".ToByteSequence())
    );
  }

  [Test]
  public void LoadFlashMemoryAsync_ER10()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKLOAD
    stream.ResponseWriter.WriteLine("FAIL ER10");

    Assert.ThrowsAsync<SkStackFlashMemoryIOException>(
      async () => await client.LoadFlashMemoryAsync()
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKLOAD\r\n".ToByteSequence())
    );
  }

  [Test]
  public void EnableFlashMemoryAutoLoadAsync()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKSREG SFF 1
    stream.ResponseWriter.WriteLine("OK");

    Assert.DoesNotThrowAsync(
      async () => await client.EnableFlashMemoryAutoLoadAsync()
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSREG SFF 1\r\n".ToByteSequence())
    );
  }

  [Test]
  public void DisableFlashMemoryAutoLoadAsync()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKSREG SFF 0
    stream.ResponseWriter.WriteLine("OK");

    Assert.DoesNotThrowAsync(
      async () => await client.DisableFlashMemoryAutoLoadAsync()
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSREG SFF 0\r\n".ToByteSequence())
    );
  }
}
