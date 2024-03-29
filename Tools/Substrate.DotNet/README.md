# Substrate.DotNet
![ajuna-Header-1080p_with_logo](https://user-images.githubusercontent.com/17710198/136852531-d9eb47cd-efcd-4c88-bdbf-78dfcbffe287.png)

# What is Substrate.DotNet ?
[![license](https://img.shields.io/github/license/SubstrateGaming/Substrate.NET.Toolchain)](https://github.com/SubstrateGaming/Substrate.NET.Toolchain/blob/origin/LICENSE)

Substrate.DotNet implements a command line utility that can scaffold substrate storage, services and clients.

## Getting Started
Substrate.DotNet is automatically installed and configured when scaffolding a new SDK project with `dotnet new substrate`, but can also be used independently.

## Usage

- The command `dotnet substrate upgrade` downloads the metadata from your substrate node (storing the downloaded metadata in `.substrate/metadata.txt`) and starts generating the respective classes. Use it when the tool is executed for the first time or after the substrate node has been updated.


- The command `dotnet substrate update` generates new or updates existing classes using an already existing `.substrate/metadata.txt` file. Use it when you already have a  `.substrate/metadata.txt` file.


## Manual Installation
It is possible to manually install the toolchain in your existing projects if it was not already installed by the `dotnet new substrate` template.

This can be done by executing the `dotnet tool install Substrate.DotNet` command in your working tree.

### Requirements

This toolchain requires having an `.substrate` directory in your working tree and its corresponding `.substrate/substrate-config.json`. The configuration file contains the necessary information for the tool to run.

In case your project has been created with `dotnet new substrate`; the configuration file is automatically created. Otherwise, you would need to manually create such a configuration file.


```json
// .substrate/substrate-config.json
{
   "projects": {
      "net_api": "Substrate.NetApi",
      "rest_service": "Substrate.RestService",
      "rest_client": "Substrate.RestClient"
   },
   "metadata": {
      "websocket": "ws://127.0.0.1:9944",
   },
   "rest_client_settings": {
      "service_assembly": "Substrate.RestService.dll"
   }
}
```

As you can see in the default configuration file above, it includes information regarding project names, root namespaces of the generated projects as well as the node url and the runtime name.

## What files are being generated?

Running `dotnet substrate upgrade` here will perform the following changes in your working directory:

- Generate `.substrate/metadata.txt` (from configured websocket endpoint)
- Generate `Substrate.NetApi/Generated/*.cs` classes.
- Generate `Substrate.RestService/Generated/Controller/*.cs` classes.
- Generate `Substrate.RestService/Generated/Storage/*.cs` classes.
- Generate `Substrate.RestClient/Generated/Clients/*.cs` classes.
- Generate `Substrate.RestClient/Generated/Interfaces/*.cs` classes.
- Generate `Substrate.RestClient.Test/Generated/*.cs` classes.
- Generate `Substrate.RestClient.Mockup/Clients/*.cs` classes.
- Generate `Substrate.RestClient.Mockup/Interfaces/*.cs` classes.

Using `dotnet substrate update` skips the first task (downloading metadata) and only generates the respective code.

## Debuging

You can run the Generator, with F5 and the argument "update"
After you initially have executed the generation once.