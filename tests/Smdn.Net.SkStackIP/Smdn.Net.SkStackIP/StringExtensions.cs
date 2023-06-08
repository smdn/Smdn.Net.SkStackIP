// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Net.SkStackIP;

internal static class StringExtensions {
  public static ReadOnlyMemory<byte> ToByteSequence(this string str)
    => str is null ? ReadOnlyMemory<byte>.Empty : Encoding.ASCII.GetBytes(str);
}
