// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;

namespace Smdn.Net.SkStackIP.Protocol {
  public delegate TResult SkStackSequenceParser<TResult>(
    ISkStackSequenceParserContext context
  );
}