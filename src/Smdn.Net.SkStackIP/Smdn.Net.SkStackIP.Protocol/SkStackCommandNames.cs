// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol {
  /// <remarks>reference: BP35A1コマンドリファレンス 3. コマンドリファレンス</remarks>
  internal class SkStackCommandNames {
    /// <remarks>reference: BP35A1コマンドリファレンス 3.1. SKSREG</remarks>
    public static ReadOnlyMemory<byte> SKSREG { get; } = SkStack.ToByteSequence(nameof(SKSREG));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.2. SKINFO</remarks>
    public static ReadOnlyMemory<byte> SKINFO { get; } = SkStack.ToByteSequence(nameof(SKINFO));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.3. SKSTART</remarks>
    public static ReadOnlyMemory<byte> SKSTART => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.4. SKJOIN</remarks>
    public static ReadOnlyMemory<byte> SKJOIN { get; } = SkStack.ToByteSequence(nameof(SKJOIN));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.5. SKREJOIN</remarks>
    public static ReadOnlyMemory<byte> SKREJOIN { get; } = SkStack.ToByteSequence(nameof(SKREJOIN));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.6. SKTERM</remarks>
    public static ReadOnlyMemory<byte> SKTERM { get; } = SkStack.ToByteSequence(nameof(SKTERM));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.7. SKSENDTO</remarks>
    public static ReadOnlyMemory<byte> SKSENDTO { get; } = SkStack.ToByteSequence(nameof(SKSENDTO));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.8. SKPING</remarks>
    public static ReadOnlyMemory<byte> SKPING => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.9. SKSCAN</remarks>
    public static ReadOnlyMemory<byte> SKSCAN { get; } = SkStack.ToByteSequence(nameof(SKSCAN));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.10. SKREGDEV</remarks>
    public static ReadOnlyMemory<byte> SKREGDEV => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.11. SKRMDEV</remarks>
    public static ReadOnlyMemory<byte> SKRMDEV => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.12. SKSETKEY</remarks>
    public static ReadOnlyMemory<byte> SKSETKEY => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.13. SKRMKEY</remarks>
    public static ReadOnlyMemory<byte> SKRMKEY => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.14. SKSECENABLE</remarks>
    public static ReadOnlyMemory<byte> SKSECENABLE => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.15. SKSETPSK</remarks>
    public static ReadOnlyMemory<byte> SKSETPSK => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.16. SKSETPWD</remarks>
    public static ReadOnlyMemory<byte> SKSETPWD { get; } = SkStack.ToByteSequence(nameof(SKSETPWD));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.17. SKSETRBID</remarks>
    public static ReadOnlyMemory<byte> SKSETRBID { get; } = SkStack.ToByteSequence(nameof(SKSETRBID));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.18. SKADDNBR</remarks>
    public static ReadOnlyMemory<byte> SKADDNBR { get; } = SkStack.ToByteSequence(nameof(SKADDNBR));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.19. SKUDPPORT</remarks>
    public static ReadOnlyMemory<byte> SKUDPPORT { get; } = SkStack.ToByteSequence(nameof(SKUDPPORT));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.20. SKSAVE</remarks>
    public static ReadOnlyMemory<byte> SKSAVE => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.21. SKLOAD</remarks>
    public static ReadOnlyMemory<byte> SKLOAD => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.22. SKERASE</remarks>
    public static ReadOnlyMemory<byte> SKERASE => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.23. SKVER</remarks>
    public static ReadOnlyMemory<byte> SKVER { get; } = SkStack.ToByteSequence(nameof(SKVER));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.24. SKAPPVER</remarks>
    public static ReadOnlyMemory<byte> SKAPPVER { get; } = SkStack.ToByteSequence(nameof(SKAPPVER));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.25. SKRESET</remarks>
    public static ReadOnlyMemory<byte> SKRESET { get; } = SkStack.ToByteSequence(nameof(SKRESET));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.26. SKTABLE</remarks>
    public static ReadOnlyMemory<byte> SKTABLE { get; } = SkStack.ToByteSequence(nameof(SKTABLE));
    /// <remarks>reference: BP35A1コマンドリファレンス 3.27. SKDSLEEP</remarks>
    public static ReadOnlyMemory<byte> SKDSLEEP => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.28. SKRFLO</remarks>
    public static ReadOnlyMemory<byte> SKRFLO => throw new NotImplementedException();
    /// <remarks>reference: BP35A1コマンドリファレンス 3.29. SKLL64</remarks>
    public static ReadOnlyMemory<byte> SKLL64 { get; } = SkStack.ToByteSequence(nameof(SKLL64));
  }
}