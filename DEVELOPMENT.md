# Ajuna.SDK Development
This document covers requirements and guidelines to give some hints about developing the tools you find here. Please make sure you have read the [Contributing](./CONTRIBUTING.md) guidelines since this document is more technical and documents the development environment instead of general contributing guidelines.

## .NET Framework Versions
Libraries must be compatible with `netstandard2.0` framework. Even though it is rather old, we keep support for that so that [Unity client system](https://docs.unity3d.com/2019.1/Documentation/Manual/dotnetProfileSupport.html) can use the libraries here.

Tools and services are currently implemented to support `net5.0` and `net6.0`.

## NuGet Package Versions
All NuGet packages here are sharing a common version number that is defined in [Version.props](./Version.props).

## Developing this toolchain locally
To develop and debug toolchain changes locally you must setup a local NuGet repository and maintain the libraries that are being compiled there. There are many different ways to achieve that. A very simple approach is listed below which gives a simple batch file for Windows to easily clear existing local packages and re-build everything.

```bat
set AJUNA_ROOT=D:\Ajuna.SDK
set LOCAL_NUGET_ROOT=D:\NuGet
set LOCAL_NUGET_CACHE=%SYSTEMDRIVE%\Users\%USERNAME%\.nuget\packages
set LOCAL_NUGET_BINARY=nuget.exe
set AJUNA_VERSION=0.1.6

cd %AJUNA_ROOT%
dotnet build --configuration Release

cd %AJUNA_ROOT%\Tools\Ajuna.DotNet
dotnet pack

rem Clear local NuGet cache
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet.template"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet.servicelayer"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet.servicelayer.model"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet.aspnetcore"

rem Clear NuGet packages from local feed
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet.template"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet.servicelayer"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet.servicelayer.model"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet.aspnetcore"

rem Add Ajuna.DotNet, Ajuna.DotNet.Template to NuGet
%LOCAL_NUGET_BINARY% add %AJUNA_ROOT%\Tools\Ajuna.DotNet\nupkg\Ajuna.DotNet.%AJUNA_VERSION%.nupkg -Source %LOCAL_NUGET_ROOT%
%LOCAL_NUGET_BINARY% add %AJUNA_ROOT%\Tools\Ajuna.DotNet.Template\bin\Release\Ajuna.DotNet.Template.%AJUNA_VERSION%.nupkg -Source %LOCAL_NUGET_ROOT%

rem Add Ajuna.ServiceLayer, Ajuna.ServiceLayer.Model to NuGet
%LOCAL_NUGET_BINARY% add %AJUNA_ROOT%\Ajuna.ServiceLayer\bin\Release\Ajuna.ServiceLayer.%AJUNA_VERSION%.nupkg -Source %LOCAL_NUGET_ROOT%
%LOCAL_NUGET_BINARY% add %AJUNA_ROOT%\Ajuna.ServiceLayer.Model\bin\Release\Ajuna.ServiceLayer.Model.%AJUNA_VERSION%.nupkg -Source %LOCAL_NUGET_ROOT%

rem Add Ajuna.AspNetCore to NuGet
%LOCAL_NUGET_BINARY% add %AJUNA_ROOT%\Ajuna.AspNetCore\bin\Release\Ajuna.AspNetCore.%AJUNA_VERSION%.nupkg -Source %LOCAL_NUGET_ROOT%

rem Install .NET new template
dotnet new --install Ajuna.DotNet.Template
```

Using that script should get you started on Windows and give insights on other platforms how to maintain the local NuGet packages.

Having this installed you can scaffold a new project a documented [here](./README.md).

## Debugging the code generators
Since the .NET new template just provides the absolute minimum to scaffold the projects you can debug and inspect the actual code generators. Simply debug `Ajuna.DotNet` as any other project having the command line arguments `update` or `upgrade`.