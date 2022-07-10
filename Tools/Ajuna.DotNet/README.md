# Ajuna.DotNet
![ajuna-Header-1080p_with_logo](https://user-images.githubusercontent.com/17710198/136852531-d9eb47cd-efcd-4c88-bdbf-78dfcbffe287.png)

# What is Ajuna.DotNet ?
[![license](https://img.shields.io/github/license/ajuna-network/Ajuna.SDK)](https://github.com/ajuna-network/Ajuna.SDK/blob/origin/LICENSE)

Ajuna.DotNet implements a command line utility that can scaffold substrate storage, services and clients.

## Getting Started
Ajuna.DotNet is automatically installed and configured when scaffolding a new SDK project with `dotnet new ajuna`, but can also be used independently.

## Usage

- The command `dotnet ajuna upgrade` downloads the metadata from your substrate node (storing the downloaded metadata in `.ajuna/metadata.txt`) and starts generating the respective classes. Use it when the tool is executed for the first time or after the substrate node has been updated.


- The command `dotnet ajuna update` generates new or updates existing classes using an already existing `.ajuna/metadata.txt` file. Use it when you already have a  `.ajuna/metadata.txt` file.


## Manual Installation
It is possible to manually install the toolchain in your existing projects if it was not already installed by the `dotnet new ajuna` template.

This can be done by executing the `dotnet tool install Ajuna.DotNet` command in your working tree.

### Requirements

This toolchain requires having an `.ajuna` directory in your working tree and its corresponding `.ajuna/ajuna-config.json`. The configuration file contains the necessary information for the tool to run.

In case your project has been created with `dotnet new ajuna`; the configuration file is automatically created. Otherwise, you would need to manually create such a configuration file.


```json
// .ajuna/ajuna-config.json
{
   "projects": {
      "net_api": "AjunaExample.NetApi",
      "rest_service": "AjunaExample.RestService",
      "rest_client": "AjunaExample.RestClient"
   },
   "metadata": {
      "websocket": "ws://127.0.0.1:9944",
   },
   "rest_client_settings": {
      "service_assembly": "AjunaExample.RestService.dll"
   }
}
```

As you can see in the default configuration file above, it includes information regarding project names, root namespaces of the generated projects as well as the node url and the runtime name.

## What files are being generated?

Running `dotnet ajuna upgrade` here will perform the following changes in your working directory:

- Generate `.ajuna/metadata.txt` (from configured websocket endpoint)
- Generate `AjunaExample.NetApi/Generated/*.cs` classes.
- Generate `AjunaExample.RestService/Generated/Controller/*.cs` classes.
- Generate `AjunaExample.RestService/Generated/Storage/*.cs` classes.
- Generate `AjunaExample.RestClient/Generated/Clients/*.cs` classes.
- Generate `AjunaExample.RestClient/Generated/Interfaces/*.cs` classes.
- Generate `AjunaExample.RestClient.Test/Generated/*.cs` classes.
- Generate `AjunaExample.RestClient.Mockup/Clients/*.cs` classes.
- Generate `AjunaExample.RestClient.Mockup/Interfaces/*.cs` classes.

Using `dotnet ajuna update` skips the first task (downloading metadata) and only generates the respective code.