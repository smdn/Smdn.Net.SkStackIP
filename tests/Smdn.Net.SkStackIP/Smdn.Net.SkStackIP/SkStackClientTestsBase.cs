// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Smdn.Net.SkStackIP;

public class SkStackClientTestsBase {
  protected static readonly TimeSpan ResponseDelayInterval = TimeSpan.FromMilliseconds(25);

  private sealed class NullLoggerScope : IDisposable {
    public static readonly NullLoggerScope Instance = new();
    public void Dispose() { }
  }

  private class TestContextLogger : ILogger {
    private readonly List<string> logs = new();

    public IReadOnlyList<string> Logs => logs;

    public TestContextLogger()
    {
    }

    public IDisposable BeginScope<TState>(TState state) => NullLoggerScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState, Exception?, string> formatter
    )
    {
      logs.Add(
        string.Format(
          provider: null,
          format: "{0:o} {1}:[{2}] {3}",
          DateTimeOffset.Now,
          logLevel switch {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            LogLevel.None => "none",
            _ => "????",
          },
          eventId.Id,
          formatter(state, exception)
        )
      );
    }
  }

  [TearDown]
  public virtual void TearDown()
  {
    var status = TestContext.CurrentContext.Result.Outcome.Status;

    if (status == TestStatus.Passed)
      return;

    if (loggerForTestCase is null)
      return;


    TestContext.WriteLine("{0}: {1}", status, TestContext.CurrentContext.Test.FullName);

    foreach (var log in loggerForTestCase.Logs) {
      TestContext.WriteLine(log);
    }
  }

  private TestContextLogger? loggerForTestCase;

  public ILogger CreateLoggerForTestCase()
  {
    loggerForTestCase ??= new();

    return loggerForTestCase;
  }
}
