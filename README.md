# Ajuna.SDK
![ajuna-Header-1080p_with_logo](https://user-images.githubusercontent.com/17710198/136852531-d9eb47cd-efcd-4c88-bdbf-78dfcbffe287.png)

# What is Ajuna.SDK ?
[![license](https://img.shields.io/github/license/ajuna-network/Ajuna.SDK)](https://github.com/ajuna-network/Ajuna.SDK/blob/origin/LICENSE)
[![contributors](https://img.shields.io/github/contributors/ajuna-network/Ajuna.SDK)](https://github.com/ajuna-network/Ajuna.SDK/graphs/contributors) 
[![GitHub issues](https://img.shields.io/github/issues/ajuna-network/Ajuna.SDK.svg)](https://github.com/ajuna-network/Ajuna.SDK/issues)

Ajuna SDK is a .NET toolchain featuring ASP .NET Core framework extensions and code generator utilities to easily build substrate storage services and clients. The toolings here perfectly extend [Ajuna.NetApi](https://github.com/ajuna-network/Ajuna.NetApi) library which provides raw access to substrate nodes.

The project behind Ajuna.SDK is [Ajuna Network](https://ajuna.io/) and the flagship game [DOTMog](https://dotmog.com/), backed by the swiss company [BloGa Tech AG](admin@bloga.tech).

## Important
This toolchain is under heavy development and things may change quickly. Use with caution until there is a stable release available.

## Projects
Below is a high level technical overview about libraries and tools avaiiable in Ajuna.SDK.

| Project | Description | NuGet 
|---|---|---|
| Ajuna.ServiceLayer | Implements the fundamental layer to access substrate node storage changes with a convenient API. | [![Nuget](https://img.shields.io/nuget/v/Ajuna.ServiceLayer)](https://www.nuget.org/packages/Ajuna.ServiceLayer/) |
| Ajuna.ServiceLayer.Model | Implements common classes to easily share classes between services and clients. | [![Nuget](https://img.shields.io/nuget/v/Ajuna.ServiceLayer.Model)](https://www.nuget.org/packages/Ajuna.ServiceLayer.Model/) |
| Ajuna.AspNetCore | Extends the service layer and implements .ASP Net Core extensions to easily build a RESTful service to access you substrate node storages. | [![Nuget](https://img.shields.io/nuget/v/Ajuna.AspNetCore)](https://www.nuget.org/packages/Ajuna.AspNetCore/) |
| Ajuna.NetApiGenerator, Ajuna.RestService.ClientGenerator | .NET developer toolchain to scaffold actual projects such as a RESTful service including all the storage classes, types and consumer clients. The projects generated with the generator toolchain are intended to be used to easily scaffold and start a substrate node storage node services. |

---

## Community
[![Discord Banner 1](https://discordapp.com/api/guilds/849331368558198803/widget.png?style=banner2)](https://discord.gg/cE72GYcFgY)

[![Discord Banner 2](https://discordapp.com/api/guilds/447132563924844544/widget.png?style=banner2)](https://discord.gg/GXYmNWtPzQ)

