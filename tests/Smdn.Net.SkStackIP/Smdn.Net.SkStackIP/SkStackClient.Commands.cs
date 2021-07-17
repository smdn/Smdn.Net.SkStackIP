// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  public class SkStackClientCommandsTestsBase {
    protected static readonly TimeSpan ResponseDelayInterval = TimeSpan.FromMilliseconds(25);

    protected IServiceProvider ServiceProvider;

    [SetUp]
    public void SetUp()
    {
      var services = new ServiceCollection();

      services.AddLogging(
        builder => builder
          .AddSimpleConsole(static options => options.SingleLine = true)
          .AddFilter(static level => true/*level <= LogLevel.Trace*/)
      );

      ServiceProvider = services.BuildServiceProvider();
    }
  }
}