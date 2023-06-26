// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  public ValueTask SaveFlashMemoryAsync(
    SkStackFlashMemoryWriteRestriction restriction,
    CancellationToken cancellationToken = default
  )
  {
    if (restriction is null)
      throw new ArgumentNullException(nameof(restriction));
    if (restriction.IsRestricted())
      throw new InvalidOperationException($"Writing to flash memory is restricted by the {nameof(SkStackFlashMemoryWriteRestriction)}.");

    cancellationToken.ThrowIfCancellationRequested();

    return SaveFlashMemoryAsyncCore();

    async ValueTask SaveFlashMemoryAsyncCore()
      => await SendSKSAVEAsync(cancellationToken).ConfigureAwait(false);
  }

  public async ValueTask LoadFlashMemoryAsync(
    CancellationToken cancellationToken = default
  )
    => await SendSKLOADAsync(cancellationToken).ConfigureAwait(false);

  public ValueTask EnableFlashMemoryAutoLoadAsync(
    CancellationToken cancellationToken = default
  )
    => SetFlashMemoryAutoLoadAsync(
      trueIfEnable: true,
      cancellationToken: cancellationToken
    );

  public ValueTask DisableFlashMemoryAutoLoadAsync(
    CancellationToken cancellationToken = default
  )
    => SetFlashMemoryAutoLoadAsync(
      trueIfEnable: false,
      cancellationToken: cancellationToken
    );

  protected async ValueTask SetFlashMemoryAutoLoadAsync(
    bool trueIfEnable,
    CancellationToken cancellationToken = default
  )
    => await SendSKSREGAsync(
      register: SkStackRegister.EnableAutoLoad,
      value: trueIfEnable,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);
}
