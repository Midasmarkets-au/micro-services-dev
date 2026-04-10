# MM-Back

Monorepo backend containing multiple services: **mono** (.NET 8 gateway), **auth** (Rust OAuth2/JWT, HTTP :9002, gRPC :50002), **idgen** (Rust gRPC/HTTP ID generator), **boardcast** (Rust SSE + gRPC broadcast), **scheduler** (Rust background jobs), and **app/web/vue** (Vue 3 frontend — client & tenant portals).

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 8.0+ | `dotnet --version` |
| Rust | 1.85+ (edition 2024) | `rustup update stable` |
| protoc | 3.x+ | `brew install protobuf` / `apt install protobuf-compiler` |
| Docker | 20.10+ | For containerised runs |
| Docker Compose | v2 | Bundled with Docker Desktop |

## Directory Structure

```
.
├── Bacera.Gateway.sln          # .NET solution (mono service + tests + tools)
├── Cargo.toml                  # Rust workspace root
├── Dockerfile                  # Multi-target: --target idgen / --target mono
├── proto/                      # Protobuf definitions (shared)
│   └── api/v1/                 #   service.proto, hello.proto
├── scripts/                    # Code-gen & helper scripts
│   ├── generate_proto.sh       #   Generate C#/Rust from proto
│   ├── grpc_csharp_plugin_rosetta.sh
│   └── run-with-local-protoc.sh
├── services/
│   ├── idgen/                  # Rust — ID generator (gRPC :50001 + HTTP :8080)
│   │   ├── Cargo.toml
│   │   ├── build.rs
│   │   └── src/
│   ├── mono/                   # .NET 8 — Gateway Web monolith
│   │   ├── Bacera.Core/
│   │   ├── Bacera.Gateway.Core/
│   │   ├── Bacera.Gateway.Infrastructure/
│   │   ├── Bacera.Gateway.Msg/
│   │   ├── Bacera.Gateway.TradingData/
│   │   ├── Bacera.Gateway.Web/          # ASP.NET entry point
│   │   └── proto/                       # Generated C# gRPC stubs
│   ├── auth/                   # Rust — Auth service (Axum, HTTP :9002, gRPC :50002)
│   ├── boardcast/              # Rust — SSE push + gRPC broadcast (HTTP :9003, gRPC :50003)
│   └── scheduler/              # Rust — Background job processor (HTTP :9004, gRPC :50004)
├── tests/                      # .NET test projects
├── tools/                      # CLI utilities (Poster, Cleaner, Register, etc.)
├── deployment/
│   ├── docker-compose.local.yml  # Local full-stack (Postgres, Redis, MySQL, Seq)
│   └── k8s/                      # Kubernetes manifests
└── entrypoint-with-redis.sh    # Container entrypoint for mono (embedded Redis)
```

## Quick Start — Local Development

### 1. Start Infrastructure

```bash
docker compose -f deployment/docker-compose.local.yml up -d postgres redis mysql seq pgbouncer
```

This starts:

| Container | Image | Port(s) |
|---|---|---|
| `postgres` | `postgres:15-alpine` | `5432` |
| `redis` | `redis:7-alpine` | `6379` |
| `mysql` | `mysql:8.0` | `3306` |
| `seq` | `datalust/seq:latest` | `5341` (ingest), `5342` (UI) |
| `pgbouncer` | `pgbouncer/pgbouncer:latest` | `6432` → `postgres:5432` |

### 2. Configure App Settings

```bash
cp services/mono/Bacera.Gateway.Web/appsettings.Sample.json \
   services/mono/Bacera.Gateway.Web/appsettings.json
```

Edit `appsettings.json` with your local DB/Redis connection strings.

### 3. Run Services

#### mono — .NET 8 Gateway

```bash
dotnet restore
dotnet run --project services/mono/Bacera.Gateway.Web
```

API starts on `http://localhost:5000`.

#### auth — Rust OAuth2/JWT Auth Service

```bash
cargo run -p auth
```

Exposes HTTP on `:9002` (`/connect/token`, `/api/v2/auth/*`, `/.well-known/jwks.json`) and gRPC on `:50002` (`AuthValidationService`).

#### idgen — Rust ID Generator (gRPC + HTTP)

```bash
cargo run -p idgen
```

Exposes gRPC on `:50001` and HTTP on `:8080`.

#### boardcast — Rust SSE + gRPC Broadcast Service

```bash
cargo run -p boardcast
```

Exposes gRPC on `:50003` and HTTP/SSE on `:9003`.

#### scheduler — Rust Background Job Processor

```bash
cargo run -p scheduler
```

Exposes HTTP (Apalis dashboard) on `:9004` and gRPC on `:50004`.

