// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1506 // TODO: refactor

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>
  ///   <para>Sends a command <c>SKSREG</c>.</para>
  ///   <para>Sets the value of the register specified by <paramref name="register"/> to <paramref name="value"/>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.1. SKSREG' for detailed specifications.</para>
  /// </remarks>
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
      writeArguments: writer => {
        writer.WriteToken(register.SREG.Span);
        register.WriteValueTo(writer, value);
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }

  /// <summary>
  ///   <para>Sends a command <c>SKSREG</c>.</para>
  ///   <para>Gets the value of the register specified by <paramref name="register"/>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.1. SKSREG' for detailed specifications.</para>
  /// </remarks>
  /// <seealso cref="SkStackRegister"/>
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
      writeArguments: writer => writer.WriteToken(register.SREG.Span),
      parseResponsePayload: register.ParseESREG,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }

#if false
  /// <summary>
  ///   <para>Sends a command <c>SKPING</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.8. SKPING' for detailed specifications.</para>
  /// </remarks>
#endif

  /// <summary>
  ///   <para>Sends a command <c>SKSAVE</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.20. SKSAVE' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKSAVEAsync(
    CancellationToken cancellationToken = default
  )
    => SendFlashMemoryCommand(
      command: SkStackCommandNames.SKSAVE,
      messageForFlashMemoryIOException: "Failed to save the register values to the flash memory.",
      cancellationToken: cancellationToken
    );

  /// <summary>
  ///   <para>Sends a command <c>SKLOAD</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.21. SKLOAD' for detailed specifications.</para>
  /// </remarks>
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

  /// <summary>
  ///   <para>Sends a command <c>SKERASE</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.22. SKERASE' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKERASEAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKERASE,
      throwIfErrorStatus: false,
      cancellationToken: cancellationToken
    );

  /// <summary>
  ///   <para>Sends a command <c>SKVER</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.23. SKVER' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse<Version>> SendSKVERAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKVER,
      writeArguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "EVER"u8) &&
          SkStackTokenParser.ExpectCharArray(ref reader, out string? version) &&
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

  /// <summary>
  ///   <para>Sends a command <c>SKAPPVER</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.24. SKAPPVER' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse<string>> SendSKAPPVERAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKAPPVER,
      writeArguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, "EAPPVER"u8) &&
          SkStackTokenParser.ExpectCharArray(ref reader, out string? appver) &&
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

  /// <summary>
  ///   <para>Sends a command <c>SKRESET</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.25. SKRESET' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKRESETAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKRESET,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  /// <summary>
  ///   <para>Sends a command <c>SKLL64</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.29. SKLL64' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse<IPAddress>> SendSKLL64Async(
    PhysicalAddress macAddress,
    CancellationToken cancellationToken = default
  )
  {
    if (macAddress is null)
      throw new ArgumentNullException(nameof(macAddress));

    return SendCommandAsync(
      command: SkStackCommandNames.SKLL64,
      writeArguments: writer => writer.WriteTokenADDR64(macAddress),
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
    );
  }
}
