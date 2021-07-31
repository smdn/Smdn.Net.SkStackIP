// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    /// <remarks>reference: BP35A1コマンドリファレンス 3.27. SKDSLEEP</remarks>
    public ValueTask<SkStackResponse> SendSKDSLEEPAsync(
      bool waitUntilWakeUp = false,
      CancellationToken cancellationToken = default
    )
      => SendCommandAsync(
        command: SkStackCommandNames.SKDSLEEP,
        arguments: Array.Empty<ReadOnlyMemory<byte>>(),
        commandEventHandler: new SKDSLEEPEventHandler(this, waitUntilWakeUp),
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      );

    private class SKDSLEEPEventHandler : ISkStackEventHandler {
      private readonly SkStackClient owner;
      private readonly bool waitUntilWakeUp;

      public SKDSLEEPEventHandler(SkStackClient owner, bool waitUntilWakeUp)
      {
        this.owner = owner;
        this.waitUntilWakeUp = waitUntilWakeUp;
      }

      bool ISkStackEventHandler.DoContinueHandlingEvents(SkStackResponseStatus status)
      {
        var sleepStartedSuccessfully = status == SkStackResponseStatus.Ok;

        if (sleepStartedSuccessfully)
          owner.RaiseEventSlept();

        if (waitUntilWakeUp)
          return sleepStartedSuccessfully; // do continue handling events if sleep started (wait until `EVENT CO`)
        else
          return false; // do not continue handling events
      }

      bool ISkStackEventHandler.TryProcessEvent(SkStackEventNumber eventNumber, IPAddress senderAddress)
        => eventNumber == SkStackEventNumber.WakeupSignalReceived;

      void ISkStackEventHandler.ProcessSubsequentEvent(ISkStackSequenceParserContext context) { /*do nothing*/ }
    }
  }
}