### 4. Run Vue Web App (app/web/vue)

```bash
cd app/web/vue
npm install
```

| Command | Description |
|---|---|
| `npm run serve.client` | Dev server — Client portal |
| `npm run serve.tenant` | Dev server — Tenant portal |
| `npm run serve.backend` | Dev server — Backend admin |
| `npm run build.client` | Production build — Client portal |
| `npm run build.client.prod` | Production build — Client (prod mode) |
| `npm run build.tenant` | Production build — Tenant portal |
| `npm run build.backend` | Production build — Backend admin |

### 5. Run Tests

```bash
# .NET tests
dotnet test

# Rust tests (root workspace)
cargo test -p idgen
cargo test -p auth
cargo test -p scheduler
```

## Docker Build

The `Dockerfile` uses multi-stage builds with `--target` to produce separate images.

### Build idgen Image

```bash
docker build --target idgen -t idgen .
```

### Build mono Image

```bash
docker build --target mono -t gateway-web .
```

### Run with Docker Compose (All Services)

```bash
docker compose -f deployment/docker-compose.local.yml up -d
```

This starts the full stack:

| Container | Image | Port(s) | Description |
|---|---|---|---|
| `auth` | `bacera-auth:local` | `9002`, `50002` | Rust OAuth2/JWT auth service |
| `gateway-web` | `bacera-gateway-web:local` | `9005`, `50005` | .NET 8 gateway (mono) |
| `idgen` | `bacera-idgen:local` | `50001`, `8080` | Rust gRPC + HTTP ID generator |
| `boardcast` | `bacera-boardcast:local` | `50003`, `9003` | Rust SSE push + gRPC broadcast |
| `scheduler` | `bacera-scheduler:local` | `50004`, `9004` | Rust background job processor |
| `postgres` | `postgres:15-alpine` | `5432` | Primary PostgreSQL database |
| `pgbouncer` | `pgbouncer/pgbouncer:latest` | `6432` | Connection pooler (transaction mode) |
| `redis` | `redis:7-alpine` | `6379` | Cache & session store |
| `mysql` | `mysql:8.0` | `3306` | Website MySQL database |
| `seq` | `datalust/seq:latest` | `5341`, `5342` | Structured log aggregation |

### Ports Reference

| Service | Container | Port | Protocol |
|---|---|---|---|
| mono (gateway-web) | `gateway-web` | 9005 | HTTP |
| mono (gRPC callback) | `gateway-web` | 50005 | gRPC |
| idgen | `idgen` | 50001 | gRPC |
| idgen | `idgen` | 8080 | HTTP |
| boardcast | `boardcast` | 50003 | gRPC |
| boardcast | `boardcast` | 9003 | HTTP/SSE |
| scheduler | `scheduler` | 50004 | gRPC |
| scheduler | `scheduler` | 9004 | HTTP |
| auth | `auth` | 9001 | HTTP |
| PostgreSQL | `postgres` | 5432 | TCP |
| PgBouncer | `pgbouncer` | 6432 | TCP |
| Redis | `redis` | 6379 | TCP |
| MySQL | `mysql` | 3306 | TCP |
| Seq UI | `seq` | 5342 | HTTP |
| Seq Ingestion | `seq` | 5341 | HTTP |

## Protobuf Code Generation

Proto definitions live in `proto/api/v1/`. To regenerate stubs for both Rust and C#:

```bash
bash scripts/generate_proto.sh
```

On macOS with Apple Silicon, use the Rosetta wrapper for the C# plugin:

```bash
bash scripts/grpc_csharp_plugin_rosetta.sh
```

## CLI Tools

Located under `tools/`, these are standalone .NET console apps:

```bash
# Run a specific tool
dotnet run --project tools/Poster
dotnet run --project tools/Cleaner
dotnet run --project tools/Register
dotnet run --project tools/Bacera.Gateway.Console.TransactionImporter
```

## Deployment

- **Docker Compose** — `deployment/docker-compose.local.yml` for local/staging. See [`deployment/readme.md`](deployment/readme.md) for full setup instructions including database migrations and seeding.
- **Kubernetes** — Manifests in `deployment/k8s/` (see `deployment/k8s/README.md`)
- **CI/CD** — GitHub Actions (`.github/workflows/ec2-docker-deploy.yml`) and Azure Pipelines (`azure-pipelines.yml`)

### Key Configuration Files

| File | Purpose |
|---|---|
| `deployment/docker-compose.local.yml` | Full local stack (all 7 services) |
| `deployment/.env.local` | Local environment variables template |
| `deployment/appsettings.local.json` | Local app settings override |
| `services/mono/Bacera.Gateway.Web/appsettings.json` | .NET app configuration |
