# Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

RUN git clone https://github.com/mehyaa/arduino-ota-server.git /src

WORKDIR /src

RUN dotnet publish ./src/Mehyaa.Arduino.OTAServer.Web/Mehyaa.Arduino.OTAServer.Web.csproj --output /app --configuration Release --runtime linux-x64 --self-contained false --verbosity minimal

# Image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

LABEL maintainer="mehyaa@gmail.com"

WORKDIR /app

COPY --from=build /app .

USER root

ENTRYPOINT ["dotnet", "Mehyaa.Arduino.OTAServer.Web.dll"]

CMD []
