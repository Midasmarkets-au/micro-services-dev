# Build context: repo root
#   docker build -f services/auth/Dockerfile -t auth .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app
COPY services/auth/Bacera.Gateway.Auth.csproj ./services/auth/
RUN dotnet restore services/auth/Bacera.Gateway.Auth.csproj

COPY services/auth/ ./services/auth/
RUN dotnet publish services/auth/Bacera.Gateway.Auth.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish .

ENV HTTP_ADDR=http://[::]:8081
EXPOSE 8081

ENTRYPOINT ["dotnet", "Bacera.Gateway.Auth.dll"]
