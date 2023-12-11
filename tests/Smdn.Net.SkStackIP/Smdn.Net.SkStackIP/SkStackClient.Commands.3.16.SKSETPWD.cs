// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKSETPWDTests : SkStackClientTestsBase {
  [Test]
  public void SKSETPWD()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    SkStackResponse? response = null;

    Assert.DoesNotThrowAsync(async () => response = await client.SendSKSETPWDAsync("0123456789AB".AsMemory()));

    Assert.IsNotNull(response);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKSETPWD C 0123456789AB\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfChar_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: string.Empty.AsMemory()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfByte_Empty()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: ReadOnlyMemory<byte>.Empty));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfChar_TooLong()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: "012345678901234567890123456789012".AsMemory()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKSETPWD_Password_ReadOnlyMemoryOfByte_TooLong()
  {
    var stream = new PseudoSkStackStream();

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKSETPWDAsync(password: "012345678901234567890123456789012".ToByteSequence()));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase("pass")]
  [TestCase("password")]
  [TestCase("passwordtext")]
  public void SKSETPWD_Password_ReadOnlyMemoryOfChar_MustMaskedForLogger(string password)
    => SKSETPWD_Password_MustMaskedForLogger(
      password,
      sendSKSETPWDAsync: (client, passwd) => client.SendSKSETPWDAsync(passwd.AsMemory())
    );

  [TestCase("pass")]
  [TestCase("password")]
  [TestCase("passwordtext")]
  public void SKSETPWD_Password_ReadOnlyMemoryOfByte_MustMaskedForLogger(string password)
    => SKSETPWD_Password_MustMaskedForLogger(
      password,
      sendSKSETPWDAsync: async (client, passwd) => {
        var passwdBytes = Encoding.ASCII.GetBytes(passwd);

        return await client.SendSKSETPWDAsync(passwdBytes.AsMemory()).ConfigureAwait(false);
      }
    );

  private void SKSETPWD_Password_MustMaskedForLogger(
    string password,
    Func<SkStackClient, string, ValueTask<SkStackResponse>> sendSKSETPWDAsync
  )
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    var logger = CreateLoggerForTestCase();
    using var client = new SkStackClient(stream, logger: logger);
    SkStackResponse? response = null;

    Assert.DoesNotThrowAsync(async () => response = await sendSKSETPWDAsync(client, password));

    Assert.IsNotNull(response);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKSETPWD {password.Length:X} {password}\r\n".ToByteSequence())
    );

    var logSKSETPWDRecorded = false;

    foreach (var log in GetLogsForTestCase() ?? Array.Empty<string>()) {
      if (!log.Contains($"SKSETPWD␠{password.Length:X}␠", StringComparison.Ordinal))
        continue;

      logSKSETPWDRecorded = true;

      StringAssert.DoesNotContain(password, log);
      StringAssert.Contains($"SKSETPWD␠{password.Length:X}␠****", log, "password must be masked");
    }

    if (!logSKSETPWDRecorded)
      Assert.Fail("log for SKSETPWD not recorded");
  }

  [TestCase("pass")]
  [TestCase("password")]
  [TestCase("passwordtext")]
  public void SKSETPWD_Password_ReadOnlyMemoryOfChar_NoLogger(string password)
    => SKSETPWD_Password_NoLogger(
      password,
      sendSKSETPWDAsync: (client, passwd) => client.SendSKSETPWDAsync(passwd.AsMemory())
    );

  [TestCase("pass")]
  [TestCase("password")]
  [TestCase("passwordtext")]
  public void SKSETPWD_Password_ReadOnlyMemoryOfByte_NoLogger(string password)
    => SKSETPWD_Password_NoLogger(
      password,
      sendSKSETPWDAsync: async (client, passwd) => {
        var passwdBytes = Encoding.ASCII.GetBytes(passwd);

        return await client.SendSKSETPWDAsync(passwdBytes.AsMemory()).ConfigureAwait(false);
      }
    );

  private void SKSETPWD_Password_NoLogger(
    string password,
    Func<SkStackClient, string, ValueTask<SkStackResponse>> sendSKSETPWDAsync
  )
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: null);
    SkStackResponse? response = null;

    Assert.DoesNotThrowAsync(async () => response = await sendSKSETPWDAsync(client, password));

    Assert.IsNotNull(response);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKSETPWD {password.Length:X} {password}\r\n".ToByteSequence()),
      "password must not be masked"
    );
  }
}
