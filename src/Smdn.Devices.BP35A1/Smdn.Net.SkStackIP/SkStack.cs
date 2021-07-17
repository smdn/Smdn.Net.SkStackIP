// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Net.SkStackIP {
  internal static class SkStack {
    internal static byte[] ToByteSequence(string command) => Encoding.ASCII.GetBytes(command);
  }
}