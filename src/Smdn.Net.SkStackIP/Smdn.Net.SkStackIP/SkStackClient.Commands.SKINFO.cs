// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
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
  /// <remarks>reference: BP35A1コマンドリファレンス 3.2. SKINFO</remarks>
  public ValueTask<SkStackResponse<(
    IPAddress LinkLocalAddress,
    PhysicalAddress MacAddress,
    SkStackChannel Channel,
    int PanId,
    int Addr16
  )>>
  SendSKINFOAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKINFO,
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, EINFO) &&
          SkStackTokenParser.ExpectIPADDR(ref reader, out var linkLocalAddress) &&
          SkStackTokenParser.ExpectADDR64(ref reader, out var macAddress) &&
          SkStackTokenParser.ExpectCHANNEL(ref reader, out var channel) &&
          SkStackTokenParser.ExpectUINT16(ref reader, out var panID) &&
          SkStackTokenParser.ExpectADDR16(ref reader, out var addr16) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return (
            linkLocalAddress,
            macAddress,
            channel,
            (int)panID,
            (int)addr16
          );
        }

        context.SetAsIncomplete();
        return default;
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  private static readonly ReadOnlyMemory<byte> EINFO = SkStack.ToByteSequence(nameof(EINFO));
}
