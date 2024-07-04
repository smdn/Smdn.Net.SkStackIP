// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The type for defining the scan intervals (scan duration factors) and the method that selects discovered PANA Authentication Agents (PAA) in the scan for PAA, by the <c>SKSCAN</c> command.
/// </summary>
/// <seealso cref="SkStackClient.ActiveScanAsync(Action{System.Buffers.IBufferWriter{byte}}, Action{System.Buffers.IBufferWriter{byte}}, Smdn.Net.SkStackIP.SkStackActiveScanOptions?, System.Threading.CancellationToken)" />
/// <seealso cref="SkStackClient.ActiveScanAsync(ReadOnlyMemory{byte}, ReadOnlyMemory{byte}, Smdn.Net.SkStackIP.SkStackActiveScanOptions?, System.Threading.CancellationToken)" />
public abstract class SkStackActiveScanOptions : ICloneable {
  /// <summary>
  /// Gets the <see cref="SkStackActiveScanOptions"/> which selects the PAA which found at first during the scan.
  /// The scan starts with a duration factor <c>3</c>. If not found, continue scanning with the following durations factors: <c>4</c>, <c>5</c>, <c>6</c>, <c>6</c>, <c>6</c>.
  /// If any PAA is not found until the last scan, it will stop the scanning.
  /// </summary>
  public static SkStackActiveScanOptions Default { get; } = new DefaultActiveScanOptions();

  private sealed class DefaultActiveScanOptions : SkStackActiveScanOptions {
    public override SkStackActiveScanOptions Clone() => new DefaultActiveScanOptions();

    internal override bool SelectPanaAuthenticationAgent(SkStackPanDescription desc) => true; // select first one

    internal override IEnumerable<int> YieldScanDurationFactors()
    {
      yield return 3;
      yield return 4;
      yield return 5;
      yield return 6;
      yield return 6;
      yield return 6;
    }
  }

  /// <summary>
  /// Gets the <see cref="SkStackActiveScanOptions"/> which does not select any PAA and does not perform any scanning.
  /// </summary>
  public static SkStackActiveScanOptions Null { get; } = new NullActiveScanOptions();

  private sealed class NullActiveScanOptions : SkStackActiveScanOptions {
    public override SkStackActiveScanOptions Clone() => new NullActiveScanOptions();

    internal override bool SelectPanaAuthenticationAgent(SkStackPanDescription desc) => false; // select nothing

    internal override IEnumerable<int> YieldScanDurationFactors()
      => Enumerable.Empty<int>();
  }

  /// <summary>
  /// Gets the <see cref="SkStackActiveScanOptions"/> selects the PAA which found at first during the scan.
  /// The scan starts with a duration factor <c>5</c> and continues scanning infinitely until it finds any PAA.
  /// </summary>
  public static SkStackActiveScanOptions ScanUntilFind { get; } = new ScanUntilFindActiveScanOptions();

  private sealed class ScanUntilFindActiveScanOptions : SkStackActiveScanOptions {
    public override SkStackActiveScanOptions Clone() => new ScanUntilFindActiveScanOptions();

    internal override bool SelectPanaAuthenticationAgent(SkStackPanDescription desc) => true; // select first one

    internal override IEnumerable<int> YieldScanDurationFactors()
    {
      for (; ; )
        yield return 5;
    }
  }

  /// <summary>
  /// Creates the <see cref="SkStackActiveScanOptions"/> with the custom selection method and duration factors.
  /// </summary>
  /// <param name="scanDurationGenerator">A collection or iterator that defines the scan durations.</param>
  /// <param name="paaSelector">
  /// A callback to select the target PAA from the PAAs found during the scan.
  /// If <see langword="null"/>, selects the PAA which found at first during the scan.
  /// </param>
  public static SkStackActiveScanOptions Create(
    IEnumerable<int> scanDurationGenerator,
    Predicate<SkStackPanDescription>? paaSelector = null
  )
    => new UserDefinedActiveScanOptions(
      paaSelector: paaSelector,
      scanDurationGenerator: scanDurationGenerator
    );

  /// <summary>
  /// Creates the <see cref="SkStackActiveScanOptions"/> with the custom selection method and the delegate for generating duration factor.
  /// </summary>
  /// <param name="scanDurationGeneratorFunc">A delegate to method that iterates the scan durations.</param>
  /// <param name="paaSelector">
  /// A callback to select the target PAA from the PAAs found during the scan.
  /// If <see langword="null"/>, selects the PAA which found at first during the scan.
  /// </param>
  public static SkStackActiveScanOptions Create(
    Func<IEnumerable<int>> scanDurationGeneratorFunc,
    Predicate<SkStackPanDescription>? paaSelector = null
  )
    => new UserDefinedActiveScanOptions(
      paaSelector: paaSelector,
      scanDurationGeneratorFunc: scanDurationGeneratorFunc
    );

