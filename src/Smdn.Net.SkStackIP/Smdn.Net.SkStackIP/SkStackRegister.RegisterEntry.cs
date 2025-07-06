// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable SA1316
#pragma warning disable CA1034

using System;
using System.Buffers;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackRegister {
#pragma warning restore IDE0040
  public abstract class RegisterEntry<TValue> {
    public string Name { get; }
    internal ReadOnlyMemory<byte> SREG { get; }
    public bool IsReadable { get; }
    public bool IsWritable { get; }
    public TValue MinValue { get; }
    public TValue MaxValue { get; }

    internal delegate void WriteSKSREGArgumentFunc(ISkStackCommandLineWriter writer, TValue value);
    private WriteSKSREGArgumentFunc WriteSKSREGArgument { get; }

    private protected delegate bool ExpectValueFunc(ref SequenceReader<byte> reader, out TValue value);
    private ExpectValueFunc ExpectValue { get; }

    private protected RegisterEntry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (TValue minValue, TValue maxValue) valueRange,
      WriteSKSREGArgumentFunc writeSKSREGArgument,
      ExpectValueFunc expectValue
    )
    {
      if (readWrite.isWritable && writeSKSREGArgument is null)
        throw new ArgumentNullException(nameof(writeSKSREGArgument));
      if (readWrite.isReadable && expectValue is null)
        throw new ArgumentNullException(nameof(expectValue));

      Name = name;
      SREG = SkStack.ToByteSequence(name);
      IsReadable = readWrite.isReadable;
      IsWritable = readWrite.isWritable;
      MinValue = valueRange.minValue;
      MaxValue = valueRange.maxValue;
      WriteSKSREGArgument = writeSKSREGArgument;
      ExpectValue = expectValue;
    }

    internal virtual void ThrowIfValueIsNotInRange(TValue value, string paramName)
    {
      if (!IsInRange(value))
        throw new ArgumentOutOfRangeException(paramName, value, $"must be in range of {MinValue}~{MaxValue}");
    }

    private protected abstract bool IsInRange(TValue value);

    internal TValue? ParseESREG(
      ISkStackSequenceParserContext context
    )
    {
      var reader = context.CreateReader();

      if (
        SkStackTokenParser.ExpectToken(ref reader, "ESREG"u8) &&
        ExpectValue(ref reader, out var result) &&
        SkStackTokenParser.ExpectEndOfLine(ref reader)
      ) {
        context.Complete(reader);
        return result;
      }

      context.SetAsIncomplete();
      return default;
    }

    internal void WriteValueTo(ISkStackCommandLineWriter writer, TValue value)
      => WriteSKSREGArgument(writer, value);
  }

  private abstract class ComparableValueRegisterEntry<TValue> :
    RegisterEntry<TValue>
    where TValue : IComparable<TValue> {
    private protected ComparableValueRegisterEntry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (TValue minValue, TValue maxValue) valueRange,
      WriteSKSREGArgumentFunc writeSKSREGArgument,
      ExpectValueFunc expectValue
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: valueRange,
        writeSKSREGArgument: writeSKSREGArgument,
        expectValue: expectValue
      )
    {
    }

    private protected override bool IsInRange(TValue value)
      => MinValue.CompareTo(value) <= 0 && 0 <= MaxValue.CompareTo(value);
  }

  private sealed class RegisterBinaryEntry : ComparableValueRegisterEntry<bool> {
    public RegisterBinaryEntry(
      string name,
      (bool isReadable, bool isWritable) readWrite
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: (minValue: false, maxValue: true),
        writeSKSREGArgument: static (writer, value) => writer.WriteTokenBinary(value),
        expectValue: SkStackTokenParser.ExpectBinary
      )
    {
    }

    private protected override bool IsInRange(bool value) => true;
  }

