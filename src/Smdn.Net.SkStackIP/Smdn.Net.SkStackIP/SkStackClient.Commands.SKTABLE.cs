// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>`SKTABLE 1`</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.26. SKTABLE</remarks>
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

  /// <summary>`SKTABLE 2`</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.26. SKTABLE</remarks>
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

  /// <summary>`SKTABLE E`</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.26. SKTABLE</remarks>
  public ValueTask<SkStackResponse<IReadOnlyList<SkStackUdpPort>>> SendSKTABLEListeningPortListAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKTABLE,
      arguments: SkStackCommandArgs.CreateEnumerable(SkStackCommandArgs.GetHex(0xE)),
      parseResponsePayload: SkStackEventParser.ExpectEPORT,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
}
