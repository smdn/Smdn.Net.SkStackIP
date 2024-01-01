// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Text;

namespace Smdn.Net.SkStackIP;

internal static class IPAddressExtensions {
  public static string ToLongFormatString(this IPAddress ipv6Address)
  {
    ArgumentNullException.ThrowIfNull(ipv6Address);

    var addressBytes = ipv6Address.GetAddressBytes();
    var sb = new StringBuilder(capacity: 32 + 7);

    for (var i = 0; i < 16; i += 2) {
      if (0 < i)
        sb.Append(':');

      sb.Append(addressBytes[i].ToString("X2", provider: null));
      sb.Append(addressBytes[i + 1].ToString("X2", provider: null));
    }

    return sb.ToString();
  }
}
