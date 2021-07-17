// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.BP35A1 {
  internal enum BP35A1Event {
    /// <summary>BP35A1コマンドリファレンス 3.30. WOPT</summary>
    WOPT,
    /// <summary>BP35A1コマンドリファレンス 3.31. ROPT</summary>
    ROPT,
    /// <summary>BP35A1コマンドリファレンス 3.32. WUART</summary>
    WUART,
    /// <summary>BP35A1コマンドリファレンス 3.33. RUART</summary>
    RUART,
  }
}