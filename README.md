# Generic Substrate SDK

# What is Ajuna.SDK ?
[![license](https://img.shields.io/github/license/ajuna-network/Ajuna.SDK)](./LICENSE)
[![contributors](https://img.shields.io/github/contributors/ajuna-network/Ajuna.SDK)](https://github.com/ajuna-network/Ajuna.SDK/graphs/contributors) 
[![GitHub issues](https://img.shields.io/github/issues/ajuna-network/Ajuna.SDK.svg)](https://github.com/ajuna-network/Ajuna.SDK/issues)

Ajuna SDK is a .NET toolchain featuring .NET framework extensions and code generation utilities to build substrate storage services and clients quickly. This toolchain ideally extends [Ajuna.NetApi](https://github.com/ajuna-network/Ajuna.NetApi) library, which provides raw access to substrate nodes.

![image](https://user-images.githubusercontent.com/17710198/221981597-de89c308-8f33-4c08-a463-3270e68a5035.png)

## Important
This toolchain is under development, and things may change quickly.

## Projects
Below is a high-level technical overview of the libraries and tools available in Ajuna.SDK.

| Project | Description                                                                                                                                                                                                                                                                               | NuGet 
|---|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---|
| Ajuna.ServiceLayer | Implements the fundamental layer to access substrate node storage changes with a convenient API.                                                                                                                                                                                          | [![Nuget](https://img.shields.io/nuget/v/Ajuna.ServiceLayer)](https://www.nuget.org/packages/Ajuna.ServiceLayer/) |
| Ajuna.ServiceLayer.Model | Implements standard classes to easily share types between services and clients.                                                                                                                                                                                                           | [![Nuget](https://img.shields.io/nuget/v/Ajuna.ServiceLayer.Model)](https://www.nuget.org/packages/Ajuna.ServiceLayer.Model/) |
| Ajuna.AspNetCore | Implements extensions to the service layer that allow for quickly building a RESTful service to access your substrate node storage.                                                                                                                                                       | [![Nuget](https://img.shields.io/nuget/v/Ajuna.AspNetCore)](https://www.nuget.org/packages/Ajuna.AspNetCore/) |
| Ajuna.DotNet, Ajuna.DotNet.Template | .NET developer toolchain to scaffold actual projects such as a RESTful service including all the storage classes, types, and consumer clients. The projects generated with the generator toolchain are intended to be used for scaffolding and starting a substrate node service quickly. | [![Nuget](https://img.shields.io/nuget/v/Ajuna.DotNet)](https://www.nuget.org/packages/Ajuna.DotNet/) [![Nuget](https://img.shields.io/nuget/v/Ajuna.DotNet.Template)](https://www.nuget.org/packages/Ajuna.DotNet.Template/)|

## Architecture

![image](https://user-images.githubusercontent.com/17710198/221981432-49c4c811-57a6-4ac9-abf0-936de42dd33e.png)

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
   --sdk_version 0.2.3 \
   --rest_service AjunaExample.RestService \
   --net_api AjunaExample.NetApiExt \
   --rest_client AjunaExample.RestClient \
   --metadata_websocket ws://127.0.0.1:9944 \
   --force \
   --allow-scripts yes
```

which generates a new solution and a couple of .NET projects in your project directory. 
(A description for all command parameters can be found [here](Tools/Ajuna.DotNet.Template/README.md))
    

```txt
.
├─── .ajuna
├─── .config
├─── AjunaExample.NetApiExt
├─── AjunaExample.RestClient
├─── AjunaExample.RestClient.Mockup
├─── AjunaExample.RestClient.Test
├─── AjunaExample.RestService
```

### Role of the Generated Projects

Before elaborating on each of the generated projects, let’s first talk about [Ajuna.NetApi](https://github.com/ajuna-network/Ajuna.NetApi/tree/master/Ajuna.NetApi) which is the basis that these projects are built upon.

#### Ajuna.NetApi

`Ajuna.NetApi` is the basic framework for accessing and handling JSON-RPC connections and handling all standard RPC calls exposed by the `rpc.methods()` of every substrate node. It additionally implements Rust primitives and Generics as a C# representation like [U8](https://github.com/ajuna-network/Ajuna.NetApi/blob/master/Ajuna.NetApi/Model/Types/Primitive/U8.cs), [BaseVec](https://github.com/ajuna-network/Ajuna.NetApi/blob/master/Ajuna.NetApi/Model/Types/Base/BaseVec.cs) (Vec<>), or [EnumExt](https://github.com/ajuna-network/Ajuna.NetApi/blob/master/Ajuna.NetApi/Model/Types/Base/BaseEnumExt.cs) (Rust-specific Enums). 


#### Ajuna.NetApiExt

Since `Ajuna.NetApi` has no other types than the ones previously described, accessing a node’s storage or sending extrinsic would involve manually creating the necessary types. This is where the generated `Ajuna.NetApiExt` comes into play since it extends `Ajuna.NetApi` by exposing all the node-specific types, storage access, extrinsic calls and more. 


#### Ajuna.RestService

This service:

 - Connects to a node and subscribes to the global storage changes, which are then maintained in memory.
 - Offers a REST service (poll) which exposes all the storage information as REST.
 - Offers a subscription service (pub/sub) providing changes over a WebSocket. 

The benefit of this approach is that this artifact is much more lightweight than the node itself and can therefore be scaled according to the needs of the consumers without putting any load on an RPC node except for one connection (per RestService instance) for the global storage subscription.


#### Ajuna.RestClient

This RestClient can be used in a C#, Unity, or any other application allowing it to access the information provided by the previously described RestService. Using the RestClient one could subscribe to the node storage changes using the WebSocket or access the storage directly through exposed REST service.

As you can see, we could in principle launch any service or create any application on top of Substrate without any further knowledge except from the library usage.

The generated projects contain everything you need in order to get started making excellent substrate services and clients in C# and the .NET framework.


### Video Tutorial

You can also watch our short step-by-step tutorial that guides you through the entire process.

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/27k8vxCrXcY/0.jpg)](https://www.youtube.com/watch?v=27k8vxCrXcY)




## Documents

- [Contributing](./CONTRIBUTING.md)
- [Development](./DEVELOPMENT.md)
- [Examples](./EXAMPLES.md)
- `dotnet ajuna` toolchain with [Ajuna.DotNet](/Tools/Ajuna.DotNet/README.md)
- `dotnet new ajuna` template with [Ajuna.DotNet.Template](/Tools/Ajuna.DotNet.Template/README.md)


## Community

[![Discord Banner 1](https://discordapp.com/api/guilds/849331368558198803/widget.png?style=banner2)](https://discord.gg/cE72GYcFgY)

[![Discord Banner 2](https://discordapp.com/api/guilds/447132563924844544/widget.png?style=banner2)](https://discord.gg/GXYmNWtPzQ)

The project behind Ajuna.SDK is [Ajuna Network](https://ajuna.io/) and the flagship game [DOTMog](https://dotmog.com/), backed by the Swiss company [BloGa Tech AG](admin@bloga.tech).
