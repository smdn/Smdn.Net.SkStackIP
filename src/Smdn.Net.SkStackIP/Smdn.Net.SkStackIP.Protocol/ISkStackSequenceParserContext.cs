// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1716

using System.Buffers;

namespace Smdn.Net.SkStackIP.Protocol;

public interface ISkStackSequenceParserContext {
  ReadOnlySequence<byte> UnparsedSequence { get; }

  SequenceReader<byte> CreateReader() => new(UnparsedSequence);
  ISkStackSequenceParserContext CreateCopy();

  void Continue();
  void Complete();
  void Complete(SequenceReader<byte> consumedReader);
  void Ignore();
  void SetAsIncomplete();
  void SetAsIncomplete(SequenceReader<byte> incompleteReader);
}
