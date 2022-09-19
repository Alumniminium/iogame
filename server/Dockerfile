# herstfortress/atlas:latest

FROM mcr.microsoft.com/dotnet/nightly/sdk:7.0-alpine as build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/published-app --configuration Release


FROM mcr.microsoft.com/dotnet/nightly/aspnet:7.0-alpine as runtime
WORKDIR /app

COPY --from=build /app/published-app /app
COPY BaseResources.json /app/

ENTRYPOINT [ "dotnet", "/app/server.dll" ]
