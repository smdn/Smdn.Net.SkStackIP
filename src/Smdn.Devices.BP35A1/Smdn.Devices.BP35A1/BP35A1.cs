// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP;

namespace Smdn.Devices.BP35A1 {
  public class BP35A1 : IDisposable {
    public static Task<BP35A1> CreateAsync(
      string serialPortName,
      int baudRate = SkStackClient.DefaultBaudRate,
      IServiceProvider serviceProvider = null
    )
      => CreateAsync(SkStackClient.Create(serialPortName, baudRate, serviceProvider));

    public static Task<BP35A1> CreateAsync(
      Stream stream,
      IServiceProvider serviceProvider = null
    )
      => CreateAsync(SkStackClient.Create(stream, serviceProvider));

    public static Task<BP35A1> CreateAsync(
      SkStackClient client,
      IServiceProvider serviceProvider = null
    )
    {
      throw new NotImplementedException();
#if false
      return new BP35A1(client, serviceProvider) {
        SkStackVersion = null,
        SkStackAppVersion = null,
        LinkLocalAddress = null,
        MacAddress = null,
        Channel = null,
        PanID = null,
      };
#endif
    }

    private SkStackClient client;
    internal SkStackClient Client => client ?? throw new ObjectDisposedException(GetType().FullName);

    public string SkStackVersion { get; init; }
    public string SkStackAppVersion { get; init; }
    public IPAddress LinkLocalAddress { get; init; }
    public PhysicalAddress MacAddress { get; init; }
    public int Channel { get; init; }
    public int PanID { get; init; }

    private BP35A1(
      SkStackClient client,
      IServiceProvider serviceProvider
    )
    {
      this.client = client ?? throw new ArgumentNullException(nameof(client));
      // TODO: logger
    }

    private void ThrowIfDisposed()
    {
      if (client is null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
      client?.Close();
      client = null;
    }
  }
}