  private sealed class UserDefinedActiveScanOptions : SkStackActiveScanOptions {
    private readonly Predicate<SkStackPanDescription>? paaSelector;
    private readonly Func<IEnumerable<int>> scanDurationGeneratorFunc;

    public UserDefinedActiveScanOptions(
      Predicate<SkStackPanDescription>? paaSelector,
      IEnumerable<int> scanDurationGenerator
    )
    {
      if (scanDurationGenerator is null)
        throw new ArgumentNullException(nameof(scanDurationGenerator));

      this.paaSelector = paaSelector;
      this.scanDurationGeneratorFunc = new(() => scanDurationGenerator);
    }

    public UserDefinedActiveScanOptions(
      Predicate<SkStackPanDescription>? paaSelector,
      Func<IEnumerable<int>> scanDurationGeneratorFunc
    )
    {
      this.paaSelector = paaSelector;
      this.scanDurationGeneratorFunc = scanDurationGeneratorFunc ?? throw new ArgumentNullException(nameof(scanDurationGeneratorFunc));
    }

    public override SkStackActiveScanOptions Clone() => new UserDefinedActiveScanOptions(paaSelector, scanDurationGeneratorFunc);
    internal override bool SelectPanaAuthenticationAgent(SkStackPanDescription desc) => paaSelector?.Invoke(desc) ?? true;
    internal override IEnumerable<int> YieldScanDurationFactors() => scanDurationGeneratorFunc();
  }

  /// <summary>
  /// Creates the <see cref="SkStackActiveScanOptions"/> with the custom selection method and duration factors.
  /// </summary>
  /// <param name="scanDurationGenerator">A collection or iterator that defines the scan durations.</param>
  /// <param name="paaMacAddress">
  /// A <see cref="PhysicalAddress"/> of the target PAA. This method selects the first PAA found during the scan that matches this <see cref="PhysicalAddress"/>.
  /// </param>
  public static SkStackActiveScanOptions Create(
    IEnumerable<int> scanDurationGenerator,
    PhysicalAddress paaMacAddress
  )
    => new FindByMacAddressActiveScanOptions(
      paaMacAddress: paaMacAddress,
      scanDurationGenerator: scanDurationGenerator
    );

  /// <summary>
  /// Creates the <see cref="SkStackActiveScanOptions"/> with the custom selection method and the delegate for generating duration factor.
  /// </summary>
  /// <param name="scanDurationGeneratorFunc">A delegate to method that iterates the scan durations.</param>
  /// <param name="paaMacAddress">
  /// A <see cref="PhysicalAddress"/> of the target PAA. This method selects the first PAA found during the scan that matches this <see cref="PhysicalAddress"/>.
  /// </param>
  public static SkStackActiveScanOptions Create(
    Func<IEnumerable<int>> scanDurationGeneratorFunc,
    PhysicalAddress paaMacAddress
  )
    => new FindByMacAddressActiveScanOptions(
      paaMacAddress: paaMacAddress,
      scanDurationGeneratorFunc: scanDurationGeneratorFunc
    );

  private sealed class FindByMacAddressActiveScanOptions : SkStackActiveScanOptions {
    private readonly PhysicalAddress paaMacAddress;
    private readonly Func<IEnumerable<int>> scanDurationGeneratorFunc;

    public FindByMacAddressActiveScanOptions(
      PhysicalAddress paaMacAddress,
      IEnumerable<int> scanDurationGenerator
    )
    {
      if (scanDurationGenerator is null)
        throw new ArgumentNullException(nameof(scanDurationGenerator));

      this.paaMacAddress = paaMacAddress;
      scanDurationGeneratorFunc = new(() => scanDurationGenerator);
    }

    public FindByMacAddressActiveScanOptions(
      PhysicalAddress paaMacAddress,
      Func<IEnumerable<int>> scanDurationGeneratorFunc
    )
    {
      this.paaMacAddress = paaMacAddress;
      this.scanDurationGeneratorFunc = scanDurationGeneratorFunc ?? throw new ArgumentNullException(nameof(scanDurationGeneratorFunc));
    }

    public override SkStackActiveScanOptions Clone() => new FindByMacAddressActiveScanOptions(paaMacAddress, scanDurationGeneratorFunc);
    internal override bool SelectPanaAuthenticationAgent(SkStackPanDescription desc) => desc.MacAddress.Equals(paaMacAddress);
    internal override IEnumerable<int> YieldScanDurationFactors() => scanDurationGeneratorFunc();
  }

  /*
    * instance members
    */
  // TODO: public IProgress<int> Progress { get; set; }
  public abstract SkStackActiveScanOptions Clone();
  object ICloneable.Clone() => Clone();
  internal abstract bool SelectPanaAuthenticationAgent(SkStackPanDescription desc);
  internal abstract IEnumerable<int> YieldScanDurationFactors();
}
