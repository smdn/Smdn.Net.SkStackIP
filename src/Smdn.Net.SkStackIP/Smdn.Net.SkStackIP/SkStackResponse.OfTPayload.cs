// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Net.SkStackIP;

public class SkStackResponse<TPayload> : SkStackResponse {
  public TPayload? Payload { get; internal set; }

  internal SkStackResponse()
    : base()
  {
  }
}
