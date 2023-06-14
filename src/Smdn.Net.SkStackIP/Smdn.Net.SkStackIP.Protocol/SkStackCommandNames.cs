// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 3. コマンドリファレンス' for detailed specifications.</para>
/// </remarks>
internal class SkStackCommandNames {
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.1. SKSREG' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSREG { get; } = SkStack.ToByteSequence(nameof(SKSREG));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.2. SKINFO' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKINFO { get; } = SkStack.ToByteSequence(nameof(SKINFO));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.3. SKSTART' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSTART => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.4. SKJOIN' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKJOIN { get; } = SkStack.ToByteSequence(nameof(SKJOIN));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.5. SKREJOIN' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKREJOIN { get; } = SkStack.ToByteSequence(nameof(SKREJOIN));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.6. SKTERM' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKTERM { get; } = SkStack.ToByteSequence(nameof(SKTERM));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.7. SKSENDTO' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSENDTO { get; } = SkStack.ToByteSequence(nameof(SKSENDTO));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.8. SKPING' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKPING => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.9. SKSCAN' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSCAN { get; } = SkStack.ToByteSequence(nameof(SKSCAN));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.10. SKREGDEV' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKREGDEV => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.11. SKRMDEV' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKRMDEV => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.12. SKSETKEY' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSETKEY => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.13. SKRMKEY' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKRMKEY => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.14. SKSECENABLE' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSECENABLE => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.15. SKSETPSK' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSETPSK => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.16. SKSETPWD' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSETPWD { get; } = SkStack.ToByteSequence(nameof(SKSETPWD));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.17. SKSETRBID' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSETRBID { get; } = SkStack.ToByteSequence(nameof(SKSETRBID));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.18. SKADDNBR' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKADDNBR { get; } = SkStack.ToByteSequence(nameof(SKADDNBR));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.19. SKUDPPORT' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKUDPPORT { get; } = SkStack.ToByteSequence(nameof(SKUDPPORT));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.20. SKSAVE' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKSAVE { get; } = SkStack.ToByteSequence(nameof(SKSAVE));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.21. SKLOAD' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKLOAD { get; } = SkStack.ToByteSequence(nameof(SKLOAD));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.22. SKERASE' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKERASE { get; } = SkStack.ToByteSequence(nameof(SKERASE));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.23. SKVER' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKVER { get; } = SkStack.ToByteSequence(nameof(SKVER));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.24. SKAPPVER' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKAPPVER { get; } = SkStack.ToByteSequence(nameof(SKAPPVER));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.25. SKRESET' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKRESET { get; } = SkStack.ToByteSequence(nameof(SKRESET));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.26. SKTABLE' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKTABLE { get; } = SkStack.ToByteSequence(nameof(SKTABLE));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.27. SKDSLEEP' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKDSLEEP { get; } = SkStack.ToByteSequence(nameof(SKDSLEEP));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.28. SKRFLO' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKRFLO => throw new NotImplementedException();

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.29. SKLL64' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> SKLL64 { get; } = SkStack.ToByteSequence(nameof(SKLL64));

#if false
  /// <summary>`SKSLEEP` undocumented command.</summary>
  public static ReadOnlyMemory<byte> SKSLEEP => throw new NotImplementedException();
#endif
}
