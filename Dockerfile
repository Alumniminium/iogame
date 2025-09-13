# Multi-stage build for IO Game with nginx frontend and .NET backend

# Stage 1: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy and restore server project
COPY server/server.csproj ./server/
RUN dotnet restore server/server.csproj

# Copy source code
COPY server/ ./server/

# Build and publish
RUN dotnet publish server/server.csproj -c Release -o /app/publish

# Stage 2: Setup nginx with .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Install nginx
RUN apk add --no-cache nginx

# Copy published .NET application
COPY --from=build /app/publish ./

# Copy web client files to nginx document root
COPY WebClient/ /usr/share/nginx/html/

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Create nginx directories
RUN mkdir -p /var/log/nginx /var/cache/nginx /tmp/nginx

# Expose port 80
EXPOSE 80

# Start both nginx and .NET application
CMD nginx && dotnet server.dll