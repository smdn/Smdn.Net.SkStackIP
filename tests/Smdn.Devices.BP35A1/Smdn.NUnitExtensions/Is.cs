// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.NUnitExtensions {
  public class Is : NUnit.Framework.Is {
    public static ReadOnlyByteMemoryEqualConstraint Equal(ReadOnlyMemory<byte> expected)
      => new ReadOnlyByteMemoryEqualConstraint(expected);
  }
}