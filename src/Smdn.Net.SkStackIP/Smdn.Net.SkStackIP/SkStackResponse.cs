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
    internal readonly struct NullPayload { }

    public bool Success => Status == SkStackResponseStatus.Ok ? true : false;
    public SkStackResponseStatus Status { get; internal set; } = SkStackResponseStatus.Undetermined;
    public ReadOnlyMemory<byte> StatusText { get; internal set; }

    internal SkStackResponse()
    {
    }

    internal bool TryParseErrorStatus(
      out SkStackErrorCode errorCode,
      out ReadOnlyMemory<byte> errorText,
      out string errorMessage
    )
    {
      errorCode = default;
      errorText = default;
      errorMessage = default;

      if (Status == SkStackResponseStatus.Ok || Status == SkStackResponseStatus.Undetermined)
        return false; // not error status

      ReadOnlySpan<byte> errorCodeName = default;

      if (5 <= StatusText.Length && StatusText.Span[4] == SkStack.SP) {
        errorCodeName = StatusText.Span.Slice(0, 4);
        errorText = StatusText.Slice(5);
      }
      else {
        errorCodeName = StatusText.Span;
        errorText = default;
      }

      errorCode = SkStackErrorCodeNames.ParseErrorCode(errorCodeName);

      errorMessage = errorCode switch {
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
      };

      return true;
    }

    internal void ThrowIfErrorStatus()
    {
      if (!TryParseErrorStatus(out var errorCode, out var errorText, out var errorMessage))
        return;

      throw new SkStackErrorResponseException(
        response: this,
        errorCode: errorCode,
        errorText: errorText.Span,
        message: errorMessage
      );
    }
  }
}