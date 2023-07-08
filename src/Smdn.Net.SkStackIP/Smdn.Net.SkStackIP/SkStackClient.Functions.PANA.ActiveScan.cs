// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  public ValueTask<IReadOnlyList<SkStackPanDescription>> ActiveScanAsync(
    ReadOnlyMemory<byte> rbid,
    ReadOnlyMemory<byte> password,
    SkStackActiveScanOptions? scanOptions = null,
    CancellationToken cancellationToken = default
  )
    => ActiveScanAsyncCore(
      rbid: rbid,
      password: password,
      scanDurationFactorGenerator: (scanOptions ?? SkStackActiveScanOptions.Default).YieldScanDurationFactors(),
      cancellationToken: cancellationToken
    );

  private async ValueTask<IReadOnlyList<SkStackPanDescription>> ActiveScanAsyncCore(
    ReadOnlyMemory<byte>? rbid,
    ReadOnlyMemory<byte>? password,
    IEnumerable<int> scanDurationFactorGenerator,
    CancellationToken cancellationToken = default
  )
  {
    if (scanDurationFactorGenerator is null)
      throw new ArgumentNullException(nameof(scanDurationFactorGenerator));

    if (rbid is not null || password is not null) {
      // If RBID or password is supplied, set them before scanning.
      await SetRouteBCredentialAsync(
        rbid: rbid,
        rbidParamName: nameof(rbid),
        password: password,
        passwordParamName: nameof(password),
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }

    foreach (var durationFactor in scanDurationFactorGenerator) {
      var (_, result) = await SendSKSCANActiveScanPairAsync(
        durationFactor: durationFactor,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      if (0 < result.Count)
        return result;

      // no pairs found, try again with next scan duration factor
    }

    return Array.Empty<SkStackPanDescription>();
  }

  private async ValueTask<SkStackPanDescription> ActiveScanPanaAuthenticationAgentAsync<TArg>(
    SkStackActiveScanOptions baseScanOptions,
    Func<TArg, SkStackPanDescription, CancellationToken, ValueTask<bool>> selectPanaAuthenticationAgentAsync,
    TArg arg,
    CancellationToken cancellationToken
  )
  {
    var activeScanResult = await ActiveScanAsyncCore(
      rbid: null,
      password: null,
      scanDurationFactorGenerator: baseScanOptions.YieldScanDurationFactors(),
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    SkStackPanDescription? ret = null;

    foreach (var pan in activeScanResult) {
      if (await selectPanaAuthenticationAgentAsync(arg, pan, cancellationToken).ConfigureAwait(false)) {
        ret = pan;
        break;
      }
    }

    return ret ?? throw new InvalidOperationException("No appropriate PAA was found or selected in active scan result.");
  }
}
