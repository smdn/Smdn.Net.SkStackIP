// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

using Microsoft.Extensions.Logging;

namespace Smdn.Net.SkStackIP.Protocol {
  public interface ISkStackSequenceParserContext {
    ReadOnlySequence<byte> UnparsedSequence { get; }
    object State { get; set; }
    ILogger Logger { get; }

    SequenceReader<byte> CreateReader() => new(UnparsedSequence);
    ISkStackSequenceParserContext CreateCopy();

    T GetOrCreateState<T>(Func<T> createState)
    {
      if (State is null) {
        var s = createState();

        State = s;

        return s;
      }

      return (T)State;
    }

    void Continue();
    void Complete();
    void Complete(SequenceReader<byte> consumedReader);
    void Ignore();
    void SetAsIncomplete();
    void SetAsIncomplete(SequenceReader<byte> incompleteReader);
  }
}
