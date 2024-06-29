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
  private const int SKSETPWDMinLength = 1;
  private const int SKSETPWDMaxLength = 32;

  /// <summary>
  ///   <para>Sends a command <c>SKSETPWD</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.16. SKSETPWD' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKSETPWDAsync(
    ReadOnlyMemory<char> password,
    CancellationToken cancellationToken = default
  )
  {
    if (password.Length is not (>= SKSETPWDMinLength and <= SKSETPWDMaxLength))
      throw new ArgumentException($"length of `{nameof(password)}` must be in range of {SKSETPWDMinLength}~{SKSETPWDMaxLength}", nameof(password));
#if SYSTEM_TEXT_ASCII
    if (!Ascii.IsValid(password.Span))
      throw new ArgumentException($"`{nameof(password)}` contains invalid characters for ASCII sequence", paramName: nameof(password));
#endif

    return SendCommandAsync(
      command: SkStackCommandNames.SKSETPWD,
      writeArguments: writer => {
        writer.WriteTokenUINT8((byte)password.Length, zeroPadding: false);
        writer.WriteMaskedToken(password.Span);
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }

  /// <summary>
  ///   <para>Sends a command <c>SKSETPWD</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.16. SKSETPWD' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKSETPWDAsync(
    ReadOnlyMemory<byte> password,
    CancellationToken cancellationToken = default
  )
  {
    if (password.Length is not (>= SKSETPWDMinLength and <= SKSETPWDMaxLength))
      throw new ArgumentException($"length of `{nameof(password)}` must be in range of {SKSETPWDMinLength}~{SKSETPWDMaxLength}", nameof(password));

    return SendCommandAsync(
      command: SkStackCommandNames.SKSETPWD,
      writeArguments: writer => {
        writer.WriteTokenUINT8((byte)password.Length, zeroPadding: false);
        writer.WriteMaskedToken(password.Span);
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }
}
