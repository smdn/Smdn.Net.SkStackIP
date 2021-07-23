// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol {
  public abstract class SkStackProtocolSyntax {
    public static SkStackProtocolSyntax Default { get; } = new DefaultSyntax();

    private sealed class DefaultSyntax : SkStackProtocolSyntax {
      public override ReadOnlySpan<byte> EndOfCommandLine => SkStack.CRLFSpan;
      public override bool ExpectStatusLine => true;
      public override ReadOnlySpan<byte> EndOfStatusLine => SkStack.CRLFSpan;
    }

    internal static SkStackProtocolSyntax SKSENDTO { get; } = new SKSENDTOSyntax();

    private sealed class SKSENDTOSyntax : SkStackProtocolSyntax {
      public override ReadOnlySpan<byte> EndOfCommandLine => ReadOnlySpan<byte>.Empty;
      public override ReadOnlySpan<byte> EndOfEchobackLine => SkStack.CRLFSpan;
      public override bool ExpectStatusLine => true;
      public override ReadOnlySpan<byte> EndOfStatusLine => SkStack.CRLFSpan;
    }

    internal static SkStackProtocolSyntax SKLL64 { get; } = new SKLL64Syntax();

    private sealed class SKLL64Syntax : SkStackProtocolSyntax {
      public override ReadOnlySpan<byte> EndOfCommandLine => SkStack.CRLFSpan;
      public override bool ExpectStatusLine => false;
      public override ReadOnlySpan<byte> EndOfStatusLine => SkStack.CRLFSpan;
    }

    protected SkStackProtocolSyntax()
    {
    }

    public abstract ReadOnlySpan<byte> EndOfCommandLine { get; }
    public virtual ReadOnlySpan<byte> EndOfEchobackLine => EndOfCommandLine;
    public abstract bool ExpectStatusLine { get; }
    public abstract ReadOnlySpan<byte> EndOfStatusLine { get; }
  }
}