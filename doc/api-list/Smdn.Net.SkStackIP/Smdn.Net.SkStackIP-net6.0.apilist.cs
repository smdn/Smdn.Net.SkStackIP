// Smdn.Net.SkStackIP.dll (Smdn.Net.SkStackIP-1.2.0)
//   Name: Smdn.Net.SkStackIP
//   AssemblyVersion: 1.2.0.0
//   InformationalVersion: 1.2.0+0e506f4265dfd6eb80e5f98b4486e10a5cda9d99
//   TargetFramework: .NETCoreApp,Version=v6.0
//   Configuration: Release
//   Referenced assemblies:
//     Microsoft.Extensions.Logging.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Polly.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=c8a3ffc3f8f825cc
//     Smdn.Fundamental.ControlPicture, Version=3.0.0.1, Culture=neutral
//     Smdn.Fundamental.PrintableEncoding.Hexadecimal, Version=3.0.0.0, Culture=neutral
//     System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.ComponentModel.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.IO.Pipelines, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Net.NetworkInformation, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Net.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Threading, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
#nullable enable annotations

using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Smdn.Net.SkStackIP;
using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  public enum SkStackERXUDPDataFormat : int {
    Binary = 0,
    HexAsciiText = 1,
  }

  public enum SkStackErrorCode : int {
    ER01 = 1,
    ER02 = 2,
    ER03 = 3,
    ER04 = 4,
    ER05 = 5,
    ER06 = 6,
    ER07 = 7,
    ER08 = 8,
    ER09 = 9,
    ER10 = 10,
    Undefined = 0,
  }

  public enum SkStackEventNumber : byte {
    ActiveScanCompleted = 34,
    BeaconReceived = 32,
    EchoRequestReceived = 5,
    EnergyDetectScanCompleted = 31,
    NeighborAdvertisementReceived = 2,
    NeighborSolicitationReceived = 1,
    PanaSessionEstablishmentCompleted = 37,
    PanaSessionEstablishmentError = 36,
    PanaSessionExpired = 41,
    PanaSessionTerminationCompleted = 39,
    PanaSessionTerminationRequestReceived = 38,
    PanaSessionTerminationTimedOut = 40,
    TransmissionTimeControlLimitationActivated = 50,
    TransmissionTimeControlLimitationDeactivated = 51,
    UdpSendCompleted = 33,
    Undefined = 0,
    WakeupSignalReceived = 192,
  }

  public enum SkStackResponseStatus : int {
    Fail = -1,
    Ok = 1,
    Undetermined = 0,
  }

  public enum SkStackUdpEncryption : byte {
    EncryptIfAble = 2,
    ForceEncrypt = 1,
    ForcePlainText = 0,
  }

  public enum SkStackUdpPortHandle : byte {
    Handle1 = 1,
    Handle2 = 2,
    Handle3 = 3,
    Handle4 = 4,
    Handle5 = 5,
    Handle6 = 6,
    None = 0,
  }

  public abstract class SkStackActiveScanOptions : ICloneable {
    public static SkStackActiveScanOptions Default { get; }
    public static SkStackActiveScanOptions Null { get; }
    public static SkStackActiveScanOptions ScanUntilFind { get; }

    public static SkStackActiveScanOptions Create(IEnumerable<int> scanDurationGenerator, PhysicalAddress paaMacAddress) {}
    public static SkStackActiveScanOptions Create(IEnumerable<int> scanDurationGenerator, Predicate<SkStackPanDescription>? paaSelector = null) {}

    protected SkStackActiveScanOptions() {}

    public abstract SkStackActiveScanOptions Clone();
    object ICloneable.Clone() {}
  }

  public class SkStackClient : IDisposable {
    public static readonly TimeSpan SKSCANDefaultDuration; // = "00:00:00.0480000"
    public static readonly TimeSpan SKSCANMaxDuration; // = "00:02:37.2960000"
    public static readonly TimeSpan SKSCANMinDuration; // = "00:00:00.0192000"

    public event EventHandler<SkStackPanaSessionEventArgs>? PanaSessionEstablished;
    public event EventHandler<SkStackPanaSessionEventArgs>? PanaSessionExpired;
    public event EventHandler<SkStackPanaSessionEventArgs>? PanaSessionTerminated;
    public event EventHandler<SkStackEventArgs>? Slept;
    public event EventHandler<SkStackEventArgs>? WokeUp;

    public SkStackClient(PipeWriter sender, PipeReader receiver, SkStackERXUDPDataFormat erxudpDataFormat = SkStackERXUDPDataFormat.Binary, ILogger? logger = null) {}
    public SkStackClient(Stream stream, bool leaveStreamOpen = true, SkStackERXUDPDataFormat erxudpDataFormat = SkStackERXUDPDataFormat.Binary, ILogger? logger = null) {}

    public SkStackERXUDPDataFormat ERXUDPDataFormat { get; protected set; }
    [MemberNotNullWhen(true, "PanaSessionPeerAddress")]
    public bool IsPanaSessionAlive { [MemberNotNullWhen(true, "PanaSessionPeerAddress")] get; }
    protected ILogger? Logger { get; }
    public IPAddress? PanaSessionPeerAddress { get; }
    public TimeSpan ReceiveResponseDelay { get; set; }
    public TimeSpan ReceiveUdpPollingInterval { get; set; }
    public ISynchronizeInvoke? SynchronizingObject { get; set; }

    public ValueTask<IReadOnlyList<SkStackPanDescription>> ActiveScanAsync(Action<IBufferWriter<byte>> writeRBID, Action<IBufferWriter<byte>> writePassword, SkStackActiveScanOptions? scanOptions = null, CancellationToken cancellationToken = default) {}
    public ValueTask<IReadOnlyList<SkStackPanDescription>> ActiveScanAsync(ReadOnlyMemory<byte> rbid, ReadOnlyMemory<byte> password, SkStackActiveScanOptions? scanOptions = null, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(Action<IBufferWriter<byte>> writeRBID, Action<IBufferWriter<byte>> writePassword, IPAddress paaAddress, SkStackChannel channel, int panId, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(Action<IBufferWriter<byte>> writeRBID, Action<IBufferWriter<byte>> writePassword, PhysicalAddress paaMacAddress, SkStackChannel channel, int panId, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(Action<IBufferWriter<byte>> writeRBID, Action<IBufferWriter<byte>> writePassword, SkStackActiveScanOptions? scanOptions = null, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(Action<IBufferWriter<byte>> writeRBID, Action<IBufferWriter<byte>> writePassword, SkStackPanDescription pan, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(ReadOnlyMemory<byte> rbid, ReadOnlyMemory<byte> password, IPAddress paaAddress, SkStackChannel channel, int panId, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(ReadOnlyMemory<byte> rbid, ReadOnlyMemory<byte> password, IPAddress paaAddress, int channelNumber, int panId, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(ReadOnlyMemory<byte> rbid, ReadOnlyMemory<byte> password, PhysicalAddress paaMacAddress, SkStackChannel channel, int panId, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(ReadOnlyMemory<byte> rbid, ReadOnlyMemory<byte> password, PhysicalAddress paaMacAddress, int channelNumber, int panId, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(ReadOnlyMemory<byte> rbid, ReadOnlyMemory<byte> password, SkStackActiveScanOptions? scanOptions = null, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(ReadOnlyMemory<byte> rbid, ReadOnlyMemory<byte> password, SkStackPanDescription pan, CancellationToken cancellationToken = default) {}
    public async ValueTask<IPAddress> ConvertToIPv6LinkLocalAddressAsync(PhysicalAddress macAddress, CancellationToken cancellationToken = default) {}
    public ValueTask DisableFlashMemoryAutoLoadAsync(CancellationToken cancellationToken = default) {}
    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    public ValueTask EnableFlashMemoryAutoLoadAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask<IReadOnlyList<IPAddress>> GetAvailableAddressListAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask<IReadOnlyList<SkStackUdpPort>> GetListeningUdpPortListAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask<IReadOnlyDictionary<IPAddress, PhysicalAddress>> GetNeighborCacheListAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask<IReadOnlyList<SkStackUdpPortHandle>> GetUnusedUdpPortHandleListAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask LoadFlashMemoryAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask<SkStackUdpPort> PrepareUdpPortAsync(int port, CancellationToken cancellationToken = default) {}
    public ValueTask<IPAddress> ReceiveUdpAsync(int port, IBufferWriter<byte> buffer, CancellationToken cancellationToken = default) {}
    public ValueTask<IPAddress> ReceiveUdpEchonetLiteAsync(IBufferWriter<byte> buffer, CancellationToken cancellationToken = default) {}
    public ValueTask SaveFlashMemoryAsync(SkStackFlashMemoryWriteRestriction restriction, CancellationToken cancellationToken = default) {}
    internal protected ValueTask<SkStackResponse<TPayload>> SendCommandAsync<TPayload>(ReadOnlyMemory<byte> command, Action<ISkStackCommandLineWriter>? writeArguments, SkStackSequenceParser<TPayload> parseResponsePayload, SkStackProtocolSyntax? syntax = null, bool throwIfErrorStatus = true, CancellationToken cancellationToken = default) {}
    internal protected async ValueTask<SkStackResponse> SendCommandAsync(ReadOnlyMemory<byte> command, Action<ISkStackCommandLineWriter>? writeArguments = null, SkStackProtocolSyntax? syntax = null, bool throwIfErrorStatus = true, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKADDNBRAsync(IPAddress ipv6Address, PhysicalAddress macAddress, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<string>> SendSKAPPVERAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKDSLEEPAsync(bool waitUntilWakeUp = false, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKERASEAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<(IPAddress LinkLocalAddress, PhysicalAddress MacAddress, SkStackChannel Channel, int PanId, int Addr16)>> SendSKINFOAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKJOINAsync(IPAddress ipv6address, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<IPAddress>> SendSKLL64Async(PhysicalAddress macAddress, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKLOADAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, IPAddress Address)> SendSKREJOINAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKRESETAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSAVEAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, IReadOnlyList<SkStackPanDescription> PanDescriptions)> SendSKSCANActiveScanAsync(TimeSpan duration = default, uint channelMask = uint.MaxValue, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, IReadOnlyList<SkStackPanDescription> PanDescriptions)> SendSKSCANActiveScanAsync(int durationFactor, uint channelMask = uint.MaxValue, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, IReadOnlyList<SkStackPanDescription> PanDescriptions)> SendSKSCANActiveScanPairAsync(TimeSpan duration = default, uint channelMask = uint.MaxValue, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, IReadOnlyList<SkStackPanDescription> PanDescriptions)> SendSKSCANActiveScanPairAsync(int durationFactor, uint channelMask = uint.MaxValue, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, IReadOnlyDictionary<SkStackChannel, decimal> ScanResult)> SendSKSCANEnergyDetectScanAsync(TimeSpan duration = default, uint channelMask = uint.MaxValue, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, IReadOnlyDictionary<SkStackChannel, decimal> ScanResult)> SendSKSCANEnergyDetectScanAsync(int durationFactor, uint channelMask = uint.MaxValue, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, bool IsCompletedSuccessfully)> SendSKSENDTOAsync(SkStackUdpPort port, IPAddress destinationAddress, int destinationPort, ReadOnlyMemory<byte> data, SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, bool IsCompletedSuccessfully)> SendSKSENDTOAsync(SkStackUdpPort port, IPEndPoint destination, ReadOnlyMemory<byte> data, SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, bool IsCompletedSuccessfully)> SendSKSENDTOAsync(SkStackUdpPortHandle handle, IPAddress destinationAddress, int destinationPort, ReadOnlyMemory<byte> data, SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble, CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, bool IsCompletedSuccessfully)> SendSKSENDTOAsync(SkStackUdpPortHandle handle, IPEndPoint destination, ReadOnlyMemory<byte> data, SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSETPWDAsync(Action<IBufferWriter<byte>> writePassword, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSETPWDAsync(ReadOnlyMemory<byte> password, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSETPWDAsync(ReadOnlyMemory<char> password, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSETRBIDAsync(Action<IBufferWriter<byte>> writeRBID, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSETRBIDAsync(ReadOnlyMemory<byte> id, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSETRBIDAsync(ReadOnlyMemory<char> id, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<TValue>> SendSKSREGAsync<TValue>(SkStackRegister.RegisterEntry<TValue> register, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKSREGAsync<TValue>(SkStackRegister.RegisterEntry<TValue> register, TValue @value, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<IReadOnlyList<IPAddress>>> SendSKTABLEAvailableAddressListAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<IReadOnlyList<SkStackUdpPort>>> SendSKTABLEListeningPortListAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<IReadOnlyDictionary<IPAddress, PhysicalAddress>>> SendSKTABLENeighborCacheListAsync(CancellationToken cancellationToken = default) {}
    public async ValueTask<(SkStackResponse Response, bool IsCompletedSuccessfully)> SendSKTERMAsync(CancellationToken cancellationToken = default) {}
    public ValueTask<(SkStackResponse Response, SkStackUdpPort UdpPort)> SendSKUDPPORTAsync(SkStackUdpPortHandle handle, int port, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse> SendSKUDPPORTUnsetAsync(SkStackUdpPortHandle handle, CancellationToken cancellationToken = default) {}
    public ValueTask<SkStackResponse<Version>> SendSKVERAsync(CancellationToken cancellationToken = default) {}
    public ValueTask SendUdpEchonetLiteAsync(ReadOnlyMemory<byte> buffer, ResiliencePipeline? resiliencePipeline = null, CancellationToken cancellationToken = default) {}
    protected async ValueTask SetFlashMemoryAutoLoadAsync(bool trueIfEnable, CancellationToken cancellationToken = default) {}
    public void StartCapturingUdpReceiveEvents(int port) {}
    public void StopCapturingUdpReceiveEvents(int port) {}
    public ValueTask<bool> TerminatePanaSessionAsync(CancellationToken cancellationToken = default) {}
    protected void ThrowIfDisposed() {}
    internal protected void ThrowIfPanaSessionAlreadyEstablished() {}
    [MemberNotNull("PanaSessionPeerAddress")]
    internal protected void ThrowIfPanaSessionIsNotEstablished() {}
  }

  public class SkStackCommandNotSupportedException : SkStackErrorResponseException {
  }

  public class SkStackErrorResponseException : SkStackResponseException {
    public SkStackErrorCode ErrorCode { get; }
    public string ErrorText { get; }
    public SkStackResponse Response { get; }
  }

  public class SkStackEventArgs : EventArgs {
    public SkStackEventNumber EventNumber { get; }
  }

  public class SkStackFlashMemoryIOException : SkStackErrorResponseException {
  }

  public abstract class SkStackFlashMemoryWriteRestriction {
    public static SkStackFlashMemoryWriteRestriction CreateGrantIfElapsed(TimeSpan interval) {}
    public static SkStackFlashMemoryWriteRestriction DangerousCreateAlwaysGrant() {}

    protected SkStackFlashMemoryWriteRestriction() {}

    internal protected abstract bool IsRestricted();
  }

  public static class SkStackKnownPortNumbers {
    public const int EchonetLite = 3610;
    public const int Pana = 716;
  }

  public class SkStackPanaSessionEstablishmentException : SkStackPanaSessionException {
    public SkStackChannel? Channel { get; }
    public IPAddress? PaaAddress { get; }
    public int? PanId { get; }
  }

  public sealed class SkStackPanaSessionEventArgs : SkStackEventArgs {
    public IPAddress PanaSessionPeerAddress { get; }
  }

  public abstract class SkStackPanaSessionException : InvalidOperationException {
    public IPAddress Address { get; }
    public SkStackEventNumber EventNumber { get; }
  }

  public sealed class SkStackPanaSessionInfo {
    public SkStackChannel Channel { get; }
    public IPAddress LocalAddress { get; }
    public PhysicalAddress LocalMacAddress { get; }
    public int PanId { get; }
    public IPAddress PeerAddress { get; }
    public PhysicalAddress PeerMacAddress { get; }
  }

  public static class SkStackRegister {
    public abstract class RegisterEntry<TValue> {
      private protected delegate bool ExpectValueFunc(ref SequenceReader<byte> reader, out TValue @value);

      public bool IsReadable { get; }
      public bool IsWritable { get; }
      public TValue MaxValue { get; }
      public TValue MinValue { get; }
      public string Name { get; }
    }

    public static SkStackRegister.RegisterEntry<bool> AcceptIcmpEcho { get; }
    public static SkStackRegister.RegisterEntry<ulong> AccumulatedSendTimeInMilliseconds { get; }
    public static SkStackRegister.RegisterEntry<SkStackChannel> Channel { get; }
    public static SkStackRegister.RegisterEntry<bool> EnableAutoLoad { get; }
    public static SkStackRegister.RegisterEntry<bool> EnableAutoReauthentication { get; }
    public static SkStackRegister.RegisterEntry<bool> EnableEchoback { get; }
    public static SkStackRegister.RegisterEntry<bool> EncryptIPMulticast { get; }
    public static SkStackRegister.RegisterEntry<uint> FrameCounter { get; }
    public static SkStackRegister.RegisterEntry<bool> IsSendingRestricted { get; }
    public static SkStackRegister.RegisterEntry<ReadOnlyMemory<byte>> PairingId { get; }
    public static SkStackRegister.RegisterEntry<ushort> PanId { get; }
    public static SkStackRegister.RegisterEntry<TimeSpan> PanaSessionLifetimeInSeconds { get; }
    public static SkStackRegister.RegisterEntry<bool> RespondBeaconRequest { get; }
    public static SkStackRegister.RegisterEntry<SkStackChannel> S02 { get; }
    public static SkStackRegister.RegisterEntry<ushort> S03 { get; }
    public static SkStackRegister.RegisterEntry<uint> S07 { get; }
    public static SkStackRegister.RegisterEntry<ReadOnlyMemory<byte>> S0A { get; }
    public static SkStackRegister.RegisterEntry<bool> S15 { get; }
    public static SkStackRegister.RegisterEntry<TimeSpan> S16 { get; }
    public static SkStackRegister.RegisterEntry<bool> S17 { get; }
    public static SkStackRegister.RegisterEntry<bool> SA0 { get; }
    public static SkStackRegister.RegisterEntry<bool> SA1 { get; }
    public static SkStackRegister.RegisterEntry<bool> SFB { get; }
    public static SkStackRegister.RegisterEntry<ulong> SFD { get; }
    public static SkStackRegister.RegisterEntry<bool> SFE { get; }
    public static SkStackRegister.RegisterEntry<bool> SFF { get; }
  }

  public class SkStackResponse {
    public SkStackResponseStatus Status { get; }
    public ReadOnlyMemory<byte> StatusText { get; }
    public bool Success { get; }
  }

  public class SkStackResponseException : InvalidOperationException {
    public SkStackResponseException() {}
    public SkStackResponseException(string message) {}
    public SkStackResponseException(string message, Exception? innerException = null) {}
  }

  public class SkStackResponse<TPayload> : SkStackResponse {
    public TPayload Payload { get; }
  }

  public class SkStackUartIOException : SkStackErrorResponseException {
  }

  public class SkStackUdpSendFailedException : InvalidOperationException {
    public SkStackUdpSendFailedException() {}
    public SkStackUdpSendFailedException(string message) {}
    public SkStackUdpSendFailedException(string message, Exception? innerException = null) {}
    public SkStackUdpSendFailedException(string message, SkStackUdpPortHandle portHandle, IPAddress peerAddress, Exception? innerException = null) {}

    public IPAddress? PeerAddress { get; }
    public SkStackUdpPortHandle PortHandle { get; }
  }

  public class SkStackUdpSendResultIndeterminateException : InvalidOperationException {
    public SkStackUdpSendResultIndeterminateException() {}
    public SkStackUdpSendResultIndeterminateException(string message) {}
    public SkStackUdpSendResultIndeterminateException(string message, Exception? innerException = null) {}
  }

  public readonly struct SkStackChannel :
    IComparable<SkStackChannel>,
    IEquatable<SkStackChannel>
  {
    public static readonly IReadOnlyDictionary<int, SkStackChannel> Channels; // = "System.Collections.Generic.Dictionary`2[System.Int32,Smdn.Net.SkStackIP.SkStackChannel]"
    public static readonly SkStackChannel Empty; // = "0ch (S02=0x00, 0 MHz)"

    public static SkStackChannel Channel33 { get; }
    public static SkStackChannel Channel34 { get; }
    public static SkStackChannel Channel35 { get; }
    public static SkStackChannel Channel36 { get; }
    public static SkStackChannel Channel37 { get; }
    public static SkStackChannel Channel38 { get; }
    public static SkStackChannel Channel39 { get; }
    public static SkStackChannel Channel40 { get; }
    public static SkStackChannel Channel41 { get; }
    public static SkStackChannel Channel42 { get; }
    public static SkStackChannel Channel43 { get; }
    public static SkStackChannel Channel44 { get; }
    public static SkStackChannel Channel45 { get; }
    public static SkStackChannel Channel46 { get; }
    public static SkStackChannel Channel47 { get; }
    public static SkStackChannel Channel48 { get; }
    public static SkStackChannel Channel49 { get; }
    public static SkStackChannel Channel50 { get; }
    public static SkStackChannel Channel51 { get; }
    public static SkStackChannel Channel52 { get; }
    public static SkStackChannel Channel53 { get; }
    public static SkStackChannel Channel54 { get; }
    public static SkStackChannel Channel55 { get; }
    public static SkStackChannel Channel56 { get; }
    public static SkStackChannel Channel57 { get; }
    public static SkStackChannel Channel58 { get; }
    public static SkStackChannel Channel59 { get; }
    public static SkStackChannel Channel60 { get; }

    public static bool operator == (SkStackChannel x, SkStackChannel y) {}
    public static bool operator != (SkStackChannel x, SkStackChannel y) {}

    public int ChannelNumber { get; }
    public decimal FrequencyMHz { get; }
    public bool IsEmpty { get; }

    public bool Equals(SkStackChannel other) {}
    public override bool Equals(object? obj) {}
    public override int GetHashCode() {}
    int IComparable<SkStackChannel>.CompareTo(SkStackChannel other) {}
    public override string ToString() {}
  }

  public readonly struct SkStackPanDescription {
    public SkStackChannel Channel { get; }
    public int ChannelPage { get; }
    public int Id { get; }
    public PhysicalAddress MacAddress { get; }
    public uint PairingId { get; }
    public decimal Rssi { get; }

    public override string ToString() {}
  }

  public readonly struct SkStackUdpPort {
    public static readonly SkStackUdpPort Null; // = "0 (#0)"

    public SkStackUdpPortHandle Handle { get; }
    public bool IsNull { get; }
    public bool IsUnused { get; }
    public int Port { get; }

    public override string ToString() {}
  }
}

namespace Smdn.Net.SkStackIP.Protocol {
  public delegate TResult SkStackSequenceParser<TResult>(ISkStackSequenceParserContext context);

  public interface ISkStackCommandLineWriter {
    void WriteMaskedToken(ReadOnlySpan<byte> token);
    void WriteToken(ReadOnlySpan<byte> token);
  }

  public interface ISkStackSequenceParserContext {
    ReadOnlySequence<byte> UnparsedSequence { get; }

    void Complete();
    void Complete(SequenceReader<byte> consumedReader);
    void Continue();
    ISkStackSequenceParserContext CreateCopy();
    virtual SequenceReader<byte> CreateReader() {}
    void Ignore();
    void SetAsIncomplete();
    void SetAsIncomplete(SequenceReader<byte> incompleteReader);
  }

  public abstract class SkStackProtocolSyntax {
    public static SkStackProtocolSyntax Default { get; }

    protected SkStackProtocolSyntax() {}

    public abstract ReadOnlySpan<byte> EndOfCommandLine { get; }
    public virtual ReadOnlySpan<byte> EndOfEchobackLine { get; }
    public abstract ReadOnlySpan<byte> EndOfStatusLine { get; }
    public abstract bool ExpectStatusLine { get; }
  }

  public static class SkStackTokenParser {
    public static bool Expect<TValue>(ref SequenceReader<byte> reader, int length, Converter<ReadOnlySequence<byte>, TValue> converter, [NotNullWhen(true)] out TValue @value) {}
    public static bool ExpectADDR16(ref SequenceReader<byte> reader, out ushort @value) {}
    public static bool ExpectADDR64(ref SequenceReader<byte> reader, [NotNullWhen(true)] out PhysicalAddress? @value) {}
    public static bool ExpectBinary(ref SequenceReader<byte> reader, out bool @value) {}
    public static bool ExpectCHANNEL(ref SequenceReader<byte> reader, out SkStackChannel @value) {}
    public static bool ExpectCharArray(ref SequenceReader<byte> reader, [NotNullWhen(true)] out string? @value) {}
    public static bool ExpectCharArray(ref SequenceReader<byte> reader, out ReadOnlyMemory<byte> @value) {}
    public static bool ExpectDecimalNumber(ref SequenceReader<byte> reader, int length, out uint @value) {}
    public static bool ExpectDecimalNumber(ref SequenceReader<byte> reader, out uint @value) {}
    public static bool ExpectEndOfLine(ref SequenceReader<byte> reader) {}
    public static bool ExpectIPADDR(ref SequenceReader<byte> reader, [NotNullWhen(true)] out IPAddress? @value) {}
    public static bool ExpectSequence(ref SequenceReader<byte> reader, ReadOnlySpan<byte> expectedSequence) {}
    public static bool ExpectToken(ref SequenceReader<byte> reader, ReadOnlySpan<byte> expectedToken) {}
    public static bool ExpectUINT16(ref SequenceReader<byte> reader, out ushort @value) {}
    public static bool ExpectUINT32(ref SequenceReader<byte> reader, out uint @value) {}
    public static bool ExpectUINT64(ref SequenceReader<byte> reader, out ulong @value) {}
    public static bool ExpectUINT8(ref SequenceReader<byte> reader, out byte @value) {}
    public static void ToByteSequence(ReadOnlySequence<byte> hexTextSequence, int byteSequenceLength, Span<byte> destination) {}
    public static bool TryExpectStatusLine(ref SequenceReader<byte> reader, out SkStackResponseStatus status) {}
    public static OperationStatus TryExpectToken(ref SequenceReader<byte> reader, ReadOnlySpan<byte> expectedToken) {}
  }

  public class SkStackUnexpectedResponseException : SkStackResponseException {
    public string? CausedText { get; }
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.4.1.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.3.1.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
