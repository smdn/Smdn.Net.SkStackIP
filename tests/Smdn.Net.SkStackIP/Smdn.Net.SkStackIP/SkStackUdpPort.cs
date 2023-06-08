// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackUdpPortTests {
  [Test]
  public void Null()
  {
    Assert.IsFalse(Enum.IsDefined(typeof(SkStackUdpPortHandle), SkStackUdpPort.Null.Handle), $"IsDefined {nameof(SkStackUdpPort.Null.Handle)}");
    Assert.True(SkStackUdpPort.Null.IsNull, nameof(SkStackUdpPort.Null.IsNull));
    Assert.True(SkStackUdpPort.Null.IsUnused, nameof(SkStackUdpPort.Null.IsUnused));
  }

  [Test]
  public void Default()
  {
    SkStackUdpPort defaultPort = default;

    Assert.IsFalse(Enum.IsDefined(typeof(SkStackUdpPortHandle), defaultPort.Handle), $"IsDefined {nameof(defaultPort.Handle)}");
    Assert.True(defaultPort.IsNull, nameof(defaultPort.IsNull));
    Assert.True(defaultPort.IsUnused, nameof(defaultPort.IsUnused));
  }
}
