// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  partial class SkStackRegister {
    public abstract class RegisterEntry<TValue> {
      public string Name { get; }
      internal ReadOnlyMemory<byte> SREG { get; }
      public bool IsReadable { get; }
      public bool IsWritable { get; }
      public TValue MinValue { get; }
      public TValue MaxValue { get; }

      internal delegate ReadOnlyMemory<byte> CreateSKSREGArgumentFunc(TValue value);
      internal CreateSKSREGArgumentFunc CreateSKSREGArgument { get; }

      private protected delegate bool ExpectValueFunc(ref SequenceReader<byte> reader, out TValue value);
      private ExpectValueFunc ExpectValue { get; }

      private protected RegisterEntry(
        string name,
        (bool isReadable, bool isWritable) readWrite,
        (TValue minValue, TValue maxValue) valueRange,
        CreateSKSREGArgumentFunc createSKSREGArgument,
        ExpectValueFunc expectValue
      )
      {
        if (readWrite.isWritable && createSKSREGArgument is null)
          throw new ArgumentNullException(nameof(createSKSREGArgument));
        if (readWrite.isReadable && expectValue is null)
          throw new ArgumentNullException(nameof(expectValue));

        this.Name = name;
        this.SREG = SkStack.ToByteSequence(name);
        this.IsReadable = readWrite.isReadable;
        this.IsWritable = readWrite.isWritable;
        this.MinValue = valueRange.minValue;
        this.MaxValue = valueRange.maxValue;
        this.CreateSKSREGArgument = createSKSREGArgument;
        this.ExpectValue = expectValue;
      }

      internal virtual void ThrowIfValueIsNotInRange(TValue value, string paramName)
      {
        if (!IsInRange(value))
          throw new ArgumentOutOfRangeException(paramName, value, $"must be in range of {MinValue}~{MaxValue}");
      }

      private protected abstract bool IsInRange(TValue value);

      private readonly ReadOnlyMemory<byte> ESREG = SkStack.ToByteSequence("ESREG");

      internal TValue ParseESREG(
        ISkStackSequenceParserContext context
      )
      {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, ESREG) &&
          ExpectValue(ref reader, out var result) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return result;
        }

        context.SetAsIncomplete();
        return default;
      }
    }

    private abstract class ComparableValueRegisterEntry<TValue> :
      RegisterEntry<TValue>
      where TValue : IComparable<TValue>
    {
      private protected ComparableValueRegisterEntry(
        string name,
        (bool isReadable, bool isWritable) readWrite,
        (TValue minValue, TValue maxValue) valueRange,
        CreateSKSREGArgumentFunc createSKSREGArgument,
        ExpectValueFunc expectValue
      )
        : base(
          name: name,
          readWrite: readWrite,
          valueRange: valueRange,
          createSKSREGArgument: createSKSREGArgument,
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
          createSKSREGArgument: static (value) => SkStackCommandArgs.GetHex(value ? 1 : 0),
          expectValue: SkStackTokenParser.ExpectBinary
        )
      {
      }

      private protected override bool IsInRange(bool value) => true;
    }

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
          createSKSREGArgument: static (value) => SkStackCommandArgs.ConvertToUINT8(value),
          expectValue: SkStackTokenParser.ExpectUINT8
        )
      {
      }
    }

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
          createSKSREGArgument: static (value) => SkStackCommandArgs.ConvertToUINT8(value.RegisterS02Value),
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
          createSKSREGArgument: static (value) => SkStackCommandArgs.ConvertToUINT16(value),
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
          createSKSREGArgument: static (value) => SkStackCommandArgs.ConvertToUINT32(value),
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
          createSKSREGArgument: static (value) => SkStackCommandArgs.ConvertToUINT32((uint)value.TotalSeconds),
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
          createSKSREGArgument: static (value) => SkStackCommandArgs.ConvertToUINT64(value),
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
          createSKSREGArgument: static (value) => value,
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
}