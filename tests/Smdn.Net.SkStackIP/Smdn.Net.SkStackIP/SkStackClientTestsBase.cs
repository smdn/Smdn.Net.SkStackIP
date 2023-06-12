// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#nullable enable

using System;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

public class SkStackClientTestsBase {
  protected static readonly TimeSpan ResponseDelayInterval = TimeSpan.FromMilliseconds(25);

  private sealed class NullLoggerScope : IDisposable {
    public static readonly NullLoggerScope Instance = new();
    public void Dispose() { }
  }

  private class TestContextLogger : ILogger {
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
      TestContext.WriteLine(
        "{0:o} {1}:[{2}] {3}",
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
      );
    }
  }

  public static ILogger CreateLoggerForTestCase()
    => new TestContextLogger();
}
