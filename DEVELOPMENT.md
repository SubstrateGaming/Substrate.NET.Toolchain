# Ajuna.SDK Development
This document covers requirements and guidelines to give hints about developing the tools you find here. Please make sure you have read the [Contributing](./CONTRIBUTING.md) guidelines since this document is more technical and documents the development environment instead of general contributing guidelines.

## .NET Framework Versions
Libraries must be compatible with `netstandard2.0` framework. Even though it is rather old, we keep support for that so that [the Unity client system](https://docs.unity3d.com/2019.1/Documentation/Manual/dotnetProfileSupport.html) can use the libraries here.

Tools and services are implemented to support `net5.0` and `net6.0`.

## NuGet Package Versions
All NuGet packages here share a standard version number that is defined in [Version.props](./Version.props).

## Developing this toolchain locally
To develop and debug toolchain changes locally, you must set up a local NuGet repository and maintain the libraries that are being compiled there. 
There are many different ways to achieve that. 

A straight-forward approach is listed below for both Windows and Mac users.
These simple batch files allow you to quickly clear existing local packages, re-build everything and install the locally generated Ajuna.DotNet.Template.

### Windows

```bat
set AJUNA_ROOT=D:\Ajuna.SDK
set LOCAL_NUGET_ROOT=D:\NuGet
set LOCAL_NUGET_CACHE=%SYSTEMDRIVE%\Users\%USERNAME%\.nuget\packages
set LOCAL_NUGET_BINARY=nuget.exe
set AJUNA_VERSION=0.1.25

cd %AJUNA_ROOT%
dotnet build -p:Version=$AJUNA_VERSION --configuration Release

cd %AJUNA_ROOT%\Tools\Ajuna.DotNet
dotnet pack -p:Version=$AJUNA_VERSION

rem Clear local NuGet cache
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet.template"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.servicelayer"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.servicelayer.model"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.aspnetcore"
rmdir /S /Q "%LOCAL_NUGET_CACHE%\ajuna.dotnet.template"

rem Clear NuGet packages from local feed
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet.template"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.servicelayer"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.servicelayer.model"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.aspnetcore"
rmdir /S /Q "%LOCAL_NUGET_ROOT%\ajuna.dotnet.template"

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

### Mac

```sh
export AJUNA_ROOT="/Documents/Development/Ajuna.SDK" 
export LOCAL_NUGET_ROOT="/Documents/Development/_LocalNugetPackages"
export LOCAL_NUGET_CACHE="/Users/{yourUserName}/.nuget/packages"
export LOCAL_NUGET_BINARY=nuget
export AJUNA_VERSION="0.1.25" # Using this variable, you override the one set in the project/solution settings

set -m

cd $AJUNA_ROOT
dotnet build -p:Version=$AJUNA_VERSION --configuration Release  

cd $AJUNA_ROOT/Tools/Ajuna.DotNet
dotnet pack -p:Version=$AJUNA_VERSION 

echo 'Clear local NuGet cache'
rm -rf $LOCAL_NUGET_CACHE/ajuna.dotnet
rm -rf $LOCAL_NUGET_CACHE/ajuna.dotnet.template
rm -rf $LOCAL_NUGET_CACHE/ajuna.servicelayer
rm -rf $LOCAL_NUGET_CACHE/ajuna.servicelayer.model
rm -rf $LOCAL_NUGET_CACHE/ajuna.aspnetcore
rm -rf $LOCAL_NUGET_CACHE/ajuna.dotnet.template

echo "Clear NuGet packages from local feed"
rm -rf $LOCAL_NUGET_ROOT/ajuna.dotnet
rm -rf $LOCAL_NUGET_ROOT%/ajuna.dotnet.template
rm -rf $LOCAL_NUGET_ROOT/ajuna.servicelayer
rm -rf $LOCAL_NUGET_ROOT/ajuna.servicelayer.model
rm -rf $LOCAL_NUGET_ROOT/ajuna.aspnetcore
rm -rf $LOCAL_NUGET_ROOT/ajuna.dotnet.template

echo "Add Ajuna.DotNet, Ajuna.DotNet.Template to NuGet"
$LOCAL_NUGET_BINARY add $AJUNA_ROOT/Tools/Ajuna.DotNet/nupkg/Ajuna.DotNet.$AJUNA_VERSION.nupkg -Source $LOCAL_NUGET_ROOT
$LOCAL_NUGET_BINARY add $AJUNA_ROOT/Tools/Ajuna.DotNet.Template/bin/Release/Ajuna.DotNet.Template.$AJUNA_VERSION.nupkg -Source $LOCAL_NUGET_ROOT

echo "Add Ajuna.ServiceLayer, Ajuna.ServiceLayer.Model to NuGet"
$LOCAL_NUGET_BINARY add $AJUNA_ROOT/Ajuna.ServiceLayer/bin/Release/Ajuna.ServiceLayer.$AJUNA_VERSION.nupkg -Source $LOCAL_NUGET_ROOT
$LOCAL_NUGET_BINARY add $AJUNA_ROOT/Ajuna.ServiceLayer.Model/bin/Release/Ajuna.ServiceLayer.Model.$AJUNA_VERSION.nupkg -Source $LOCAL_NUGET_ROOT

echo "Add Ajuna.AspNetCore to NuGet"
$LOCAL_NUGET_BINARY add $AJUNA_ROOT/Ajuna.AspNetCore/bin/Release/Ajuna.AspNetCore.$AJUNA_VERSION.nupkg -Source $LOCAL_NUGET_ROOT

echo "Install .NET new template"
dotnet new --install Ajuna.DotNet.Template
```
Once you have created the sh file, do not forget to set the permissions accordingly in order to allow it to be executed.

`chmod 755 file.sh`

Using these scripts should get you started on both platforms on how to maintain the local NuGet packages.

Having this installed, you can scaffold a new project as documented [here](./README.md).

## Debugging the code generators
Since the .NET new template provides the absolute minimum to scaffold the projects, you can debug and inspect the actual code generators. Debug Ajuna.DotNet, like any other project.

## Testing the generated artefacts

First set the environment variable AJUNA_USE_MOCKUP_PROVIDER
```bat
set AJUNA_USE_MOCKUP_PROVIDER=true
```
Check if set with this command which should return true
```bat
echo %AJUNA_USE_MOCKUP_PROVIDER%
```
If it is the first time, you will be asked for certificate, when running the rest client
```bat
tbd
```
Afterwards you can execute the rest service in 
```bat
dotnet run --framework net6.0
```
Execute tests

Please report any test failures, ... as issue!