#if false // unused
  private sealed class RegisterUINT8Entry : ComparableValueRegisterEntry<byte> {
    public RegisterUINT8Entry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (byte minValue, byte maxValue) valueRange
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: valueRange,
        writeSKSREGArgument: static (writer, value) => writer.WriteTokenUINT8(value),
        expectValue: SkStackTokenParser.ExpectUINT8
      )
    {
    }
  }
#endif

  private sealed class RegisterChannelEntry : ComparableValueRegisterEntry<SkStackChannel> {
    public RegisterChannelEntry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (SkStackChannel minValue, SkStackChannel maxValue) valueRange
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: valueRange,
        writeSKSREGArgument: static (writer, value) => writer.WriteTokenUINT8(value.RegisterS02Value),
        expectValue: SkStackTokenParser.ExpectCHANNEL
      )
    {
    }
  }

  private sealed class RegisterUINT16Entry : ComparableValueRegisterEntry<ushort> {
    public RegisterUINT16Entry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (ushort minValue, ushort maxValue) valueRange
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: valueRange,
        writeSKSREGArgument: static (writer, value) => writer.WriteTokenUINT16(value),
        expectValue: SkStackTokenParser.ExpectUINT16
      )
    {
    }
  }

  private sealed class RegisterUINT32Entry : ComparableValueRegisterEntry<uint> {
    public RegisterUINT32Entry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (uint minValue, uint maxValue) valueRange
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: valueRange,
        writeSKSREGArgument: static (writer, value) => writer.WriteTokenUINT32(value),
        expectValue: SkStackTokenParser.ExpectUINT32
      )
    {
    }
  }

  private sealed class RegisterUINT32SecondsTimeSpanEntry : ComparableValueRegisterEntry<TimeSpan> {
    public RegisterUINT32SecondsTimeSpanEntry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (uint minValue, uint maxValue) valueRange
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: (
          minValue: TimeSpan.FromSeconds(valueRange.minValue),
          maxValue: TimeSpan.FromSeconds(valueRange.maxValue)
        ),
        writeSKSREGArgument: static (writer, value) => writer.WriteTokenUINT32((uint)value.TotalSeconds),
        expectValue: ExpectValue
      )
    {
    }

    private static bool ExpectValue(
      ref SequenceReader<byte> reader,
      out TimeSpan value
    )
    {
      value = default;

      if (SkStackTokenParser.ExpectUINT32(ref reader, out var seconds)) {
        value = TimeSpan.FromSeconds(seconds);
        return true;
      }

      return false;
    }
  }

  private sealed class RegisterUINT64Entry : ComparableValueRegisterEntry<ulong> {
    public RegisterUINT64Entry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      (uint minValue, uint maxValue) valueRange
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: valueRange,
        writeSKSREGArgument: static (writer, value) => writer.WriteTokenUINT64(value),
        expectValue: SkStackTokenParser.ExpectUINT64
      )
    {
    }
  }

  private sealed class RegisterCHARArrayEntry : RegisterEntry<ReadOnlyMemory<byte>> {
    private readonly int minLength;
    private readonly int maxLength;

    public RegisterCHARArrayEntry(
      string name,
      (bool isReadable, bool isWritable) readWrite,
      int minLength,
      int maxLength
    )
      : base(
        name: name,
        readWrite: readWrite,
        valueRange: default,
        writeSKSREGArgument: static (writer, value) => writer.WriteToken(value.Span),
        expectValue: SkStackTokenParser.ExpectCharArray
      )
    {
      this.minLength = minLength;
      this.maxLength = maxLength;
    }

    private protected override bool IsInRange(ReadOnlyMemory<byte> value) => throw new NotImplementedException();

    internal override void ThrowIfValueIsNotInRange(ReadOnlyMemory<byte> value, string paramName)
    {
      if (value.IsEmpty)
        throw new ArgumentException("must be non-empty value", paramName);
      if (!(minLength <= value.Length && value.Length <= maxLength))
        throw new ArgumentOutOfRangeException(paramName, value, $"length of {paramName} must be in range of {minLength}~{maxLength}");
    }
  }
}
