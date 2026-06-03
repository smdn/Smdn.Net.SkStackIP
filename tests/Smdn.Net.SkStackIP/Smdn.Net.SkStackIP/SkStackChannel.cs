// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackChannelTests {
  private static System.Collections.IEnumerable YieldTestCases_CreateMask()
  {
    yield return new object[] {
      new[] { SkStackChannel.Channel33 },
      0b_0000_0000_0000_0000_0000_0000_0000_0001u
    };
    yield return new object[] {
      new[] { SkStackChannel.Channel60 },
      0b_0000_1000_0000_0000_0000_0000_0000_0000u
    };
    yield return new object[] {
      Array.Empty<SkStackChannel>(),
      0b_0000_0000_0000_0000_0000_0000_0000_0000u
    };
    yield return new object[] {
      new[] { SkStackChannel.Channel33, SkStackChannel.Channel33 },
      0b_0000_0000_0000_0000_0000_0000_0000_0001u
    };
    yield return new object[] {
      new[] { SkStackChannel.Channel33, SkStackChannel.Channel34 },
      0b_0000_0000_0000_0000_0000_0000_0000_0011u
    };
    yield return new object[] {
      new[] { SkStackChannel.Channel33, SkStackChannel.Channel34, SkStackChannel.Channel35 },
      0b_0000_0000_0000_0000_0000_0000_0000_0111u
    };
    yield return new object[] {
      new[] { SkStackChannel.Channel33, SkStackChannel.Channel60 },
      0b_0000_1000_0000_0000_0000_0000_0000_0001u
    };
  }

  [TestCaseSource(nameof(YieldTestCases_CreateMask))]
  public void CreateMask_OfParamsArray(SkStackChannel[] channels, uint expected)
    => Assert.That(
      SkStackChannel.CreateMask(channels),
      Is.EqualTo(expected)
    );

  [Test]
  public void CreateMask_OfParamsArray_ArgumentNull()
  {
    SkStackChannel[] channels = null!;

    Assert.That(
      () => SkStackChannel.CreateMask(channels: channels),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("channels")
    );
  }

  [TestCaseSource(nameof(YieldTestCases_CreateMask))]
  public void CreateMask_OfParamsIEnumerable(SkStackChannel[] channels, uint expected)
    => Assert.That(
      SkStackChannel.CreateMask((IEnumerable<SkStackChannel>)channels),
      Is.EqualTo(expected)
    );

  [Test]
  public void CreateMask_OfParamsIEnumerable_ArgumentNull()
  {
    IEnumerable<SkStackChannel> channels = null!;

    Assert.That(
      () => SkStackChannel.CreateMask(channels: channels),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("channels")
    );
  }

  [TestCaseSource(nameof(YieldTestCases_CreateMask))]
  public void CreateMask_OfParamsReadOnlySpan(SkStackChannel[] channels, uint expected)
    => Assert.That(
      SkStackChannel.CreateMask(channels.AsSpan()),
      Is.EqualTo(expected)
    );

  private static System.Collections.IEnumerable YieldTestCases_CreateMask_InvalidChannel()
  {
    yield return new[] { SkStackChannel.Empty };
    yield return new[] { SkStackChannel.Channel33, SkStackChannel.Empty };
    yield return new[] { SkStackChannel.Empty, SkStackChannel.Channel60 };

    // yield return SkStackChannel.Channels[32]; // cannot test
    // yield return SkStackChannel.Channels[61]; // cannot test
  }

  [TestCaseSource(nameof(YieldTestCases_CreateMask_InvalidChannel))]
  public void CreateMask_OfParamsArray_InvalidChannel(SkStackChannel[] channels)
    => Assert.That(() => SkStackChannel.CreateMask(channels), Throws.InvalidOperationException);

  [TestCaseSource(nameof(YieldTestCases_CreateMask_InvalidChannel))]
  public void CreateMask_OfParamsIEnumerable_InvalidChannel(SkStackChannel[] channels)
    => Assert.That(() => SkStackChannel.CreateMask((IEnumerable<SkStackChannel>)channels), Throws.InvalidOperationException);

  [TestCaseSource(nameof(YieldTestCases_CreateMask_InvalidChannel))]
  public void CreateMask_OfParamsReadOnlySpan_InvalidChannel(SkStackChannel[] channels)
    => Assert.That(() => SkStackChannel.CreateMask(channels.AsSpan()), Throws.InvalidOperationException);

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
    Assert.That(SkStackChannel.Channel33.Equals(null!), Is.False, "case #1");
    Assert.That(SkStackChannel.Channel33.Equals(33), Is.False, "case #2");
    Assert.That(SkStackChannel.Channel33.Equals((object)SkStackChannel.Channel33), Is.True, "case #3");
    Assert.That(SkStackChannel.Channel33.Equals((object)SkStackChannel.Channel34), Is.False, "case #4");
    Assert.That(SkStackChannel.Empty.Equals(null!), Is.False, "case #5");
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
