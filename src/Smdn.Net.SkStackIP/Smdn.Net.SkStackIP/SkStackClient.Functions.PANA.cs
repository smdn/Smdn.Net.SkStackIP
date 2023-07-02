// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE || SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
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

  private async ValueTask SetRouteBCredentialAsync(
    ReadOnlyMemory<char>? rbid,
    string rbidParamName,
    ReadOnlyMemory<char>? password,
    string passwordParamName,
    CancellationToken cancellationToken
  )
  {
    if (rbid is not null) {
      if (rbid.Value.IsEmpty)
        throw new ArgumentException("must be non-empty string", rbidParamName ?? nameof(rbid));

      _ = await SendSKSETRBIDAsync(
        id: rbid.Value,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }

    if (password is not null) {
      if (password.Value.IsEmpty)
        throw new ArgumentException("must be non-empty string", passwordParamName ?? nameof(password));

      _ = await SendSKSETPWDAsync(
        password: password.Value,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
  }

  // <returns><see langword="true"/> if terminated successfully, otherwise <see langword="false"/> (timed out).</returns>
  public async ValueTask<bool> TerminatePanaSessionAsync(
    CancellationToken cancellationToken = default
  )
  {
    var (_, isCompletedSuccessfully) = await SendSKTERMAsync(
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return isCompletedSuccessfully;
  }
}
