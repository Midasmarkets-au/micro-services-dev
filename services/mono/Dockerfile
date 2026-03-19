# Build context: repo root
#   docker build -f services/mono/Dockerfile -t mono .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update && apt-get install -y --no-install-recommends protobuf-compiler && rm -rf /var/lib/apt/lists/*
WORKDIR /src

COPY ["services/mono/Directory.Build.props", "services/mono/"]
COPY ["services/mono/Bacera.Gateway.Web/Bacera.Gateway.Web.csproj", "services/mono/Bacera.Gateway.Web/"]
COPY ["services/mono/Bacera.Gateway.Infrastructure/Bacera.Gateway.Infrastructure.csproj", "services/mono/Bacera.Gateway.Infrastructure/"]
COPY ["services/mono/Bacera.Gateway.Core/Bacera.Gateway.Core.csproj", "services/mono/Bacera.Gateway.Core/"]
COPY ["services/mono/Bacera.Gateway.TradingData/Bacera.Gateway.TradingData.csproj", "services/mono/Bacera.Gateway.TradingData/"]
COPY ["services/mono/Bacera.Core/Bacera.Core.csproj", "services/mono/Bacera.Core/"]

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "services/mono/Bacera.Gateway.Web/Bacera.Gateway.Web.csproj"

COPY proto/ proto/
COPY services/mono/ services/mono/

WORKDIR "/src/services/mono/Bacera.Gateway.Web"
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet build "Bacera.Gateway.Web.csproj" -c Release -o /app/build /m:1 /p:RunAnalyzers=false /p:Protobuf_ProtocFullPath=/usr/bin/protoc

FROM build AS publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "Bacera.Gateway.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false /m:1 /p:RunAnalyzers=false /p:Protobuf_ProtocFullPath=/usr/bin/protoc

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV REDIS_CONNECTION=redis:6379
ENV REDIS_PASSWORD=

ENTRYPOINT ["dotnet", "Bacera.Gateway.Web.dll"]
