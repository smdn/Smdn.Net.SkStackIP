// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Collections.Generic;

namespace Smdn.Net.SkStackIP {
  public class SkStackResponse {
    public bool Success => Status == SkStackResponseStatus.Ok ? true : false;
    public SkStackResponseStatus Status { get; internal set; } = SkStackResponseStatus.Undetermined;
    public ReadOnlyMemory<byte> StatusText { get; internal set; }
    public IReadOnlyList<ReadOnlyMemory<byte>> Lines => ResponseLines;
    internal readonly List<ReadOnlyMemory<byte>> ResponseLines = new(capacity: 1);

    internal SkStackResponse()
    {
    }

    internal void ThrowIfErrorStatus()
    {
      if (Success)
        return;

      var errorStatusText = StatusText.Span;
      ReadOnlySpan<byte> errorCode = default;
      ReadOnlySpan<byte> errorText = default;

      if (5 <= errorStatusText.Length && errorStatusText[4] == 0x20/*SP*/) {
        errorCode = errorStatusText.Slice(0, 4);
        errorText = errorStatusText.Slice(5);
      }
      else {
        errorCode = errorStatusText;
        errorText = default;
      }

      if (errorCode == SkStackErrorCodes.ER01.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Reserved error code ER01");
      if (errorCode == SkStackErrorCodes.ER02.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Reserved error code ER02");
      if (errorCode == SkStackErrorCodes.ER03.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Reserved error code ER03");
      if (errorCode == SkStackErrorCodes.ER04.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Unsupported command (ER04)");
      if (errorCode == SkStackErrorCodes.ER05.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Invalid number of arguments (ER05)");
      if (errorCode == SkStackErrorCodes.ER06.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Argument out-of-range or invalid format (ER06)");
      if (errorCode == SkStackErrorCodes.ER07.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Reserved error code ER07");
      if (errorCode == SkStackErrorCodes.ER08.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Reserved error code ER08");
      if (errorCode == SkStackErrorCodes.ER09.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "UART input error (ER09)");
      if (errorCode == SkStackErrorCodes.ER10.Span) throw new SkStackErrorResponseException(this, errorCode, errorText, "Command completed unsuccessfully (ER10)");

      throw new SkStackErrorResponseException(this, errorCode, errorText, "unknown or undefined error code");
    }

    internal ReadOnlyMemory<byte> GetFirstLineOrThrow()
    {
      if (ResponseLines.Count == 0)
        throw SkStackUnexpectedResponseException.CreateLackOfExpectedResponseText();

      return ResponseLines[0];
    }
  }
}