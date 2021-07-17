// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  public class SkStackResponse<TPayload> : SkStackResponse {
    public TPayload Payload { get; internal set; }

    internal SkStackResponse()
      : base()
    {
    }
  }

  public class SkStackResponse {
    internal readonly struct NullPayload {}

    public bool Success => Status == SkStackResponseStatus.Ok ? true : false;
    public SkStackResponseStatus Status { get; internal set; } = SkStackResponseStatus.Undetermined;
    public ReadOnlyMemory<byte> StatusText { get; internal set; }

    internal SkStackResponse()
    {
    }

    internal void ThrowIfErrorStatus()
    {
      if (Status == SkStackResponseStatus.Ok || Status == SkStackResponseStatus.Undetermined)
        return;

      var errorStatusText = StatusText.Span;
      ReadOnlySpan<byte> errorCodeName = default;
      ReadOnlySpan<byte> errorText = default;

      if (5 <= errorStatusText.Length && errorStatusText[4] == SkStack.SP) {
        errorCodeName = errorStatusText.Slice(0, 4);
        errorText = errorStatusText.Slice(5);
      }
      else {
        errorCodeName = errorStatusText;
        errorText = default;
      }

      var errorCode = SkStackErrorCodeNames.ParseErrorCode(errorCodeName);

      throw new SkStackErrorResponseException(
        this,
        errorCode,
        errorText,
        errorCode switch {
          SkStackErrorCode.ER01 => "Reserved error code",
          SkStackErrorCode.ER02 => "Reserved error code",
          SkStackErrorCode.ER03 => "Reserved error code",
          SkStackErrorCode.ER04 => "Unsupported command",
          SkStackErrorCode.ER05 => "Invalid number of arguments",
          SkStackErrorCode.ER06 => "Argument out-of-range or invalid format",
          SkStackErrorCode.ER07 => "Reserved error code",
          SkStackErrorCode.ER08 => "Reserved error code",
          SkStackErrorCode.ER09 => "UART input error",
          SkStackErrorCode.ER10 => "Command completed unsuccessfully",
          _ => "unknown or undefined error code"
        }
      );
    }
  }
}