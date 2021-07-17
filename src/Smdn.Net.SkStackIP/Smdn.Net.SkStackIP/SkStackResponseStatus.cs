// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP {
  public enum SkStackResponseStatus {
    Undetermined = 0, // used as default(SkStackResponseStatus)
    Ok = +1,
    Fail = -1,
  }
}