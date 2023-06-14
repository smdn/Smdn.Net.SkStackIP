// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Net.SkStackIP;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 4.8. EVENT' for detailed specifications.</para>
/// </remarks>
public enum SkStackEventNumber : byte {
  Undefined = 0x00,

  NeighborSolicitationReceived = 0x01,
  NeighborAdvertisementReceived = 0x02,
  EchoRequestReceived = 0x05,
  EnergyDetectScanCompleted = 0x1F,
  BeaconReceived = 0x20,
  UdpSendCompleted = 0x21,
  ActiveScanCompleted = 0x22,

  PanaSessionEstablishmentError = 0x24,
  PanaSessionEstablishmentCompleted = 0x25,
  PanaSessionTerminationRequestReceived = 0x26,
  PanaSessionTerminationCompleted = 0x27,
  PanaSessionTerminationTimedOut = 0x28,
  PanaSessionExpired = 0x29,

  /// <seealso href="https://www.arib.or.jp/kikaku/kikaku_tushin/desc/std-t108.html">[ARIB STD-T108] 920MHz帯テレメータ用、テレコントロール用及びデータ伝送用無線設備</seealso>
  /// <seealso href="http://www.arib.or.jp/english/html/overview/doc/5-STD-T108v1_3-E1.pdf">[ARIB STD-T108] 920MHz帯テレメータ用、テレコントロール用及びデータ伝送用無線設備 (PDF)</seealso>
  TransmissionTimeControlLimitationActivated = 0x32,

  /// <seealso href="https://www.arib.or.jp/kikaku/kikaku_tushin/desc/std-t108.html">[ARIB STD-T108] 920MHz帯テレメータ用、テレコントロール用及びデータ伝送用無線設備</seealso>
  /// <seealso href="http://www.arib.or.jp/english/html/overview/doc/5-STD-T108v1_3-E1.pdf">[ARIB STD-T108] 920MHz帯テレメータ用、テレコントロール用及びデータ伝送用無線設備 (PDF)</seealso>
  TransmissionTimeControlLimitationDeactivated = 0x33,

  /// <summary><c>SKDSLEEP</c>; Wake-up signal received.</summary>
  /// <remarks>This event is not clearly documented.</remarks>
  WakeupSignalReceived = 0xC0,
}
