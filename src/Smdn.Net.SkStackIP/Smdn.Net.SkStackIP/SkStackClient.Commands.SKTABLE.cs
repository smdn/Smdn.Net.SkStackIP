// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using System.Linq;
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
  ///   <para>Sends a command <c>SKTABLE 1</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.26. SKTABLE' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse<IReadOnlyList<IPAddress>>> SendSKTABLEAvailableAddressListAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKTABLE,
      arguments: SkStackCommandArgs.CreateEnumerable(SkStackCommandArgs.GetHex(0x1)),
      parseResponsePayload: SkStackEventParser.ExpectEADDR,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  /// <summary>
  ///   <para>Sends a command <c>SKTABLE 2</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.26. SKTABLE' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse<IReadOnlyDictionary<IPAddress, PhysicalAddress>>> SendSKTABLENeighborCacheListAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKTABLE,
      arguments: SkStackCommandArgs.CreateEnumerable(SkStackCommandArgs.GetHex(0x2)),
      parseResponsePayload: SkStackEventParser.ExpectENEIGHBOR,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );

  /// <summary>
  ///   <para>Sends a command <c>SKTABLE E</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.26. SKTABLE' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse<IReadOnlyList<SkStackUdpPort>>> SendSKTABLEListeningPortListAsync(
    CancellationToken cancellationToken = default
  )
  {
    return SendSKTABLEListeningPortListAsyncCore();

    async ValueTask<SkStackResponse<IReadOnlyList<SkStackUdpPort>>> SendSKTABLEListeningPortListAsyncCore()
    {
      var resp = await SendCommandAsync(
        command: SkStackCommandNames.SKTABLE,
        arguments: SkStackCommandArgs.CreateEnumerable(SkStackCommandArgs.GetHex(0xE)),
        parseResponsePayload: SkStackEventParser.ExpectEPORT,
        throwIfErrorStatus: true,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      var portList = resp.Payload!;

      // store or update the port handle for ECHONET Lite each time the EPORT is received
      udpPortHandleForEchonetLite = portList.FirstOrDefault(static p => p.Port == SkStackKnownPortNumbers.EchonetLite).Handle;

      return resp;
    }
  }
}
