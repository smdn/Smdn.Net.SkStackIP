// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES || SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>Gets the <see cref="IPAddress"/> of current PANA session peer.</summary>
  /// <value><see langword="null"/> if PANA session has been terminated, expired, or not been established.</value>
  public IPAddress? PanaSessionPeerAddress { get; private set; }

  /// <summary>Gets a value indicating whether or not the PANA session is alive.</summary>
  /// <value><see langword="true"/> if PANA session is established and alive, <see langword="false"/> if PANA session has been terminated, expired, or not been established.</value>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNullWhen(true, nameof(PanaSessionPeerAddress))]
#endif
  public bool IsPanaSessionAlive => PanaSessionPeerAddress is not null;

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(PanaSessionPeerAddress))]
#endif
  protected internal void ThrowIfPanaSessionIsNotEstablished()
  {
    if (PanaSessionPeerAddress is null)
      throw new InvalidOperationException("PANA session has expired or has not been established yet.");
  }

  protected internal void ThrowIfPanaSessionAlreadyEstablished()
  {
    if (PanaSessionPeerAddress is not null)
      throw new InvalidOperationException("PANA session has been already established.");
  }

#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [return: NotNullIfNotNull(nameof(rbid))]
#endif
  private static Action<IBufferWriter<byte>>? CreateActionForWritingRBID(
    ReadOnlyMemory<byte>? rbid,
    string rbidParamName
  )
  {
    if (rbid is null)
      return null;

    if (rbid.Value.IsEmpty)
      throw new ArgumentException("must be non-empty string", rbidParamName ?? nameof(rbid));

    return new(writer => writer.Write(rbid.Value.Span));
  }

#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [return: NotNullIfNotNull(nameof(password))]
#endif
  private static Action<IBufferWriter<byte>>? CreateActionForWritingPassword(
    ReadOnlyMemory<byte>? password,
    string passwordParamName
  )
  {
    if (password is null)
      return null;

    if (password.Value.IsEmpty)
      throw new ArgumentException("must be non-empty string", passwordParamName ?? nameof(password));

    return new(writer => writer.Write(password.Value.Span));
  }

  private async ValueTask SetRouteBCredentialAsync(
    Action<IBufferWriter<byte>>? writeRBID,
    Action<IBufferWriter<byte>>? writePassword,
    CancellationToken cancellationToken
  )
  {
    if (writeRBID is not null) {
      _ = await SendSKSETRBIDAsync(
        writeRBID: writeRBID,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }

    if (writePassword is not null) {
      _ = await SendSKSETPWDAsync(
        writePassword: writePassword,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
  }

  /// <summary>
  /// Terminates the currently established PANA session by sending <c>SKTERM</c> command.
  /// </summary>
  /// <exception cref="InvalidOperationException">PANA session has already expired or has not been established yet.</exception>
  /// <returns><see langword="true"/> if terminated successfully, otherwise <see langword="false"/> (timed out).</returns>
  public ValueTask<bool> TerminatePanaSessionAsync(
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfDisposed();
    ThrowIfPanaSessionIsNotEstablished();

    return TerminatePanaSessionAsyncCore();

    async ValueTask<bool> TerminatePanaSessionAsyncCore()
    {
      var (_, isCompletedSuccessfully) = await SendSKTERMAsync(
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return isCompletedSuccessfully;
    }
  }
}
