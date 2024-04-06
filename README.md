[![GitHub license](https://img.shields.io/github/license/smdn/Smdn.Net.SkStackIP)](https://github.com/smdn/Smdn.Net.SkStackIP/blob/main/LICENSE.txt)
[![tests/main](https://img.shields.io/github/actions/workflow/status/smdn/Smdn.Net.SkStackIP/test.yml?branch=main&label=tests%2Fmain)](https://github.com/smdn/Smdn.Net.SkStackIP/actions/workflows/test.yml)
[![CodeQL](https://github.com/smdn/Smdn.Net.SkStackIP/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/smdn/Smdn.Net.SkStackIP/actions/workflows/codeql-analysis.yml)

This repository provides a .NET client library for [Skyley Networks](https://www.skyley.com/)' [SKSTACK IP](https://www.skyley.com/wiki/?SKSTACK+IP+for+HAN).

> [!IMPORTANT]
> This project and .NET implementation published in this project are not affiliated with Skyley Networks. [An official Java implementation](https://github.com/SkyleyNetworks/SKSTACK_API) is available from Skyley Networks' repository.



# Smdn.Net.SkStackIP
[![NuGet](https://img.shields.io/nuget/v/Smdn.Net.SkStackIP.svg)](https://www.nuget.org/packages/Smdn.Net.SkStackIP/)

[Smdn.Net.SkStackIP](./src/Smdn.Net.SkStackIP) is a .NET library that provides APIs for operating devices that implement Skyley Networks' SKSTACK IP.

This library supports to use any `Stream` or `PipeReader`/`PipeWriter` as the communication channel for the SKSTACK IP protocol, so it has the ability to communicate with devices that use other than serial ports, e.g., pseudo devices.



# Smdn.Devices.BP35XX
[![NuGet](https://img.shields.io/nuget/v/Smdn.Devices.BP35XX.svg)](https://www.nuget.org/packages/Smdn.Devices.BP35XX/)

[Smdn.Devices.BP35XX](./src/Smdn.Devices.BP35XX) is a .NET library for operating [ROHM BP35A1](https://www.rohm.co.jp/products/wireless-communication/specified-low-power-radio-modules/bp35a1-product) and other [ROHM Wi-SUN modules](https://www.rohm.co.jp/products/wireless-communication/specified-low-power-radio-modules) using the SKSTACK IP.

> [!NOTE]
> Currently this library only support the BP35A1, but I would like to add support for other devices. If you would like to add other devices, please contact me.



# Project status
The library implementation and API is almost stable.

Documentation and examples of use are incomplete. If you have any questions, please ask through [Discussions](https://github.com/smdn/Smdn.Net.SkStackIP/discussions) or [Issues](https://github.com/smdn/Smdn.Net.SkStackIP/issues/).

# For contributers
Contributions are appreciated!

If there's a feature you would like to add or a bug you would like to fix, please read [Contribution guidelines](./CONTRIBUTING.md) and create an Issue or Pull Request.

IssueやPull Requestを送る際は、[Contribution guidelines](./CONTRIBUTING.md)をご覧頂ください。　可能なら英語が望ましいですが、日本語で構いません。

# Related project
[smdn/Smdn.Net.EchonetLite](https://github.com/smdn/Smdn.Net.EchonetLite): This project is to implement [ECHONET Lite](https://echonet.jp/english/) and its related standards/specifications with .NET, also providing an implementation for **Route B** using SKSTACK IP. The major goal is to implement the functionalities to access smart energy meters via the Route B.


# Notice
<!-- #pragma section-start NupkgReadmeFile_Notice -->
## License
This project is licensed under the terms of the [MIT License](./LICENSE.txt).

## Disclaimer
(An English translation for the reference follows the text written in Japanese.)

本プロジェクトは、Skyley Networks、およびSKSTACK IPを搭載する製品の製造元・供給元・販売元とは無関係の、非公式なものです。

This is an unofficial project that has no affiliation with Skyley Networks and the manufacturers/vendors/suppliers of the products that equipped with SKSTACK IP.

## Third-Party Notices
See [ThirdPartyNotices.md](./ThirdPartyNotices.md) for detail.
<!-- #pragma section-end NupkgReadmeFile_Notice -->
