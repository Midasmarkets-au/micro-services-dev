# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

Multi-tenant trading/finance platform monorepo. Services split between .NET 8 and Rust, communicating internally via gRPC.

- `services/mono` — .NET 8 ASP.NET Core gateway (main API, port 5000 local / 9000 Docker)
- `services/auth` — Rust (Axum) OAuth2/JWT token endpoint (port 8081)
- `services/idgen` — Rust gRPC + HTTP Snowflake ID generator (gRPC :50051, HTTP :8080)
- `services/boardcast` — Rust SSE push + gRPC broadcast service (gRPC :50052, HTTP :8081)
- `services/scheduler` — Rust background job processor using Apalis (gRPC :50053, HTTP :8082)
- `services/auth_rust` — **Separate** Rust Cargo workspace (not part of root workspace)
- `app/web/vue` — Vue 3 frontend (client, tenant, and backend-admin portals)
- `proto/api/v1/` — Shared Protobuf definitions consumed by both .NET and Rust

## Commands

### Infrastructure

```bash
docker compose -f deployment/docker-compose.local.yml up -d postgres redis mysql seq pgbouncer
cp deployment/.env.local .env
cp services/mono/Bacera.Gateway.Web/appsettings.Sample.json services/mono/Bacera.Gateway.Web/appsettings.json
```

### .NET (mono gateway)

```bash
dotnet restore
dotnet build
dotnet run --project services/mono/Bacera.Gateway.Web
# CLI job mode
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
cd services/auth_rust && cargo run
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
cargo test -p scheduler

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
Internet → NGINX Ingress → mono (.NET) ──gRPC──► idgen      (Rust, :50051)
                                        ──gRPC──► boardcast  (Rust, :50052)
                                        ──gRPC──► scheduler  (Rust, :50053)
                           scheduler ──gRPC──► mono (MonoCallbackGrpcService, for WS notifications)
```

The mono gateway exposes REST and SignalR over HTTP. gRPC is used for all inter-service calls. gRPC reflection is enabled in development.

### mono (.NET 8) — Internal Structure

`services/mono/Bacera.Gateway.Web/Program.cs` wires up the pipeline; all `Setup*` calls are partial-class extension methods in `StartUp/`.

- **`Bacera.Gateway.Web`** — ASP.NET Core entry point. Controllers live under `Areas/` using ASP.NET Areas routing (`api/{area}/{controller}/{action}`). `MonoCallbackGrpcService` exposes a gRPC endpoint for the Rust scheduler to trigger SignalR notifications.
- **`Bacera.Gateway.Core`** — Domain models, DTOs, interfaces, and business types. No infrastructure dependencies.
- **`Bacera.Gateway.Infrastructure`** — EF Core DbContexts: `AuthDbContext`, `CentralDbContext`, `TenantDbContext`, `MetaTrade4DbContext`, `MetaTrade5DbContext`, `WebsiteDbContext`. `MyDbContextPool` provides per-tenant context pooling.
- **`Bacera.Gateway.Msg`** / **`Bacera.Gateway.TradingData`** — Messaging and trading data helpers.

### Multi-tenancy

Every HTTP request has tenant context injected by `MultiTenantServiceMiddleware` (scoped service implementing `ITenantGetter`/`ITenantSetter`). Each tenant has its own PostgreSQL database (`portal_tenant_<id>`), resolved lazily from `MyDbContextPool`.

### Authentication

`mono` runs IdentityServer4 (`SetupIdentityServer()`). The `auth` Rust service mirrors the OAuth2 `/connect/token` endpoint supporting `password` and `client_credentials` grant types. Password hashing uses ASP.NET Identity v3 format (PBKDF2/SHA256), implemented in `services/auth/src/password.rs`.

### Real-time

SignalR hubs (`/hub/client`, `/hub/tenant`, `/live/trade/symbol-group-hub`) backed by Redis. The `boardcast` Rust service handles SSE subscriptions and fan-out; mono pushes events to it via gRPC.

### Background Jobs

`mono` uses **Hangfire** for scheduled/recurring jobs (e.g. `ReportJob`, `RebateJob`, `TradeJob`). The `scheduler` Rust service uses **Apalis** with a PostgreSQL backend for durable job queues; it calls back into mono via gRPC (`MonoCallbackGrpcService`) when jobs complete. The scheduler also runs an in-process cron loop for time-based triggers (e.g. CloseTradeJob at 22:30 UTC, AccountDailyJob at ~21:29–22:29 UTC Mon–Fri with DST adjustment).

### Key Environment Variables

| Variable | Used by | Purpose |
|---|---|---|
| `DB_HOST` / `DB_PORT` / `DB_USERNAME` / `DB_PASSWORD` / `DB_DATABASE` | auth, scheduler | PostgreSQL connection |
| `REDIS_CONNECTION` | mono | Redis host:port |
| `JWT_SECRET` | auth | JWT signing key |
| `IDGEN_GRPC_ADDR` | mono | idgen gRPC URL (default `http://idgen:50051`) |
| `BOARDCAST_GRPC_ADDR` | mono | boardcast gRPC URL (default `http://boardcast:50052`) |
| `SCHEDULER_GRPC_URL` | mono | scheduler gRPC URL (default `http://scheduler:50053`) |
| `CORS_ALLOWED_ORIGINS` | mono | Comma-separated origins (production) |
| `ASPNETCORE_ENVIRONMENT` | mono | `Development` / `Staging` / `Production` |

`ASPNETCORE_ENVIRONMENT=Development` enables Swagger UI and permissive CORS. Production/Staging enforce `AllowSpecificOrigins`.

Both .NET and Rust services auto-load a `.env` file by walking up the directory tree.
