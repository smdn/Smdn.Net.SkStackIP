// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    /// <remarks>reference: BP35A1コマンドリファレンス 3.6. SKTERM</remarks>
    public async ValueTask<(
      SkStackResponse Response,
      bool IsCompletedSuccessfully
    )> SendSKTERMAsync(
      CancellationToken cancellationToken = default
    )
    {
      var resp = await SendCommandAsync(
        command: SkStackCommandNames.SKTERM,
        arguments: Array.Empty<ReadOnlyMemory<byte>>(),
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      ).ConfigureAwait(false);

      var finalStatusEvent = await ReceiveEventAsync(
        parseEvent: ParseSKTERMEvent,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      RaiseEventPanaSessionTerminated(finalStatusEvent);

      var isCompletedSuccessfully = finalStatusEvent.Number == SkStackEventNumber.PanaSessionTerminationCompleted;

      return (resp, isCompletedSuccessfully);
    }

    private static SkStackEvent ParseSKTERMEvent(
      ISkStackSequenceParserContext context
    )
    {
      static bool IsPanaSessionTerminationEvent(SkStackEventNumber eventNumber)
        => eventNumber switch {
          SkStackEventNumber.PanaSessionTerminationCompleted => true,
          SkStackEventNumber.PanaSessionTerminationTimedOut => true,
          _ => false,
        };


      if (SkStackEventParser.TryExpectEVENT(context, IsPanaSessionTerminationEvent, out var ev27or28)) {
        context.Logger?.LogInfoPanaEventReceived(ev27or28);
        context.Complete();
        return ev27or28;
      }

      context.SetAsIncomplete();
      return default;
    }
  }
}