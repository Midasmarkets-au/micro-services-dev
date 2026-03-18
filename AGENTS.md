# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Repository Overview

Monorepo backend for a multi-tenant trading/finance platform. Services are split between .NET 8 and Rust, communicating internally via gRPC.

- `services/mono` — .NET 8 ASP.NET Core gateway (main API, port 5000/9000 in Docker)
- `services/auth` — Rust (Axum) OAuth2/JWT token endpoint (port 8081)
- `services/idgen` — Rust gRPC + HTTP Snowflake ID generator (gRPC :50051, HTTP :8080)
- `services/boardcast` — Rust SSE push + gRPC broadcast service (gRPC :50052, HTTP :8081)
- `services/scheduler` — Rust background job processor using Apalis (gRPC :50053)
- `services/auth_rust` — **Separate** Rust Cargo workspace (not part of root workspace)
- `app/web/vue` — Vue 3 frontend (client, tenant, and backend-admin portals)
- `proto/api/v1/` — Shared Protobuf definitions consumed by both .NET and Rust

## Commands

### Infrastructure (required before running services locally)

```bash
docker compose -f deployment/docker-compose.local.yml up -d postgres redis mysql seq pgbouncer
cp deployment/.env.local .env
# Edit .env with local DB/Redis credentials
cp services/mono/Bacera.Gateway.Web/appsettings.Sample.json services/mono/Bacera.Gateway.Web/appsettings.json
```

### .NET (mono gateway)

```bash
dotnet restore
dotnet build
dotnet run --project services/mono/Bacera.Gateway.Web
# Run with command mode (CLI jobs)
dotnet run --project services/mono/Bacera.Gateway.Web -- cmd
```

### Rust (root workspace: idgen, auth, boardcast, scheduler)

```bash
cargo build
cargo run -p idgen
cargo run -p auth
cargo run -p boardcast
cargo run -p scheduler
```

### Rust (auth_rust — separate workspace)

```bash
# Must be run from within the service directory
cargo run   # from services/auth_rust/
```

### Vue frontend

```bash
cd app/web/vue && npm install
npm run serve.client    # Client portal dev server
npm run serve.tenant    # Tenant portal dev server
npm run serve.backend   # Backend admin dev server
```

### Tests

```bash
# .NET — all tests
dotnet test

# .NET — single test project
dotnet test tests/Bacera.Gateway.Tests
dotnet test tests/Bacera.Gateway.Infrastructure.Tests
dotnet test tests/Bacera.Gateway.Web.Tests

# Rust — root workspace
cargo test -p idgen
cargo test -p auth

# Rust — auth_rust workspace
cd services/auth_rust && cargo test
```

### Protobuf code generation

```bash
bash scripts/generate_proto.sh
# macOS Apple Silicon (uses Rosetta for the C# plugin):
bash scripts/grpc_csharp_plugin_rosetta.sh
```

On macOS, `protoc` and `grpc_csharp_plugin` paths are configured in `services/mono/Directory.Build.props` (`/opt/homebrew/bin/`).

### Database migrations (EF Core)

```bash
dotnet ef database update --context AuthDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_central;Username=postgres;Password=$DB_PASSWORD"

dotnet ef database update --context CentralDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_central;Username=postgres;Password=$DB_PASSWORD"

dotnet ef database update --context TenantDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_tenant_1;Username=postgres;Password=$DB_PASSWORD"
```

### Docker builds

```bash
docker build --target idgen -t idgen .
docker build --target mono -t gateway-web .
docker compose -f deployment/docker-compose.local.yml up -d   # Full stack
```

## Architecture

### Service Communication

```
Internet → NGINX Ingress → mono (.NET) ──gRPC──► idgen (Rust, :50051)
                                        ──gRPC──► boardcast (Rust, :50052)
                                        ──gRPC──► scheduler (Rust, :50053)
                           scheduler ──gRPC──► mono (MonoCallbackService, for WS notifications)
```

