[![GitHub license](https://img.shields.io/github/license/smdn/Smdn.Net.SkStackIP)](https://github.com/smdn/Smdn.Net.SkStackIP/blob/main/LICENSE.txt)
[![tests/main](https://img.shields.io/github/actions/workflow/status/smdn/Smdn.Net.SkStackIP/test.yml?branch=main&label=tests%2Fmain)](https://github.com/smdn/Smdn.Net.SkStackIP/actions/workflows/test.yml)
[![CodeQL](https://github.com/smdn/Smdn.Net.SkStackIP/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/smdn/Smdn.Net.SkStackIP/actions/workflows/codeql-analysis.yml)

This repository provides a .NET client library for [Skyley Networks](https://www.skyley.com/)' [SKSTACK IP](https://www.skyley.com/wiki/?SKSTACK+IP+for+HAN).

This project and .NET implementation published in this project are not affiliated with Skyley Networks. [An official Java implementation](https://github.com/SkyleyNetworks/SKSTACK_API) is available from Skyley Networks' repository.

# Smdn.Net.SkStackIP
[![NuGet](https://img.shields.io/nuget/v/Smdn.Net.SkStackIP.svg)](https://www.nuget.org/packages/Smdn.Net.SkStackIP/)

`Smdn.Net.SkStackIP` is a .NET library that provides APIs for operating devices that implement Skyley Networks' SKSTACK IP.

This library supports to use any `Stream` or `PipeReader`/`PipeWriter` as the communication channel for the SKSTACK IP protocol, so it has the ability to communicate with devices that use other than serial ports, e.g., pseudo devices.

# Project status
The library implementation and API is almost stable.

Documentation and examples of use are incomplete. If you have any questions, please ask through issue.

# For contributers
Contributions are appreciated!

If there's a feature you would like to add or a bug you would like to fix, please read [Contribution guidelines](./CONTRIBUTING.md) and create an Issue or Pull Request.

IssueやPull Requestを送る際は、[Contribution guidelines](./CONTRIBUTING.md)をご覧頂ください。　可能なら英語が望ましいですが、日本語で構いません。

# Notice
<!-- #pragma section-start NupkgReadmeFile_Notice -->
## License
This project is licensed under the terms of the [MIT License](./LICENSE.txt).

## Third-Party Notices
See [ThirdPartyNotices.md](./ThirdPartyNotices.md) for detail.
<!-- #pragma section-end NupkgReadmeFile_Notice -->
