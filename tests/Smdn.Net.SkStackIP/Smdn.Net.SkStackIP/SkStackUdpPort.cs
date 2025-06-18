// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackUdpPortTests {
  [Test]
  public void Null()
  {
    Assert.That(SkStackUdpPort.Null.Handle, Is.EqualTo(SkStackUdpPortHandle.None), nameof(SkStackUdpPort.Null.Handle));
    Assert.That(SkStackUdpPort.Null.IsNull, Is.True, nameof(SkStackUdpPort.Null.IsNull));
    Assert.That(SkStackUdpPort.Null.IsUnused, Is.True, nameof(SkStackUdpPort.Null.IsUnused));
  }

  [Test]
  public void Default()
  {
    SkStackUdpPort defaultPort = default;

    Assert.That(defaultPort.Handle, Is.EqualTo(SkStackUdpPortHandle.None), nameof(defaultPort.Handle));
    Assert.That(defaultPort.IsNull, Is.True, nameof(defaultPort.IsNull));
    Assert.That(defaultPort.IsUnused, Is.True, nameof(defaultPort.IsUnused));
  }
}
