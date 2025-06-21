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
  /// <seealso cref="PanaSessionState"/>
  public IPAddress? PanaSessionPeerAddress { get; private set; }

  /// <summary>
  /// Gets the value of the <see cref="SkStackEventNumber"/> that indicating current status of the PANA session.
  /// </summary>
  /// <value>
  /// The value of this property will be one of the following.
  /// <list type="bullet">
  ///   <item>
  ///     <term><see cref="SkStackEventNumber.Undefined"/></term>
  ///     <description>
  ///       Initial state of the instance, where neither PANA session establishment nor termination has been attempted.
  ///       <see cref="PanaSessionPeerAddress"/> will be <see langword="null"/>.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <term><see cref="SkStackEventNumber.PanaSessionEstablishmentError"/></term>
  ///     <description>
  ///       State where the PANA session establishment was attempted but failed.
  ///       <see cref="PanaSessionPeerAddress"/> becomes <see langword="null"/>.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <term><see cref="SkStackEventNumber.PanaSessionEstablishmentCompleted"/></term>
  ///     <description>
  ///       State where the PANA session establishment was successful.
  ///       <see cref="PanaSessionPeerAddress"/> becomes a <see cref="IPAddress"/> representing the peer's address.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <term><see cref="SkStackEventNumber.PanaSessionTerminationRequestReceived"/></term>
  ///     <description>
  ///       State where the PANA session termination was requested by the peer.
  ///       <see cref="PanaSessionPeerAddress"/> becomes <see langword="null"/>.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <term><see cref="SkStackEventNumber.PanaSessionTerminationCompleted"/></term>
  ///     <description>
  ///       State where the PANA session was terminated.
  ///       <see cref="PanaSessionPeerAddress"/> becomes <see langword="null"/>.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <term><see cref="SkStackEventNumber.PanaSessionTerminationTimedOut"/></term>
  ///     <description>
  ///       State where the PANA session was terminated, but the response to the termination request timed out.
  ///       <see cref="PanaSessionPeerAddress"/> becomes <see langword="null"/>.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <term><see cref="SkStackEventNumber.PanaSessionExpired"/></term>
  ///     <description>
  ///       State where the PANA session has ended due to expiration.
  ///       <see cref="PanaSessionPeerAddress"/> becomes <see langword="null"/>.
  ///     </description>
  ///   </item>
  /// </list>
  /// </value>
  /// <seealso cref="PanaSessionPeerAddress"/>
  public SkStackEventNumber PanaSessionState { get; private set; }

  /// <summary>Gets a value indicating whether or not the PANA session is alive.</summary>
  /// <value><see langword="true"/> if PANA session is established and alive, <see langword="false"/> if PANA session has been terminated, expired, or not been established.</value>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNullWhen(true, nameof(PanaSessionPeerAddress))]
#endif
  public bool IsPanaSessionAlive => PanaSessionPeerAddress is not null;

  /// <summary>
  /// Throws <see cref="SkStackPanaSessionNotEstablishedException"/> if <see cref="PanaSessionState"/>
  /// is not <see cref="SkStackEventNumber.PanaSessionEstablishmentCompleted"/>.
  /// </summary>
  /// <exception cref="SkStackPanaSessionNotEstablishedException">
  /// <see cref="PanaSessionState"/> is not <see cref="SkStackEventNumber.PanaSessionEstablishmentCompleted"/>.
  /// </exception>
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(PanaSessionPeerAddress))]
#endif
  protected internal void ThrowIfPanaSessionIsNotEstablished()
  {
    if (PanaSessionState != SkStackEventNumber.PanaSessionEstablishmentCompleted)
      throw new SkStackPanaSessionNotEstablishedException();

    // inconsistency in internal state
    if (PanaSessionPeerAddress is null)
      throw new InvalidOperationException($"{nameof(PanaSessionPeerAddress)} is not set.");
  }

  /// <summary>
  /// Throws <see cref="SkStackPanaSessionStateException"/> if <see cref="PanaSessionState"/>
  /// is <see cref="SkStackEventNumber.PanaSessionEstablishmentCompleted"/>.
  /// </summary>
  /// <exception cref="SkStackPanaSessionNotEstablishedException">
  /// <see cref="PanaSessionState"/> is <see cref="SkStackEventNumber.PanaSessionEstablishmentCompleted"/>.
  /// </exception>
  public void ThrowIfPanaSessionAlreadyEstablished()
  {
    if (PanaSessionState == SkStackEventNumber.PanaSessionEstablishmentCompleted)
      throw new SkStackPanaSessionStateException("The PANA session has already been established.");

    // inconsistency in internal state
    if (PanaSessionPeerAddress is not null)
      throw new InvalidOperationException($"{nameof(PanaSessionPeerAddress)} is set.");
  }

  /// <summary>
  /// Throws <see cref="SkStackPanaSessionStateException"/> or its derived class
  /// when the <see cref="PanaSessionState"/> indicates that the PANA session
  /// is not alive.
  /// </summary>
  /// <remarks>
  /// This method throws nothing when the <see cref="PanaSessionState"/>
  /// is <see cref="SkStackEventNumber.PanaSessionEstablishmentCompleted"/>.
  /// </remarks>
  /// <exception cref="SkStackPanaSessionNotEstablishedException">
  ///   <see cref="PanaSessionState"/> is
  ///   <see cref="SkStackEventNumber.Undefined"/> or
  ///   <see cref="SkStackEventNumber.PanaSessionEstablishmentError"/>.
  /// </exception>
  /// <exception cref="SkStackPanaSessionTerminatedException">
  ///   <see cref="PanaSessionState"/> is
  ///   <see cref="SkStackEventNumber.PanaSessionTerminationRequestReceived"/>, or
  ///   <see cref="SkStackEventNumber.PanaSessionTerminationCompleted"/>, or
  ///   <see cref="SkStackEventNumber.PanaSessionTerminationTimedOut"/>.
  /// </exception>
  /// <exception cref="SkStackPanaSessionExpiredException">
  ///   <see cref="PanaSessionState"/> is
  ///   <see cref="SkStackEventNumber.PanaSessionExpired"/>.
  /// </exception>
  /// <exception cref="SkStackPanaSessionStateException">
  /// Cannot determine the current status of the PANA session.
  /// </exception>
  public void ThrowIfPanaSessionNotAlive()
  {
    _ = PanaSessionState switch {
      // established
      SkStackEventNumber.PanaSessionEstablishmentCompleted => default(int), // throws nothing

      // not established yet
      SkStackEventNumber.Undefined
        => throw new SkStackPanaSessionNotEstablishedException("The PANA session has not yet been established."),

      // establishment failed
      SkStackEventNumber.PanaSessionEstablishmentError
        => throw new SkStackPanaSessionNotEstablishedException("Establishing a PANA session has failed."),

      // terminated
      SkStackEventNumber.PanaSessionTerminationRequestReceived or
      SkStackEventNumber.PanaSessionTerminationCompleted or
      SkStackEventNumber.PanaSessionTerminationTimedOut
        => throw new SkStackPanaSessionTerminatedException("The PANA session has terminated."),

      // expired
      SkStackEventNumber.PanaSessionExpired
        => throw new SkStackPanaSessionExpiredException("The PANA session has been expired."),

      // unknown state
      _ => throw new SkStackPanaSessionStateException("Cannot determine the current status of the PANA session."),
    };
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
  /// <exception cref="SkStackPanaSessionNotEstablishedException">PANA session has already expired or has not been established yet.</exception>
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
