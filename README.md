# MM-Back

Monorepo backend containing two services: **mono** (.NET 8 gateway) and **idgen** (Rust gRPC/HTTP ID generator).

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
│   ├── idgen/                  # Rust — ID generator (gRPC :50051 + HTTP :8080)
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
│   └── auth/                   # (reserved)
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

### 3. Run mono (.NET Gateway)

```bash
dotnet restore
dotnet run --project services/mono/Bacera.Gateway.Web
```

The API starts on `http://localhost:5000` (or the port configured in launch settings).

### 4. Run idgen (Rust ID Generator)

```bash
cargo build -p idgen
cargo run -p idgen
```

Exposes gRPC on `:50051` and HTTP on `:8080`.

### 5. Run Tests

```bash
# .NET tests
dotnet test

# Rust tests
cargo test -p idgen
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
| `gateway-web` | `bacera-gateway-web:local` | `9000:80` | .NET 8 gateway (mono) |
| `idgen` | `bacera-idgen:local` | `50051`, `8080` | Rust gRPC + HTTP ID generator |
| `postgres` | `postgres:15-alpine` | `5432` | Primary PostgreSQL database |
| `pgbouncer` | `pgbouncer/pgbouncer:latest` | `6432` | Connection pooler (transaction mode) |
| `redis` | `redis:7-alpine` | `6379` | Cache & session store |
| `mysql` | `mysql:8.0` | `3306` | Website MySQL database |
| `seq` | `datalust/seq:latest` | `5341`, `5342` | Structured log aggregation |

### Ports Reference

| Service | Container | Port | Protocol |
|---|---|---|---|
| mono (gateway-web) | `gateway-web` | 9000 | HTTP |
| idgen | `idgen` | 50051 | gRPC |
| idgen | `idgen` | 8080 | HTTP |
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
