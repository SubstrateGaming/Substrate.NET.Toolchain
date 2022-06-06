# Ajuna.DotNet
![ajuna-Header-1080p_with_logo](https://user-images.githubusercontent.com/17710198/136852531-d9eb47cd-efcd-4c88-bdbf-78dfcbffe287.png)

# What is Ajuna.DotNet ?
[![license](https://img.shields.io/github/license/ajuna-network/Ajuna.SDK)](https://github.com/ajuna-network/Ajuna.SDK/blob/origin/LICENSE)

Ajuna.DotNet implements the command line utility to scaffold build substrate storage services and clients.

## Getting Started
This tool is automatically being installed and configured when scaffolding a new SDK project with `dotnet new ajuna`.

## Usage
The command `dotnet ajuna update` updates your generated classes to the newest version. Use this if you're updating the toolchain to a newer version to re-generate all the classes.

The command `dotnet ajuna upgrade` is used if:

- a) The tool is executed the first time. It downloads the metadata from your substrate node and starts generating classes then.
- b) The substrate node is updated. Use this to re-download all the metadata and generate the classes.

## Manual Installation
It is possible to manually install the toolchain to your existing projects if it was not scaffolded with `dotnet new ajuna` template.

### Installation
Install the toolchain with `dotnet tool install Ajuna.DotNet` from your working tree.

### Requirements

This toolchain requires to have a `.ajuna` directory in your working tree and its corresponding `.ajuna/ajuna-config.json`. The configuration file tells the generator how to generate the code, e.g: project namings.

If the project was created with `dotnet new ajuna` the configuration file is being created automatically. Otherwise you would need to create such a configuration file manually.


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
      "runtime": "ajuna_solo_runtime"
   },
   "rest_client_settings": {
      "service_assembly": "AjunaExample.RestService.dll"
   }
}
```

As you can see this configuration file simply tells the toolchain how you want to name the projects and root namespaces of the generated projects.

## What files are being generated?

Running `dotnet ajuna upgrade` here will perform the following changes to your working directory:

- Generate `.ajuna/metadata.txt` (from configured websocket endpoint)
- Generate `AjunaExample.NetApi/Generated/*.cs` classes.
- Generate `AjunaExample.RestService/Generated/Controller/*.cs` classes.
- Generate `AjunaExample.RestService/Generated/Storage/*.cs` classes.
- Generate `AjunaExample.RestClient/Generated/Clients/*.cs` classes.
- Generate `AjunaExample.RestClient/Generated/Interfaces/*.cs` classes.
- Generate `AjunaExample.RestClient.Test/Generated/*.cs` classes.
- Generate `AjunaExample.RestClient.Mockup/Clients/*.cs` classes.
- Generate `AjunaExample.RestClient.Mockup/Interfaces/*.cs` classes.

Using `dotnet ajuna update` skips the first task (downloading metadata) and generates the reamining code.