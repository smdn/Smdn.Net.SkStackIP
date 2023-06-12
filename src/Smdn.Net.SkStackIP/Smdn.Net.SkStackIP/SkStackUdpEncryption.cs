// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP;

/// <remarks>reference: BP35A1コマンドリファレンス 3.7. SKSENDTO</remarks>
public enum SkStackUdpEncryption : byte {
  ForcePlainText = 0x00,
  ForceEncrypt = 0x01,
  EncryptIfAble = 0x02,
}
