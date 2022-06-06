# Ajuna.DotNet.Template
![ajuna-Header-1080p_with_logo](https://user-images.githubusercontent.com/17710198/136852531-d9eb47cd-efcd-4c88-bdbf-78dfcbffe287.png)

# What is Ajuna.DotNet.Template ?
[![license](https://img.shields.io/github/license/ajuna-network/Ajuna.SDK)](https://github.com/ajuna-network/Ajuna.SDK/blob/origin/LICENSE)

Ajuna.DotNet.Template implements the `dotnet new ajuna` template.

## Getting Started
Install our .NET new template with:

```sh
dotnet new --install Ajuna.DotNet.Template
```

which makes `dotnet new ajuna` available.

## Usage
Please refer to the root [README.md](../../README.md) for general usage guides.

## Command Line Options
This template comes with some arguments to specify template parameters. 

To list all available template parameters simply run the command `dotnet new ajuna --help` which prints all the available parameters.

| Parameter | Description |
|---|---|
| --sdk_version | This parameter specifies the used Ajuna SDK version. Defaults to the latest version. |
| --metadata_websocket | This parameter tells the generator where to pull the substrate metadata from. Default is localhost `ws://127.0.0.1:9944`. |
| --metadata_runtime | This parameter tells the generator which runtime to use to generate classes for. Default is `ajuna_solo_runtime`. |
| -net_api | This parameters controls how to name the generated type extension library. Default is `Ajuna.NetApiExt`. |
| -rest_service | This parameters controls how to name the generated rest service. Default is `Ajuna.RestService`. |
| -rest_client | This parameters controls how to name the generated rest client, tests and mockup project. Default is `Ajuna.RestClient`. |
