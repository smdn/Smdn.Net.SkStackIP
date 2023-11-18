// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>
  ///   <para>Sends a command <c>SKDSLEEP</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.27. SKDSLEEP' for detailed specifications.</para>
  /// </remarks>
  /// <seealso cref="Slept"/>
  /// <seealso cref="WokeUp"/>
  public ValueTask<SkStackResponse> SendSKDSLEEPAsync(
    bool waitUntilWakeUp = false,
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKDSLEEP,
      writeArguments: null,
      commandEventHandler: new SKDSLEEPEventHandler(this, waitUntilWakeUp),
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  private class SKDSLEEPEventHandler : SkStackEventHandlerBase {
    private readonly SkStackClient owner;
    private readonly bool waitUntilWakeUp;

    public SKDSLEEPEventHandler(SkStackClient owner, bool waitUntilWakeUp)
    {
      this.owner = owner;
      this.waitUntilWakeUp = waitUntilWakeUp;
    }

    public override bool DoContinueHandlingEvents(SkStackResponseStatus status)
    {
      var sleepStartedSuccessfully = status == SkStackResponseStatus.Ok;

      if (sleepStartedSuccessfully)
        owner.RaiseEventSlept();

      if (waitUntilWakeUp)
        return sleepStartedSuccessfully; // do continue handling events if sleep started (wait until `EVENT CO`)
      else
        return false; // do not continue handling events
    }

    public override bool TryProcessEvent(SkStackEvent ev)
      => ev.Number == SkStackEventNumber.WakeupSignalReceived;
  }
}
