// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.NetworkInformation;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackActiveScanOptionsTests : SkStackClientTestsBase {
  private void TestYieldScanDurationFactors(SkStackActiveScanOptions options, int[] expectedScanDurationFactors)
  {
    var methodYieldScanDurationFactors = options.GetType().GetMethod(
      name: "YieldScanDurationFactors",
      bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
      types: Type.EmptyTypes
    )!;
    var scanDurationFactors = (IEnumerable<int>)methodYieldScanDurationFactors.Invoke(obj: options, parameters: null)!;

    Assert.That(scanDurationFactors.ToArray(), Is.EquivalentTo(expectedScanDurationFactors));
  }

  [Test]
  public void YieldScanDurationFactors_Null()
    => TestYieldScanDurationFactors(SkStackActiveScanOptions.Null, Array.Empty<int>());

  [Test]
  public void YieldScanDurationFactors_Default()
    => TestYieldScanDurationFactors(SkStackActiveScanOptions.Default, [3, 4, 5, 6, 6, 6]);

  [TestCase(null, typeof(ArgumentNullException))]
  [TestCase(new int[0], null)]
  [TestCase(new int[] { 1 }, null)]
  [TestCase(new int[] { 1, 2, 3, 4, 5 }, null)]
  public void YieldScanDurationFactors_Create_ScanDurationGenerator_WithPaaSelector(int[]? scanDurationFactors, Type? typeOfExpectedException)
    => Assert.That(
      () => TestYieldScanDurationFactors(SkStackActiveScanOptions.Create(scanDurationFactors!, paaSelector: null), scanDurationFactors!),
      typeOfExpectedException is null ? Throws.Nothing : Throws.TypeOf(typeOfExpectedException)
    );

  [TestCase(null, typeof(ArgumentNullException))]
  [TestCase(new int[0], null)]
  [TestCase(new int[] { 1 }, null)]
  [TestCase(new int[] { 1, 2, 3, 4, 5 }, null)]
  public void YieldScanDurationFactors_Create_ScanDurationGenerator_WithPaaMacAddress(int[]? scanDurationFactors, Type? typeOfExpectedException)
    => Assert.That(
      () => TestYieldScanDurationFactors(SkStackActiveScanOptions.Create(scanDurationFactors!, paaMacAddress: PhysicalAddress.None), scanDurationFactors!),
      typeOfExpectedException is null ? Throws.Nothing : Throws.TypeOf(typeOfExpectedException)
    );

  [Test]
  public void YieldScanDurationFactors_Create_ScanDurationGeneratorFunc_ArgumentNull()
  {
    Assert.That(() => SkStackActiveScanOptions.Create(scanDurationGeneratorFunc: null!, paaSelector: null), Throws.ArgumentNullException);
    Assert.That(() => SkStackActiveScanOptions.Create(scanDurationGeneratorFunc: null!, paaMacAddress: PhysicalAddress.None), Throws.ArgumentNullException);
  }

  [TestCase(new int[0])]
  [TestCase(new int[] { 1 })]
  [TestCase(new int[] { 1, 2, 3, 4, 5 })]
  public void YieldScanDurationFactors_Create_ScanDurationGeneratorFunc_WithPaaSelector(int[] scanDurationFactors)
    => Assert.That(
      () => TestYieldScanDurationFactors(SkStackActiveScanOptions.Create(scanDurationGeneratorFunc: () => scanDurationFactors, paaSelector: null), scanDurationFactors!),
      Throws.Nothing
    );

  [TestCase(new int[0])]
  [TestCase(new int[] { 1 })]
  [TestCase(new int[] { 1, 2, 3, 4, 5 })]
  public void YieldScanDurationFactors_Create_ScanDurationGeneratorFunc_WithPaaMacAddress(int[] scanDurationFactors)
    => Assert.That(
      () => TestYieldScanDurationFactors(SkStackActiveScanOptions.Create(scanDurationGeneratorFunc: () => scanDurationFactors, paaMacAddress: PhysicalAddress.None), scanDurationFactors!),
      Throws.Nothing
    );
}
