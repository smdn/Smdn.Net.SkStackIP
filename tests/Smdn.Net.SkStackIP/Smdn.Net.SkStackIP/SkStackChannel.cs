// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackChannelTests {
  [Test]
  public void Equals_OfObject()
  {
    Assert.IsFalse(SkStackChannel.Channel33.Equals((object)null!), "case #1");
    Assert.IsFalse(SkStackChannel.Channel33.Equals(33), "case #2");
    Assert.IsTrue(SkStackChannel.Channel33.Equals((object)SkStackChannel.Channel33), "case #3");
    Assert.IsFalse(SkStackChannel.Channel33.Equals((object)SkStackChannel.Channel34), "case #4");
  }

  [Test]
  public void Equals_OfSkStackChannel()
  {
    Assert.IsTrue(SkStackChannel.Channel33.Equals(SkStackChannel.Channel33), "case #1");
    Assert.IsFalse(SkStackChannel.Channel33.Equals(SkStackChannel.Channel34), "case #1");
  }

  [Test]
  public void OpEquality()
  {
    Assert.IsTrue(SkStackChannel.Channel33 == SkStackChannel.Channel33, "case #1");
    Assert.IsFalse(SkStackChannel.Channel33 == SkStackChannel.Channel34, "case #2");
  }

  [Test]
  public void OpInequality()
  {
    Assert.IsFalse(SkStackChannel.Channel33 != SkStackChannel.Channel33, "case #1");
    Assert.IsTrue(SkStackChannel.Channel33 != SkStackChannel.Channel34, "case #2");
  }
}
