// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1028

namespace Smdn.Net.SkStackIP;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 3.7. SKSENDTO' for detailed specifications.</para>
/// </remarks>
public enum SkStackUdpEncryption : byte {
  ForcePlainText = 0x00,
  ForceEncrypt = 0x01,
  EncryptIfAble = 0x02,
}
