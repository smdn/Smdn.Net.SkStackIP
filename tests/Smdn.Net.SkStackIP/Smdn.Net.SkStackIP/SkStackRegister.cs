// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackRegisterTests {
  [Test]
  public void Name()
  {
    Assert.That(SkStackRegister.S02.Name, Is.EqualTo(nameof(SkStackRegister.S02)), nameof(SkStackRegister.S02));
    Assert.That(SkStackRegister.S03.Name, Is.EqualTo(nameof(SkStackRegister.S03)), nameof(SkStackRegister.S03));
    Assert.That(SkStackRegister.S07.Name, Is.EqualTo(nameof(SkStackRegister.S07)), nameof(SkStackRegister.S07));
    Assert.That(SkStackRegister.S0A.Name, Is.EqualTo(nameof(SkStackRegister.S0A)), nameof(SkStackRegister.S0A));
    Assert.That(SkStackRegister.S15.Name, Is.EqualTo(nameof(SkStackRegister.S15)), nameof(SkStackRegister.S15));
    Assert.That(SkStackRegister.S16.Name, Is.EqualTo(nameof(SkStackRegister.S16)), nameof(SkStackRegister.S16));
    Assert.That(SkStackRegister.S17.Name, Is.EqualTo(nameof(SkStackRegister.S17)), nameof(SkStackRegister.S17));
    Assert.That(SkStackRegister.SA0.Name, Is.EqualTo(nameof(SkStackRegister.SA0)), nameof(SkStackRegister.SA0));
    Assert.That(SkStackRegister.SA1.Name, Is.EqualTo(nameof(SkStackRegister.SA1)), nameof(SkStackRegister.SA1));
    Assert.That(SkStackRegister.SFB.Name, Is.EqualTo(nameof(SkStackRegister.SFB)), nameof(SkStackRegister.SFB));
    Assert.That(SkStackRegister.SFD.Name, Is.EqualTo(nameof(SkStackRegister.SFD)), nameof(SkStackRegister.SFD));
    Assert.That(SkStackRegister.SFE.Name, Is.EqualTo(nameof(SkStackRegister.SFE)), nameof(SkStackRegister.SFE));
    Assert.That(SkStackRegister.SFF.Name, Is.EqualTo(nameof(SkStackRegister.SFF)), nameof(SkStackRegister.SFF));
  }

  [Test]
  public void IsReadable()
  {
    Assert.That(SkStackRegister.S02.IsReadable, Is.True, nameof(SkStackRegister.S02));
    Assert.That(SkStackRegister.S03.IsReadable, Is.True, nameof(SkStackRegister.S03));
    Assert.That(SkStackRegister.S07.IsReadable, Is.True, nameof(SkStackRegister.S07));
    Assert.That(SkStackRegister.S0A.IsReadable, Is.True, nameof(SkStackRegister.S0A));
    Assert.That(SkStackRegister.S15.IsReadable, Is.True, nameof(SkStackRegister.S15));
    Assert.That(SkStackRegister.S16.IsReadable, Is.True, nameof(SkStackRegister.S16));
    Assert.That(SkStackRegister.S17.IsReadable, Is.True, nameof(SkStackRegister.S17));
    Assert.That(SkStackRegister.SA0.IsReadable, Is.True, nameof(SkStackRegister.SA0));
    Assert.That(SkStackRegister.SA1.IsReadable, Is.True, nameof(SkStackRegister.SA1));
    Assert.That(SkStackRegister.SFB.IsReadable, Is.True, nameof(SkStackRegister.SFB));
    Assert.That(SkStackRegister.SFD.IsReadable, Is.True, nameof(SkStackRegister.SFD));
    Assert.That(SkStackRegister.SFE.IsReadable, Is.True, nameof(SkStackRegister.SFE));
    Assert.That(SkStackRegister.SFF.IsReadable, Is.True, nameof(SkStackRegister.SFF));
  }

  [Test]
  public void IsWritable()
  {
    Assert.That(SkStackRegister.S02.IsWritable, Is.True, nameof(SkStackRegister.S02));
    Assert.That(SkStackRegister.S03.IsWritable, Is.True, nameof(SkStackRegister.S03));
    Assert.That(SkStackRegister.S07.IsWritable, Is.False, nameof(SkStackRegister.S07));
    Assert.That(SkStackRegister.S0A.IsWritable, Is.True, nameof(SkStackRegister.S0A));
    Assert.That(SkStackRegister.S15.IsWritable, Is.True, nameof(SkStackRegister.S15));
    Assert.That(SkStackRegister.S16.IsWritable, Is.True, nameof(SkStackRegister.S16));
    Assert.That(SkStackRegister.S17.IsWritable, Is.True, nameof(SkStackRegister.S17));
    Assert.That(SkStackRegister.SA0.IsWritable, Is.True, nameof(SkStackRegister.SA0));
    Assert.That(SkStackRegister.SA1.IsWritable, Is.True, nameof(SkStackRegister.SA1));
    Assert.That(SkStackRegister.SFB.IsWritable, Is.False, nameof(SkStackRegister.SFB));
    Assert.That(SkStackRegister.SFD.IsWritable, Is.False, nameof(SkStackRegister.SFD));
    Assert.That(SkStackRegister.SFE.IsWritable, Is.True, nameof(SkStackRegister.SFE));
    Assert.That(SkStackRegister.SFF.IsWritable, Is.True, nameof(SkStackRegister.SFF));
  }
}
