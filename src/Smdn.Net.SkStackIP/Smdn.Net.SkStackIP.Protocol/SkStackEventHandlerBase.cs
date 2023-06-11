// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Net.SkStackIP.Protocol;

internal abstract class SkStackEventHandlerBase {
  public virtual bool DoContinueHandlingEvents(SkStackResponseStatus status) => status != SkStackResponseStatus.Fail;
  public abstract bool TryProcessEvent(SkStackEvent ev);
  public virtual void ProcessSubsequentEvent(ISkStackSequenceParserContext context) { /*do nothing*/ }
}
