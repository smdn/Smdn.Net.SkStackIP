// SPDX-FileCopyrightText: 2025 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using NUnit.Framework;

namespace Smdn.Devices.BP35XX;

[TestFixture]
public class BP35A1OptionsTests {
  [Test]
  public void Configure_ArgumentNull()
  {
    var options = new BP35A1Options();

    Assert.That(
      () => options.Configure(baseOptions: null!),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("baseOptions")
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Configure()
  {
    yield return new BP35A1Options() {
      SerialPortName = "/dev/null",
      BaudRate = BP35UartBaudRate.Baud9600,
      UseFlowControl = true,
      TryLoadFlashMemory = true,
    };

    yield return new BP35A1Options() {
      SerialPortName = "NUL",
      BaudRate = BP35UartBaudRate.Baud19200,
      UseFlowControl = true,
      TryLoadFlashMemory = true,
    };

    yield return new BP35A1Options() {
      SerialPortName = default,
      BaudRate = default,
      UseFlowControl = false,
      TryLoadFlashMemory = true,
    };

    yield return new BP35A1Options() {
      SerialPortName = default,
      BaudRate = default,
      UseFlowControl = true,
      TryLoadFlashMemory = false,
    };
  }

  [TestCaseSource(nameof(YieldTestCases_Configure))]
  public void Configure(BP35A1Options baseOptions)
  {
    var options = new BP35A1Options();
    var configuredOptions = options.Configure(baseOptions);

    Assert.That(configuredOptions, Is.SameAs(options));
    Assert.That(configuredOptions.SerialPortName, Is.EqualTo(baseOptions.SerialPortName));
    Assert.That(configuredOptions.BaudRate, Is.EqualTo(baseOptions.BaudRate));
    Assert.That(configuredOptions.UseFlowControl, Is.EqualTo(baseOptions.UseFlowControl));
    Assert.That(configuredOptions.TryLoadFlashMemory, Is.EqualTo(baseOptions.TryLoadFlashMemory));
  }
}
