// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackChannelTests {
  [Test]
  public void IsEmpty()
  {
    Assert.That(SkStackChannel.Empty.IsEmpty, Is.True, nameof(SkStackChannel.Empty));
    Assert.That(default(SkStackChannel).IsEmpty, Is.True, "default");
    Assert.That(SkStackChannel.Channel33.IsEmpty, Is.False, nameof(SkStackChannel.Channel33));
  }

  [Test]
  public void Equals_OfObject()
  {
    Assert.That(SkStackChannel.Channel33.Equals((object)null!), Is.False, "case #1");
    Assert.That(SkStackChannel.Channel33.Equals(33), Is.False, "case #2");
    Assert.That(SkStackChannel.Channel33.Equals((object)SkStackChannel.Channel33), Is.True, "case #3");
    Assert.That(SkStackChannel.Channel33.Equals((object)SkStackChannel.Channel34), Is.False, "case #4");
    Assert.That(SkStackChannel.Empty.Equals((object)null!), Is.False, "case #5");
  }

  [Test]
  public void Equals_OfSkStackChannel()
  {
    Assert.That(SkStackChannel.Channel33.Equals(SkStackChannel.Channel33), Is.True, "case #1");
    Assert.That(SkStackChannel.Channel33.Equals(SkStackChannel.Channel34), Is.False, "case #2");
    Assert.That(SkStackChannel.Empty.Equals(SkStackChannel.Channel33), Is.False, "case #3");
    Assert.That(SkStackChannel.Empty.Equals(SkStackChannel.Empty), Is.True, "case #4");
  }

  [Test]
  public void OpEquality()
  {
    Assert.That(SkStackChannel.Channel33 == SkStackChannel.Channel33, Is.True, "case #1");
    Assert.That(SkStackChannel.Channel33 == SkStackChannel.Channel34, Is.False, "case #2");
    Assert.That(SkStackChannel.Channel33 == SkStackChannel.Empty, Is.False, "case #3");
    Assert.That(SkStackChannel.Empty == default, Is.True, "case #4");
  }

  [Test]
  public void OpInequality()
  {
    Assert.That(SkStackChannel.Channel33 != SkStackChannel.Channel33, Is.False, "case #1");
    Assert.That(SkStackChannel.Channel33 != SkStackChannel.Channel34, Is.True, "case #2");
    Assert.That(SkStackChannel.Channel33 != SkStackChannel.Empty, Is.True, "case #3");
    Assert.That(SkStackChannel.Empty != default, Is.False, "case #4");
  }
}
