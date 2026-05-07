# TacticalAPI

The TacticalAPI is a [gRPC](https://grpc.io/) interface from Rheinmetall for accessing situational awareness systems. It is used in security, emergency services, and defense to track, visualize, and analyze data for immediate action.

This repository provides the service definitions used for exchanging tactical data across different platforms, languages, and environments.

## Repository stucture

```bash
📂 tacticalapi
├── 📁 rheinmetall/...  # .proto definitions (the contract)
├── 📁 testclient       # test and reference implementations
│   ├── 📂 csharp       # test client for the TacticalAPI in C#
├── 📄 LICENSE          # repository licensing terms
├── 📄 README.md        # introduction and short information about the repository
└── 📃 SECURITY.md      # security guidance
```

## Getting started

Because this is a gRPC-based interface, you have the flexibility to use the tools and languages that best fit your environment.
You can finde more information about gRPC here: https://grpc.io/docs/what-is-grpc/.

## Consumption

To ensure your project stays synchronized with the latest contract updates, we recommend adding this repository as a submodule:

```bash
git submodule add https://github.com/Rheinmetall/tacticalapi.git
```

_If submodules are not supported by your workflow, you may manually copy the .proto files directly into your project's include directory._

## Code generation

Available code generators and integration for many programming languages can be found on https://grpc.io/docs/languages/.

## Example

A sample and test client in [.NET](https://dotnet.microsoft.com/en-us/) is provided [here](./testclient/csharp/README.md)

## Contributing

We welcome contributions to the TacticalAPI by reporting bugs or suggesting interface improvements via [GitHub Issues](https://github.com/Rheinmetall/tacticalapi/issues).

## Commercial Support

For professional integration services, custom feature development, or commercial support inquiries, please reach out to our team:
[opensource.rme@rheinmetall.com](mailto:opensource.rme@rheinmetall.com)

## Disclaimer

This project is provided under the Eclipse Public License 2.0 (EPL 2.0). Your use is governed solely by the terms of the LICENSE file in this repository. Where explicitly indicated in individual source files, a Secondary License may apply; the respective license notices control.

The software is provided “as is,” without warranties or representations of any kind, express or implied, including but not limited to merchantability, fitness for a particular purpose, and non infringement. To the extent permitted by law, liability is disclaimed. Mandatory statutory rights remain unaffected, including liability for intent, gross negligence, and for injury to life, body, or health.

There is no entitlement to support, maintenance, or updates. Community support may be provided on a voluntary, best effort basis via GitHub issues. Any commercial offerings or SLAs, if available, are separate and not part of this project. For commercial support inquiries, please contact: opensource.rme@rheinmetall.com.

Please report security vulnerabilities confidentially according to the SECURITY policy at opensource.rme@rheinmetall.com and do not post sensitive or personal information in public issues. For details, see SECURITY.md.

By contributing to this project, you confirm you have the rights necessary to license your contributions and you license them under the EPL 2.0. The rules in CONTRIBUTING.md apply; depending on the project, a DCO sign off or a Contributor License Agreement (CLA) may be required.

Company and product names and logos in this repository are trademarks or trade names of Rheinmetall/tacticalapi. No trademark or naming rights are granted by the license. Any use requires prior written consent.

Use, distribution, and import of this software may be subject to export control, sanctions, and other applicable laws. You are responsible for complying with all applicable requirements.
In case of any discrepancy, the LICENSE text prevails. This notice is for convenience only and does not modify the license.
