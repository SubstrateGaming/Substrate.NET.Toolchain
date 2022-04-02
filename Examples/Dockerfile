#
# Ajuna Network
# Rest service layer testing image
#
# Building
# docker build -t aspnet:ajuna .
#
# Running
# docker run --rm -d -p 80:80 --env AJUNA_WEBSOCKET_ENDPOINT="ws://X.X.X.X:9944" aspnet:ajuna
# 
# where X.X.X.X is the IP of the hosted substrate ajuna Docker container.
# Most likely this is the IP address of your local development machine.
#

FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /src
COPY . .
RUN dotnet restore "Ajuna.RestService/Ajuna.RestService.csproj"
RUN dotnet build "Ajuna.RestService/Ajuna.RestService.csproj" -c Release -o /app/build
RUN dotnet publish "Ajuna.RestService/Ajuna.RestService.csproj" -c Release -o /app/publish
COPY "Ajuna.RestService/Ajuna.RestService.xml" /app/publish

#
# Ajuna.RestService runtime
#
# Description
# This is the runtime environment. Copy from build environment and kick-off the service.
#
FROM mcr.microsoft.com/dotnet/aspnet:5.0 as final
LABEL maintainer="hello@ajuna.io"
LABEL version="0.1"
LABEL description="This image is for development purposes. Follow the instruction in Dockerfile to build and start this image."

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT [ "dotnet", "Ajuna.RestService.dll" ]
