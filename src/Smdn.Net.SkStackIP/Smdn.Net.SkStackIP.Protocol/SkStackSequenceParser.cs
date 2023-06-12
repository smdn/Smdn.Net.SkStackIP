// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP.Protocol;

public delegate TResult SkStackSequenceParser<TResult>(
  ISkStackSequenceParserContext context
);
