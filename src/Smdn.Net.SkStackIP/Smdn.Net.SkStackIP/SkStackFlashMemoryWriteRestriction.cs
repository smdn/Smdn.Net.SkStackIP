// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Diagnostics;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// Provides a restriction to write to device's flash memory.
/// </summary>
/// <remarks>
/// For devices such as the <see href="https://www.rohm.co.jp/products/wireless-communication/specified-low-power-radio-modules/bp35a1-product">ROHM BP35A1</see>,
/// the number of writes to flash memory is limited up to approximately 10,000.
/// Be careful not to write unnecessarily to prevent damage to the flash memory.
/// </remarks>
/// <see cref="SkStackClient.SaveFlashMemoryAsync"/>
public abstract class SkStackFlashMemoryWriteRestriction {
  /// <summary>
  /// Create an <see cref="SkStackFlashMemoryWriteRestriction"/> instance that always grants write permission.
  /// </summary>
  /// <remarks>
  /// Be careful not to exceed the write limit, as the instance returned by this method will grant all write requests.
  /// </remarks>
  public static SkStackFlashMemoryWriteRestriction DangerousCreateAlwaysGrant()
    => new AlwaysGrantSkStackFlashMemoryWriteRestriction();

  private sealed class AlwaysGrantSkStackFlashMemoryWriteRestriction : SkStackFlashMemoryWriteRestriction {
    protected internal override bool IsRestricted() => false;
  }

  /// <summary>
  /// Create an <see cref="SkStackFlashMemoryWriteRestriction"/> instance that grants write permission only if a certain amount of time has elapsed.
  /// </summary>
  public static SkStackFlashMemoryWriteRestriction CreateGrantIfElapsed(TimeSpan interval)
  {
    if (interval <= TimeSpan.Zero)
      throw new ArgumentOutOfRangeException(message: "must be non zero positive value", paramName: nameof(interval), actualValue: interval);

    return new GrantIfElapsedSkStackFlashMemoryWriteRestriction(interval);
  }

  private sealed class GrantIfElapsedSkStackFlashMemoryWriteRestriction : SkStackFlashMemoryWriteRestriction {
    private readonly TimeSpan interval;
    private Stopwatch? stopwatch;

    public GrantIfElapsedSkStackFlashMemoryWriteRestriction(TimeSpan interval)
    {
      this.interval = interval;
    }

    protected internal override bool IsRestricted()
    {
      const bool Permit = false;

      if (stopwatch is null) {
        stopwatch = Stopwatch.StartNew();

        return Permit; // permit the initial write
      }

      if (interval <= stopwatch.Elapsed) {
        stopwatch.Restart();

        return Permit; // permit if specific interval has elapsed
      }

      return !Permit;
    }
  }

  /*
   * instance members
   */
  protected internal abstract bool IsRestricted();
}
