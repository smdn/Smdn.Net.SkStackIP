// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackChannelTests {
  [Test]
  public void CreateMask()
  {
    Assert.That(
      SkStackChannel.CreateMask(SkStackChannel.Channel33),
      Is.EqualTo(0b_0000_0000_0000_0000_0000_0000_0000_0001u)
    );
    Assert.That(
      SkStackChannel.CreateMask(SkStackChannel.Channel60),
      Is.EqualTo(0b_0000_1000_0000_0000_0000_0000_0000_0000u)
    );

    Assert.That(
      SkStackChannel.CreateMask(),
      Is.EqualTo(0b_0000_0000_0000_0000_0000_0000_0000_0000u)
    );

    Assert.That(
      SkStackChannel.CreateMask(SkStackChannel.Channel33, SkStackChannel.Channel33),
      Is.EqualTo(0b_0000_0000_0000_0000_0000_0000_0000_0001u)
    );
    Assert.That(
      SkStackChannel.CreateMask(SkStackChannel.Channel33, SkStackChannel.Channel34),
      Is.EqualTo(0b_0000_0000_0000_0000_0000_0000_0000_0011u)
    );
    Assert.That(
      SkStackChannel.CreateMask(SkStackChannel.Channel33, SkStackChannel.Channel34, SkStackChannel.Channel35),
      Is.EqualTo(0b_0000_0000_0000_0000_0000_0000_0000_0111u)
    );
    Assert.That(
      SkStackChannel.CreateMask(SkStackChannel.Channel33, SkStackChannel.Channel60),
      Is.EqualTo(0b_0000_1000_0000_0000_0000_0000_0000_0001u)
    );
  }

  [Test]
  public void CreateMask_ArgumentNull()
    => Assert.That(() => SkStackChannel.CreateMask(channels: null!), Throws.ArgumentNullException);

  private static System.Collections.IEnumerable YieldTestCases_CreateMask_InvalidChannel()
  {
    yield return SkStackChannel.Empty;
    // yield return SkStackChannel.Channels[32]; // cannot test
    // yield return SkStackChannel.Channels[61]; // cannot test
  }

  [TestCaseSource(nameof(YieldTestCases_CreateMask_InvalidChannel))]
  public void CreateMask_InvalidChannel(SkStackChannel invalidChannel)
  {
    Assert.That(() => SkStackChannel.CreateMask(invalidChannel), Throws.InvalidOperationException);
    Assert.That(() => SkStackChannel.CreateMask(SkStackChannel.Channel33, invalidChannel), Throws.InvalidOperationException);
    Assert.That(() => SkStackChannel.CreateMask(SkStackChannel.Channel60, invalidChannel), Throws.InvalidOperationException);
  }

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
