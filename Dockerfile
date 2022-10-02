# herstfortress/atlas:latest

FROM mcr.microsoft.com/dotnet/nightly/sdk:7.0-alpine as build
WORKDIR /app
COPY /server .
COPY /Shared /Shared
RUN dotnet restore /app/server.csproj
RUN dotnet publish -c Release -o /app/published-app /app/server.csproj

FROM mcr.microsoft.com/dotnet/nightly/aspnet:7.0-alpine as runtime
WORKDIR /app

COPY --from=build /app/published-app /app
COPY BaseResources.json /app/

ENTRYPOINT [ "dotnet", "/app/server.dll" ]
