// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <remarks>reference: BP35A1コマンドリファレンス 3.6. SKTERM</remarks>
  public async ValueTask<(
    SkStackResponse Response,
    bool IsCompletedSuccessfully
  )> SendSKTERMAsync(
    CancellationToken cancellationToken = default
  )
  {
    var eventHandler = new SKTERMEventHandler();

    var resp = await SendCommandAsync(
      command: SkStackCommandNames.SKTERM,
      arguments: Array.Empty<ReadOnlyMemory<byte>>(),
      commandEventHandler: eventHandler,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return (resp, eventHandler.IsCompletedSuccessfully);
  }

  private class SKTERMEventHandler : SkStackEventHandlerBase {
    public bool IsCompletedSuccessfully { get; private set; }

    public override bool TryProcessEvent(SkStackEvent ev)
    {
      switch (ev.Number) {
        case SkStackEventNumber.PanaSessionTerminationCompleted:
          IsCompletedSuccessfully = true;
          return true;

        case SkStackEventNumber.PanaSessionTerminationTimedOut:
          IsCompletedSuccessfully = false;
          return true;

        default:
          return false;
      }
    }
  }
}
