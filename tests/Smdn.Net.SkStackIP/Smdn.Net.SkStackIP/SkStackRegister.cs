// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using NUnit.Framework;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackRegisterTests {
    [Test]
    public void Name()
    {
      Assert.AreEqual(nameof(SkStackRegister.S02), SkStackRegister.S02.Name, nameof(SkStackRegister.S02));
      Assert.AreEqual(nameof(SkStackRegister.S03), SkStackRegister.S03.Name, nameof(SkStackRegister.S03));
      Assert.AreEqual(nameof(SkStackRegister.S07), SkStackRegister.S07.Name, nameof(SkStackRegister.S07));
      Assert.AreEqual(nameof(SkStackRegister.S0A), SkStackRegister.S0A.Name, nameof(SkStackRegister.S0A));
      Assert.AreEqual(nameof(SkStackRegister.S15), SkStackRegister.S15.Name, nameof(SkStackRegister.S15));
      Assert.AreEqual(nameof(SkStackRegister.S16), SkStackRegister.S16.Name, nameof(SkStackRegister.S16));
      Assert.AreEqual(nameof(SkStackRegister.S17), SkStackRegister.S17.Name, nameof(SkStackRegister.S17));
      Assert.AreEqual(nameof(SkStackRegister.SA0), SkStackRegister.SA0.Name, nameof(SkStackRegister.SA0));
      Assert.AreEqual(nameof(SkStackRegister.SA1), SkStackRegister.SA1.Name, nameof(SkStackRegister.SA1));
      Assert.AreEqual(nameof(SkStackRegister.SFB), SkStackRegister.SFB.Name, nameof(SkStackRegister.SFB));
      Assert.AreEqual(nameof(SkStackRegister.SFD), SkStackRegister.SFD.Name, nameof(SkStackRegister.SFD));
      Assert.AreEqual(nameof(SkStackRegister.SFE), SkStackRegister.SFE.Name, nameof(SkStackRegister.SFE));
      Assert.AreEqual(nameof(SkStackRegister.SFF), SkStackRegister.SFF.Name, nameof(SkStackRegister.SFF));
    }

    [Test]
    public void IsReadable()
    {
      Assert.IsTrue(SkStackRegister.S02.IsReadable, nameof(SkStackRegister.S02));
      Assert.IsTrue(SkStackRegister.S03.IsReadable, nameof(SkStackRegister.S03));
      Assert.IsTrue(SkStackRegister.S07.IsReadable, nameof(SkStackRegister.S07));
      Assert.IsTrue(SkStackRegister.S0A.IsReadable, nameof(SkStackRegister.S0A));
      Assert.IsTrue(SkStackRegister.S15.IsReadable, nameof(SkStackRegister.S15));
      Assert.IsTrue(SkStackRegister.S16.IsReadable, nameof(SkStackRegister.S16));
      Assert.IsTrue(SkStackRegister.S17.IsReadable, nameof(SkStackRegister.S17));
      Assert.IsTrue(SkStackRegister.SA0.IsReadable, nameof(SkStackRegister.SA0));
      Assert.IsTrue(SkStackRegister.SA1.IsReadable, nameof(SkStackRegister.SA1));
      Assert.IsTrue(SkStackRegister.SFB.IsReadable, nameof(SkStackRegister.SFB));
      Assert.IsTrue(SkStackRegister.SFD.IsReadable, nameof(SkStackRegister.SFD));
      Assert.IsTrue(SkStackRegister.SFE.IsReadable, nameof(SkStackRegister.SFE));
      Assert.IsTrue(SkStackRegister.SFF.IsReadable, nameof(SkStackRegister.SFF));
    }

    [Test]
    public void IsWritable()
    {
      Assert.IsTrue(SkStackRegister.S02.IsWritable, nameof(SkStackRegister.S02));
      Assert.IsTrue(SkStackRegister.S03.IsWritable, nameof(SkStackRegister.S03));
      Assert.IsFalse(SkStackRegister.S07.IsWritable, nameof(SkStackRegister.S07));
      Assert.IsTrue(SkStackRegister.S0A.IsWritable, nameof(SkStackRegister.S0A));
      Assert.IsTrue(SkStackRegister.S15.IsWritable, nameof(SkStackRegister.S15));
      Assert.IsTrue(SkStackRegister.S16.IsWritable, nameof(SkStackRegister.S16));
      Assert.IsTrue(SkStackRegister.S17.IsWritable, nameof(SkStackRegister.S17));
      Assert.IsTrue(SkStackRegister.SA0.IsWritable, nameof(SkStackRegister.SA0));
      Assert.IsTrue(SkStackRegister.SA1.IsWritable, nameof(SkStackRegister.SA1));
      Assert.IsFalse(SkStackRegister.SFB.IsWritable, nameof(SkStackRegister.SFB));
      Assert.IsFalse(SkStackRegister.SFD.IsWritable, nameof(SkStackRegister.SFD));
      Assert.IsTrue(SkStackRegister.SFE.IsWritable, nameof(SkStackRegister.SFE));
      Assert.IsTrue(SkStackRegister.SFF.IsWritable, nameof(SkStackRegister.SFF));
    }
  }
}