The mono gateway exposes REST and SignalR over HTTP. gRPC is used for all inter-service calls. gRPC reflection is enabled in development (use `grpcurl` or Postman).

### mono (.NET 8) — Internal Structure

`services/mono/` contains four projects wired together by `Bacera.Gateway.Web`:

- **`Bacera.Gateway.Web`** — ASP.NET Core entry point. Startup extensions are in `StartUp/` (each `Setup*` method is a partial class). Controllers live under `Controllers/Areas/` using ASP.NET Areas for routing (`api/{area}/{controller}/{action}`).
- **`Bacera.Gateway.Core`** — Domain models, DTOs, interfaces, and business types. No infrastructure dependencies.
- **`Bacera.Gateway.Infrastructure`** — EF Core DbContexts and data access. Multiple DbContexts: `AuthDbContext`, `CentralDbContext`, `TenantDbContext`, `MetaTrade4DbContext`, `MetaTrade5DbContext`, `WebsiteDbContext`. `MyDbContextPool` provides per-tenant context pooling.
- **`Bacera.Gateway.Msg`** / **`Bacera.Gateway.TradingData`** — Messaging and trading data helpers.

### Multi-tenancy

Every HTTP request has tenant context injected by `MultiTenantServiceMiddleware` (which uses the `Tenancy` scoped service implementing `ITenantGetter`/`ITenantSetter`). `TenantDbContext` is resolved per-tenant from `MyDbContextPool`. Each tenant has its own database (`portal_tenant_<id>`).

### Authentication

`mono` runs IdentityServer4 (configured in `SetupIdentityServer()`). The `auth` Rust service mirrors the OAuth2 `/connect/token` endpoint, supporting `password` and `client_credentials` grant types. Password hashing uses ASP.NET Identity v3 format (PBKDF2/SHA256), implemented in `services/auth/src/password.rs`.

### Real-time

SignalR hubs (`/hub/client`, `/hub/tenant`, `/live/trade/symbol-group-hub`) are backed by a Redis connection. The `boardcast` Rust service handles SSE subscriptions (`GET /event?channel=<name>`) and can receive publish calls from mono via gRPC, then fan-out to SSE subscribers.

### Background Jobs

`mono` uses Hangfire for scheduled/recurring jobs (e.g., `ReportJob`, `RebateJob`, `TradeJob`). The `scheduler` Rust service uses [Apalis](https://github.com/geofmureithi/apalis) with a PostgreSQL backend for durable job queues; it calls back into mono via gRPC when jobs complete.

### Environment Variables

Both .NET and Rust services auto-load a `.env` file by walking up the directory tree. Key variables:

| Variable | Used by | Purpose |
|---|---|---|
| `DB_HOST` / `DB_PORT` / `DB_USERNAME` / `DB_PASSWORD` / `DB_DATABASE` | auth, scheduler | PostgreSQL connection |
| `REDIS_CONNECTION` | mono | Redis host:port |
| `REDIS_PASSWORD` | mono | Redis auth |
| `REDIS_CLUSTER_MODE` | mono | Enable cluster mode (disables admin commands) |
| `JWT_SECRET` | auth | JWT signing key |
| `IDGEN_GRPC_ADDR` | mono | idgen gRPC URL (default `http://idgen:50051`) |
| `BOARDCAST_GRPC_ADDR` | mono | boardcast gRPC URL (default `http://boardcast:50052`) |
| `SCHEDULER_GRPC_URL` | mono | scheduler gRPC URL (default `http://scheduler:50053`) |
| `CORS_ALLOWED_ORIGINS` | mono | Comma-separated origins (production) |
| `API_LOG_ENABLE` | mono | Enable `ApiLogMiddleware` (`true`/`false`) |
| `ASPNETCORE_ENVIRONMENT` | mono | `Development` / `Staging` / `Production` |

`ASPNETCORE_ENVIRONMENT=Development` enables Swagger UI and permissive CORS. Production/Staging enforce `AllowSpecificOrigins` from `CORS_ALLOWED_ORIGINS`.
