// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    /// <summary>BP35A1コマンドリファレンス 3.2. SKINFO</summary>
    public async Task<(
      IPAddress linkLocalAddress,
      PhysicalAddress macAddress,
      SkStackChannel channel,
      int panId,
      int addr16
    )>
    SendSKINFOAsync(
      CancellationToken cancellationToken = default
    )
    {
      var response = await SendCommandAsync(
        SkStackCommandCodes.SKINFO,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      ).ConfigureAwait(false);

      var line = response.GetFirstLineOrThrow();

      return line.ConvertTokens((l, tokens) => {
        var tks = tokens.Span;

        if (tks.Length < 6)
          throw SkStackUnexpectedResponseException.CreateInvalidFormat(l.Span);

        // TODO: tks[0] == EINFO

        return (
          linkLocalAddress: IPAddress.Parse(System.Text.Encoding.ASCII.GetString(tks[1].Span)),
          macAddress: PhysicalAddress.Parse(System.Text.Encoding.ASCII.GetString(tks[2].Span)),
          channel: SkStackChannel.FindByChannelNumber(tks[3].ToUINT8()),
          panId: tks[4].ToUINT16(),
          addr16: tks[5].ToUINT16()
        );
      });
    }

    /// <summary>BP35A1コマンドリファレンス 3.4. SKJOIN</summary>

    /// <summary>BP35A1コマンドリファレンス 3.6. SKTERM</summary>
    public Task SendSKTERMAsync(
      CancellationToken cancellationToken = default
    )
      => SendCommandAsync(
        SkStackCommandCodes.SKTERM,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      );

    /// <summary>BP35A1コマンドリファレンス 3.7. SKSENDTO</summary>
    /// <summary>BP35A1コマンドリファレンス 3.8. SKPING</summary>
    /// <summary>BP35A1コマンドリファレンス 3.9. SKSCAN</summary>

    /// <summary>BP35A1コマンドリファレンス 3.23. SKVER</summary>
    public async Task<Version> SendSKVERAsync(
      CancellationToken cancellationToken = default
    )
    {
      var response = await SendCommandAsync(
        SkStackCommandCodes.SKVER,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      ).ConfigureAwait(false);

      var line = response.GetFirstLineOrThrow();

      return line.ConvertTokens((l, tokens) => {
        var tks = tokens.Span;

        if (tks.Length < 2)
          throw SkStackUnexpectedResponseException.CreateInvalidFormat(l.Span);

        // TODO: tks[0] == EVER
        return Version.Parse(System.Text.Encoding.ASCII.GetString(tks[1].Span));
      });
    }

    /// <summary>BP35A1コマンドリファレンス 3.24. SKAPPVER</summary>
    public async Task<string> SendSKAPPVERAsync(
      CancellationToken cancellationToken = default
    )
    {
      var response = await SendCommandAsync(
        SkStackCommandCodes.SKAPPVER,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      ).ConfigureAwait(false);

      var line = response.GetFirstLineOrThrow();

      return line.ConvertTokens((l, tokens) => {
        var tks = tokens.Span;

        if (tks.Length < 2)
          throw SkStackUnexpectedResponseException.CreateInvalidFormat(l.Span);

        // TODO: tks[0] == EAPPVER
        return System.Text.Encoding.ASCII.GetString(tks[1].Span);
      });
    }

    /// <summary>BP35A1コマンドリファレンス 3.25. SKRESET</summary>
    public Task SendSKRESETAsync(
      CancellationToken cancellationToken = default
    )
      => SendCommandAsync(
        SkStackCommandCodes.SKRESET,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      );
  }
}