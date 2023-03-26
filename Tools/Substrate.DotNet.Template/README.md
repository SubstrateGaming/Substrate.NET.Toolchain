# Substrate.DotNet.Template
![ajuna-Header-1080p_with_logo](https://user-images.githubusercontent.com/17710198/136852531-d9eb47cd-efcd-4c88-bdbf-78dfcbffe287.png)

# What is Substrate.DotNet.Template ?
[![license](https://img.shields.io/github/license/SubstrateGaming/Substrate.NET.Toolchain)](https://github.com/SubstrateGaming/Substrate.NET.Toolchain/blob/origin/LICENSE)

Substrate.DotNet.Template implements the `dotnet new substrate` template.

## Getting Started
Install our .NET new template with:

```sh
dotnet new --install Substrate.DotNet.Template
```

which makes `dotnet new substrate` available.

## Usage
Please refer to the root [README.md](../../README.md) for general usage guides.

## Command Line Options
This template comes with some arguments to specify template parameters. 

To list all available template parameters, simply run the command `dotnet new substrate --help`, which prints all the available parameters.

| Parameter                       | Description                                                                                                               |
|----------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| --sdk_version                    | This parameter specifies the used Substrate .NET Toolchain version. Defaults to the latest version.                      |
| --api_version                    | This parameter specifies the used Substrate .NET API version. Defaults to the latest version.                            |
| --metadata_websocket             | This parameter tells the generator where to pull the substrate metadata from. Default is localhost `ws://127.0.0.1:9944`.|
| --net_api                        | This parameter controls the generated type extension librarys name. Default is `Substrate.NetApiExt`.                    |
| --rest_service                   | This parameter controls the generated rest service name. Default is `Substrate.RestService`.                             |
| --rest_client                    | This parameter controls generated rest client name, tests and mockup project. Default is `Substrate.RestClient`.         |
| --generate_openapi_documentation | This parameter Activates or deactivates the OpenApi Document Generation in the generated rest service. Default is `true`.|


