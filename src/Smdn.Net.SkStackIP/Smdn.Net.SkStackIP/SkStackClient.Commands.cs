// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1506 // TODO: refactor

using System;
using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>sets the value of register <paramref name="register"/> to <paramref name="value"/></value>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.1. SKSREG</remarks>
  public ValueTask<SkStackResponse> SendSKSREGAsync<TValue>(
    SkStackRegister.RegisterEntry<TValue> register,
    TValue value,
    CancellationToken cancellationToken = default
  )
  {
    if (register is null)
      throw new ArgumentNullException(nameof(register));
    if (!register.IsWritable)
      throw new InvalidOperationException($"register {register.Name} is not writable");

    register.ThrowIfValueIsNotInRange(value, nameof(value));

    return SendCommandAsync(
      command: SkStackCommandNames.SKSREG,
      arguments: SkStackCommandArgs.CreateEnumerable(register.SREG, register.CreateSKSREGArgument(value)),
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }

  /// <summary>gets the value of register <paramref name="register"/></value>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.1. SKSREG</remarks>
  public ValueTask<SkStackResponse<TValue>> SendSKSREGAsync<TValue>(
    SkStackRegister.RegisterEntry<TValue> register,
    CancellationToken cancellationToken = default
  )
  {
    if (register is null)
      throw new ArgumentNullException(nameof(register));
    if (!register.IsReadable)
      throw new InvalidOperationException($"register {register.Name} is not readable");

    return SendCommandAsync(
      command: SkStackCommandNames.SKSREG,
      arguments: SkStackCommandArgs.CreateEnumerable(register.SREG),
      parseResponsePayload: register.ParseESREG,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.8. SKPING</remarks>

  /// <remarks>reference: BP35A1コマンドリファレンス 3.16. SKSETPWD</remarks>
  public ValueTask<SkStackResponse> SendSKSETPWDAsync(
    string password,
    CancellationToken cancellationToken = default
  )
  {
    if (password is null)
      throw new ArgumentNullException(nameof(password));

    var length = SkStack.DefaultEncoding.GetByteCount(password);

    if (length is not (>= SKSETPWDMinLength and <= SKSETPWDMaxLength))
      throw new ArgumentException($"length of `{nameof(password)}` must be in range of {SKSETPWDMinLength}~{SKSETPWDMaxLength}", nameof(password));

    return Core();

    async ValueTask<SkStackResponse> Core()
    {
      byte[] PWD = null;

      try {
        PWD = ArrayPool<byte>.Shared.Rent(length);

        var lengthOfPWD = SkStack.DefaultEncoding.GetBytes(password.AsSpan(), PWD.AsSpan());

        return await SendSKSETPWDAsync(
          password: PWD.AsMemory(0, lengthOfPWD),
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (PWD is not null)
          ArrayPool<byte>.Shared.Return(PWD, clearArray: true);
      }
    }
  }

  private const int SKSETPWDMinLength = 1;
  private const int SKSETPWDMaxLength = 32;

  /// <remarks>reference: BP35A1コマンドリファレンス 3.16. SKSETPWD</remarks>
  public ValueTask<SkStackResponse> SendSKSETPWDAsync(
    ReadOnlyMemory<byte> password,
    CancellationToken cancellationToken = default
  )
  {
    if (password.Length is not (>= SKSETPWDMinLength and <= SKSETPWDMaxLength))
      throw new ArgumentException($"length of `{nameof(password)}` must be in range of {SKSETPWDMinLength}~{SKSETPWDMaxLength}", nameof(password));

    return SKSETPWD();

    async ValueTask<SkStackResponse> SKSETPWD()
    {
      byte[] LEN = default;

      try {
        LEN = ArrayPool<byte>.Shared.Rent(2);

        SkStackCommandArgs.TryConvertToUINT8(LEN, (byte)password.Length, out var lengthOfLEN, zeroPadding: false);

        return await SendCommandAsync(
          command: SkStackCommandNames.SKSETPWD,
          arguments: SkStackCommandArgs.CreateEnumerable(LEN.AsMemory(0, lengthOfLEN), password),
          throwIfErrorStatus: true,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (LEN is not null)
          ArrayPool<byte>.Shared.Return(LEN);
      }
    }
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.17. SKSETRBID</remarks>
  public ValueTask<SkStackResponse> SendSKSETRBIDAsync(
    string routeBID,
    CancellationToken cancellationToken = default
  )
  {
    if (routeBID is null)
      throw new ArgumentNullException(nameof(routeBID));

    var length = SkStack.DefaultEncoding.GetByteCount(routeBID);

    if (length != SKSETRBIDLengthOfID)
      throw new ArgumentException($"length of `{nameof(routeBID)}` must be exact {SKSETRBIDLengthOfID}", nameof(routeBID));

    return Core();

    async ValueTask<SkStackResponse> Core()
    {
      byte[] ID = null;

      try {
        ID = ArrayPool<byte>.Shared.Rent(length);

        var lengthOfID = SkStack.DefaultEncoding.GetBytes(routeBID.AsSpan(), ID.AsSpan());

        return await SendSKSETRBIDAsync(
          routeBID: ID.AsMemory(0, lengthOfID),
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (ID is not null)
          ArrayPool<byte>.Shared.Return(ID, clearArray: true);
      }
    }
  }

  private const int SKSETRBIDLengthOfID = 32;

  /// <remarks>reference: BP35A1コマンドリファレンス 3.17. SKSETRBID</remarks>
  public ValueTask<SkStackResponse> SendSKSETRBIDAsync(
    ReadOnlyMemory<byte> routeBID,
    CancellationToken cancellationToken = default
  )
  {
    if (routeBID.Length != SKSETRBIDLengthOfID)
      throw new ArgumentException($"length of `{nameof(routeBID)}` must be exact {SKSETRBIDLengthOfID}", nameof(routeBID));

    return SendCommandAsync(
      command: SkStackCommandNames.SKSETRBID,
      arguments: SkStackCommandArgs.CreateEnumerable(routeBID),
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.20. SKSAVE</remarks>
  public ValueTask<SkStackResponse> SendSKSAVEAsync(
    CancellationToken cancellationToken = default
  )
    => SendFlashMemoryCommand(
      command: SkStackCommandNames.SKSAVE,
      messageForFlashMemoryIOException: "Failed to save the register values to the flash memory.",
      cancellationToken: cancellationToken
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.21. SKLOAD</remarks>
  public ValueTask<SkStackResponse> SendSKLOADAsync(
    CancellationToken cancellationToken = default
  )
    => SendFlashMemoryCommand(
      command: SkStackCommandNames.SKLOAD,
      messageForFlashMemoryIOException: "Failed to load the register values from the flash memory or the register values have not been saved in the flash memory.",
      cancellationToken: cancellationToken
    );

  private async ValueTask<SkStackResponse> SendFlashMemoryCommand(
    ReadOnlyMemory<byte> command,
    string messageForFlashMemoryIOException,
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendCommandAsync(
      command: command,
      arguments: Array.Empty<ReadOnlyMemory<byte>>(),
      throwIfErrorStatus: false,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    resp.ThrowIfErrorStatus(
      (r, code, text) => code == SkStackErrorCode.ER10
        ? new SkStackFlashMemoryIOException(r, code, text.Span, messageForFlashMemoryIOException)
        : null
    );

    return resp;
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.22. SKERASE</remarks>
  public ValueTask<SkStackResponse> SendSKERASEAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKERASE,
      arguments: Array.Empty<ReadOnlyMemory<byte>>(),
      throwIfErrorStatus: false,
      cancellationToken: cancellationToken
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.23. SKVER</remarks>
  public ValueTask<SkStackResponse<Version>> SendSKVERAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKVER,
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, EVER) &&
          SkStackTokenParser.ExpectCharArray(ref reader, out string version) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return Version.Parse(version);
        }

        context.SetAsIncomplete();
        return default;
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  private static readonly ReadOnlyMemory<byte> EVER = SkStack.ToByteSequence(nameof(EVER));

  /// <remarks>reference: BP35A1コマンドリファレンス 3.24. SKAPPVER</remarks>
  public ValueTask<SkStackResponse<string>> SendSKAPPVERAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKAPPVER,
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, EAPPVER) &&
          SkStackTokenParser.ExpectCharArray(ref reader, out string appver) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return appver;
        }

        context.SetAsIncomplete();
        return default;
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  private static readonly ReadOnlyMemory<byte> EAPPVER = SkStack.ToByteSequence(nameof(EAPPVER));

  /// <remarks>reference: BP35A1コマンドリファレンス 3.25. SKRESET</remarks>
  public ValueTask<SkStackResponse> SendSKRESETAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKRESET,
      arguments: Array.Empty<ReadOnlyMemory<byte>>(),
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.29. SKLL64</remarks>
  public ValueTask<SkStackResponse<IPAddress>> SendSKLL64Async(
    PhysicalAddress macAddress,
    CancellationToken cancellationToken = default
  )
  {
    if (macAddress is null)
      throw new ArgumentNullException(nameof(macAddress));

    return SKLL64();

    async ValueTask<SkStackResponse<IPAddress>> SKLL64()
    {
      byte[] ADDR64 = null;

      try {
        ADDR64 = ArrayPool<byte>.Shared.Rent(SkStackCommandArgs.LengthOfADDR64);

        SkStackCommandArgs.TryConvertToADDR64(ADDR64, macAddress, out var lengthOfADDR64);

        return await SendCommandAsync(
          command: SkStackCommandNames.SKLL64,
          arguments: SkStackCommandArgs.CreateEnumerable(ADDR64.AsMemory(0, lengthOfADDR64)),
          parseResponsePayload: static context => {
            var reader = context.CreateReader();

            if (
              SkStackTokenParser.ExpectIPADDR(ref reader, out var linkLocalAddress) &&
              SkStackTokenParser.ExpectEndOfLine(ref reader)
            ) {
              context.Complete(reader);
              return linkLocalAddress;
            }

            context.SetAsIncomplete();
            return default;
          },
          syntax: SkStackProtocolSyntax.SKLL64, // SKLL64 does not define its status
          throwIfErrorStatus: true,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (ADDR64 is not null)
          ArrayPool<byte>.Shared.Return(ADDR64);
      }
    }
  }
}
