// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_TEXT_ASCII
using System.Text;
#endif
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  private const int SKSETRBIDLengthOfId = 32;

  /// <summary>
  ///   <para>Sends a command <c>SKSETRBID</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.17. SKSETRBID' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKSETRBIDAsync(
    ReadOnlyMemory<char> id,
    CancellationToken cancellationToken = default
  )
  {
    if (id.Length != SKSETRBIDLengthOfId)
      throw new ArgumentException($"length of `{nameof(id)}` must be exact {SKSETRBIDLengthOfId}", nameof(id));
#if SYSTEM_TEXT_ASCII
    if (!Ascii.IsValid(id.Span))
      throw new ArgumentException($"`{nameof(id)}` contains invalid characters for ASCII sequence", paramName: nameof(id));
#endif

    return SendCommandAsync(
      command: SkStackCommandNames.SKSETRBID,
      writeArguments: writer => writer.WriteToken(id.Span),
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }

  /// <summary>
  ///   <para>Sends a command <c>SKSETRBID</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.17. SKSETRBID' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKSETRBIDAsync(
    ReadOnlyMemory<byte> id,
    CancellationToken cancellationToken = default
  )
  {
    if (id.Length != SKSETRBIDLengthOfId)
      throw new ArgumentException($"length of `{nameof(id)}` must be exact {SKSETRBIDLengthOfId}", nameof(id));

    return SendCommandAsync(
      command: SkStackCommandNames.SKSETRBID,
      writeArguments: writer => writer.WriteToken(id.Span),
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }
}
