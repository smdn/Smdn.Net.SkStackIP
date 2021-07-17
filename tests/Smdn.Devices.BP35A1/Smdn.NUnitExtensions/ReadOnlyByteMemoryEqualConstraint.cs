// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using NUnit.Framework.Constraints;

namespace Smdn.NUnitExtensions {
  public class ReadOnlyByteMemoryEqualConstraintResult : EqualConstraintResult {
    private readonly ReadOnlyMemory<byte> expectedValue;

    public ReadOnlyByteMemoryEqualConstraintResult(ReadOnlyByteMemoryEqualConstraint constraint, object actual, bool hasSucceeded)
      : base(constraint, actual, hasSucceeded)
    {
      this.expectedValue = constraint.Expected;
    }

    public override void WriteMessageTo(MessageWriter writer)
    {
      writer.WriteMessageLine($"Expected: \"{Encoding.ASCII.GetString(expectedValue.Span)}\" ({BitConverter.ToString(expectedValue.ToArray())})");

      if (ActualValue is ReadOnlyMemory<byte> actualMemory)
        writer.WriteLine($"But was: \"{Encoding.ASCII.GetString(actualMemory.Span)}\" ({BitConverter.ToString(actualMemory.ToArray())})");
      else
        writer.WriteActualValue(ActualValue);
    }
  }

  public class ReadOnlyByteMemoryEqualConstraint : EqualConstraint {
    public ReadOnlyMemory<byte> Expected { get; }
    private Tolerance _tolerance = Tolerance.Default;

    public ReadOnlyByteMemoryEqualConstraint(ReadOnlyMemory<byte> expected)
      : base(expected)
    {
      this.Expected = expected;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
      if (actual is ReadOnlyMemory<byte> actualMemory)
        return new ReadOnlyByteMemoryEqualConstraintResult(this, actualMemory, Expected.Span.SequenceEqual(actualMemory.Span));
      else
        return new EqualConstraintResult(this, actual, new NUnitEqualityComparer().AreEqual(Expected, actual, ref _tolerance));
    }

    public override string Description => "ReadOnlyMemory<byte>.SequenceEquals";
  }
}
