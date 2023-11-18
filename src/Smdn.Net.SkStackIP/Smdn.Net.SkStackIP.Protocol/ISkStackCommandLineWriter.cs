// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Net.SkStackIP.Protocol;

public interface ISkStackCommandLineWriter {
  void WriteToken(ReadOnlySpan<byte> token);
}
