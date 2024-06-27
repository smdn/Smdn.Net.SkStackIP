// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;
using System.Net;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that represents an error on the establishment of a PANA session.
/// </summary>
/// <seealso cref="SkStackClient.SendSKJOINAsync(IPAddress, System.Threading.CancellationToken)"/>
public class SkStackPanaSessionEstablishmentException : SkStackPanaSessionException {
  /// <summary>Gets the <see cref="IPAddress"/> of the PANA Authentication Agent(PAA) that attempted PANA authentication.</summary>
  public IPAddress? PaaAddress { get; }

  /// <summary>Gets the channel number used when attempting to establish a PANA session.</summary>
  /// <value>If this exception occurs as a result of the <see cref="SkStackClient.AuthenticateAsPanaClientAsync(ReadOnlyMemory{byte}, ReadOnlyMemory{byte}, IPAddress, SkStackChannel, int, System.Threading.CancellationToken)"/> method, the non-<see langword="null"/> value will be set to this property.　Otherwise, it will be <see langword="null"/>.</value>
  public SkStackChannel? Channel { get; }

  /// <summary>Gets the PAN ID used when attempting to establish a PANA session.</summary>
  /// <value>If this exception occurs as a result of the <see cref="SkStackClient.AuthenticateAsPanaClientAsync(ReadOnlyMemory{byte}, ReadOnlyMemory{byte}, IPAddress, SkStackChannel, int, System.Threading.CancellationToken)"/> method, the non-<see langword="null"/> value will be set to this property.　Otherwise, it will be <see langword="null"/>.</value>
  public int? PanId { get; }

  internal SkStackPanaSessionEstablishmentException(
    string? message,
    IPAddress address,
    SkStackEventNumber eventNumber,
    Exception? innerException = null
  )
    : base(
      message: message ?? $"PANA session establishment failed. (0x{eventNumber:X})",
      address: address,
      eventNumber: eventNumber,
      innerException: innerException
    )
  {
  }

  internal SkStackPanaSessionEstablishmentException(
    string? message,
    IPAddress paaAddress,
    IPAddress address,
    SkStackEventNumber eventNumber,
    Exception? innerException = null
  )
    : base(
      message: message ?? $"PANA session establishment failed. (0x{eventNumber:X}, PAA={paaAddress})",
      address: address,
      eventNumber: eventNumber,
      innerException: innerException
    )
  {
    PaaAddress = paaAddress;
  }

  internal SkStackPanaSessionEstablishmentException(
    string? message,
    IPAddress paaAddress,
    SkStackChannel channel,
    int panId,
    IPAddress address,
    SkStackEventNumber eventNumber,
    Exception? innerException = null
  )
    : base(
      message: message ?? $"PANA session establishment failed. (0x{eventNumber:X}, PAA={paaAddress}, Channel={channel}, PanID=0x{panId:X4})",
      address: address,
      eventNumber: eventNumber,
      innerException: innerException
    )
  {
    PaaAddress = paaAddress;
    Channel = channel;
    PanId = panId;
  }
}
