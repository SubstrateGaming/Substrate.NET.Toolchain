# Ajuna.SDK
![ajuna-Header-1080p_with_logo](https://user-images.githubusercontent.com/17710198/136852531-d9eb47cd-efcd-4c88-bdbf-78dfcbffe287.png)

# What is Ajuna.SDK ?
[![license](https://img.shields.io/github/license/ajuna-network/Ajuna.SDK)](./LICENSE)
[![contributors](https://img.shields.io/github/contributors/ajuna-network/Ajuna.SDK)](https://github.com/ajuna-network/Ajuna.SDK/graphs/contributors) 
[![GitHub issues](https://img.shields.io/github/issues/ajuna-network/Ajuna.SDK.svg)](https://github.com/ajuna-network/Ajuna.SDK/issues)

Ajuna SDK is a .NET toolchain featuring .NET framework extensions and code generation utilities to build substrate storage services and clients quickly. This toolchain ideally extends [Ajuna.NetApi](https://github.com/ajuna-network/Ajuna.NetApi) library, which provides raw access to substrate nodes.

The project behind Ajuna.SDK is [Ajuna Network](https://ajuna.io/) and the flagship game [DOTMog](https://dotmog.com/), backed by the Swiss company [BloGa Tech AG](admin@bloga.tech).

## Important
This toolchain is under heavy development, and things may change quickly. Please use it with caution until there is a stable release available.

## Projects
Below is a high-level technical overview of the libraries and tools available in Ajuna.SDK.

| Project | Description                                                                                                                                                                                                                                                                               | NuGet 
|---|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---|
| Ajuna.ServiceLayer | Implements the fundamental layer to access substrate node storage changes with a convenient API.                                                                                                                                                                                          | [![Nuget](https://img.shields.io/nuget/v/Ajuna.ServiceLayer)](https://www.nuget.org/packages/Ajuna.ServiceLayer/) |
| Ajuna.ServiceLayer.Model | Implements standard classes to easily share types between services and clients.                                                                                                                                                                                                           | [![Nuget](https://img.shields.io/nuget/v/Ajuna.ServiceLayer.Model)](https://www.nuget.org/packages/Ajuna.ServiceLayer.Model/) |
| Ajuna.AspNetCore | Implements extensions to the service layer that allow for quickly building a RESTful service to access your substrate node storage.                                                                                                                                                       | [![Nuget](https://img.shields.io/nuget/v/Ajuna.AspNetCore)](https://www.nuget.org/packages/Ajuna.AspNetCore/) |
| Ajuna.DotNet, Ajuna.DotNet.Template | .NET developer toolchain to scaffold actual projects such as a RESTful service including all the storage classes, types, and consumer clients. The projects generated with the generator toolchain are intended to be used for scaffolding and starting a substrate node service quickly. | [![Nuget](https://img.shields.io/nuget/v/Ajuna.DotNet)](https://www.nuget.org/packages/Ajuna.DotNet/) [![Nuget](https://img.shields.io/nuget/v/Ajuna.DotNet.Template)](https://www.nuget.org/packages/Ajuna.DotNet.Template/)|

## Architecture

![Ajuna Network Architecture - Lightpaper](https://user-images.githubusercontent.com/17710198/177153394-b9fc1e08-a61b-47c2-9797-482accb201e1.png)

## Getting Started

Assuming your [substrate node is running locally](https://github.com/ajuna-network/Ajuna), you're ready to build your services and clients using the Ajuna SDK toolchain.

---

### Installing the template

---

Install our .NET template with the following command:

```sh
dotnet new --install Ajuna.DotNet.Template
```

which makes `dotnet new ajuna` available.

### Scaffolding a project

---

Using a terminal of your choice, create a new directory for your project and execute the following command in that directory:

```sh
dotnet new sln
dotnet new ajuna \
   --sdk_version 0.1.10 \
   --rest_service AjunaExample.RestService \
   --net_api AjunaExample.NetApi \
   --rest_client AjunaExample.RestClient \
   --metadata_websocket ws://127.0.0.1:9944 \
   --metadata_runtime ajuna_solo_runtime \
   --force \
   --allow-scripts yes
```

which generates a new solution and a couple of .NET projects in your project directory. 
(A description for all command parameters can be found [here](Tools/Ajuna.DotNet.Template/README.md))
    

```txt
.
├─── .ajuna
├─── .config
├─── AjunaExample.NetApi
├─── AjunaExample.RestClient
├─── AjunaExample.RestClient.Mockup
├─── AjunaExample.RestClient.Test
├─── AjunaExample.RestService
```

This contains everything you need in order to get started making excellent substrate services and clients in .NET.


## Documents

- [Contributing](./CONTRIBUTING.md)
- [Development](./DEVELOPMENT.md)
- `dotnet ajuna` toolchain with [Ajuna.DotNet](/Tools/Ajuna.DotNet/README.md)
- `dotnet new ajuna` template with [Ajuna.DotNet.Template](/Tools/Ajuna.DotNet.Template/README.md)


## Community

[![Discord Banner 1](https://discordapp.com/api/guilds/849331368558198803/widget.png?style=banner2)](https://discord.gg/cE72GYcFgY)

[![Discord Banner 2](https://discordapp.com/api/guilds/447132563924844544/widget.png?style=banner2)](https://discord.gg/GXYmNWtPzQ)

