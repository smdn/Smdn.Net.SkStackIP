// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Net.SkStackIP {
  /// <summary>BP35A1コマンドリファレンス 3. コマンドリファレンス</summary>
  public class SkStackCommandCodes {
    /// <summary>BP35A1コマンドリファレンス 3.1. SKSREG</summary>
    public static ReadOnlyMemory<byte> SKREG { get; } = SkStack.ToByteSequence("SKREG");

    /// <summary>BP35A1コマンドリファレンス 3.2. SKINFO</summary>
    public static ReadOnlyMemory<byte> SKINFO { get; } = SkStack.ToByteSequence("SKINFO");

#if false
    /// <summary>BP35A1コマンドリファレンス 3.3. SKSTART</summary>
    SKSTART,
    /// <summary>BP35A1コマンドリファレンス 3.4. SKJOIN</summary>
    SKJOIN,
    /// <summary>BP35A1コマンドリファレンス 3.5. SKREJOIN</summary>
    SKREJOIN,
#endif

    /// <summary>BP35A1コマンドリファレンス 3.6. SKTERM</summary>
    public static ReadOnlyMemory<byte> SKTERM { get; } = SkStack.ToByteSequence("SKTERM");

#if false
    /// <summary>BP35A1コマンドリファレンス 3.7. SKSENDTO</summary>
    SKSENDTO,
    /// <summary>BP35A1コマンドリファレンス 3.8. SKPING</summary>
    SKPING,
    /// <summary>BP35A1コマンドリファレンス 3.9. SKSCAN</summary>
    SKSCAN,
    /// <summary>BP35A1コマンドリファレンス 3.10. SKREGDEV</summary>
    SKREGDEV,
    /// <summary>BP35A1コマンドリファレンス 3.11. SKRMDEV</summary>
    SKRMDEV,
    /// <summary>BP35A1コマンドリファレンス 3.12. SKSETKEY</summary>
    SKSETKEY,
    /// <summary>BP35A1コマンドリファレンス 3.13. SKRMKEY</summary>
    SKRMKEY,
    /// <summary>BP35A1コマンドリファレンス 3.14. SKSECENABLE</summary>
    SKSECENABLE,
    /// <summary>BP35A1コマンドリファレンス 3.15. SKSETPSK</summary>
    SKSETPSK,
    /// <summary>BP35A1コマンドリファレンス 3.16. SKSETPWD</summary>
    SKSETPWD,
    /// <summary>BP35A1コマンドリファレンス 3.17. SKSETRBID</summary>
    SKSETRBID,
    /// <summary>BP35A1コマンドリファレンス 3.18. SKADDNBR</summary>
    SKADDNBR,
    /// <summary>BP35A1コマンドリファレンス 3.19. SKUDPPORT</summary>
    SKUDPPORT,
    /// <summary>BP35A1コマンドリファレンス 3.20. SKSAVE</summary>
    SKSAVE,
    /// <summary>BP35A1コマンドリファレンス 3.21. SKLOAD</summary>
    SKLOAD,
    /// <summary>BP35A1コマンドリファレンス 3.22. SKERASE</summary>
    SKERASE,
#endif

    /// <summary>BP35A1コマンドリファレンス 3.23. SKVER</summary>
    public static ReadOnlyMemory<byte> SKVER { get; } = SkStack.ToByteSequence("SKVER");

    /// <summary>BP35A1コマンドリファレンス 3.24. SKAPPVER</summary>
    public static ReadOnlyMemory<byte> SKAPPVER { get; } = SkStack.ToByteSequence("SKAPPVER");

    /// <summary>BP35A1コマンドリファレンス 3.25. SKRESET</summary>
    public static ReadOnlyMemory<byte> SKRESET { get; } = SkStack.ToByteSequence("SKRESET");

#if false
    /// <summary>BP35A1コマンドリファレンス 3.26. SKTABLE</summary>
    SKTABLE,
    /// <summary>BP35A1コマンドリファレンス 3.27. SKDSLEEP</summary>
    SKDSLEEP,
    /// <summary>BP35A1コマンドリファレンス 3.28. SKRFLO</summary>
    SKRFLO,
    /// <summary>BP35A1コマンドリファレンス 3.29. SKLL64</summary>
    SKLL64,
#endif
  }
}