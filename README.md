# MM-Back

Monorepo backend containing three services: **mono** (.NET 8 gateway), **idgen** (Rust gRPC/HTTP ID generator), and **boardcast** (Rust SSE push + gRPC broadcast).

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
├── proto/                      # Protobuf definitions (shared)
│   └── api/v1/
│       ├── service.proto       #   ApiService (gRPC + HTTP)
│       ├── hello.proto
│       ├── auth.proto
│       └── boardcast.proto     #   BoardcastService (Publish / Subscribe)
├── scripts/                    # Code-gen & helper scripts
├── services/
│   ├── idgen/                  # Rust — ID generator (gRPC :50051 + HTTP :8080)
│   │   ├── Cargo.toml
│   │   ├── build.rs
│   │   └── src/
│   ├── boardcast/              # Rust — SSE push + gRPC broadcast (gRPC :50052 + HTTP :8081)
│   │   ├── Cargo.toml
│   │   ├── build.rs
│   │   ├── Dockerfile
│   │   ├── test-sse.html       #   Browser SSE test page
│   │   └── src/
│   │       ├── lib.rs          #   BroadcastBus (DashMap + tokio broadcast)
│   │       └── main.rs         #   Axum SSE handler + Tonic gRPC server
│   ├── mono/                   # .NET 8 — Gateway Web monolith
│   │   ├── Bacera.Core/
│   │   ├── Bacera.Gateway.Core/
│   │   ├── Bacera.Gateway.Infrastructure/
│   │   ├── Bacera.Gateway.Msg/
│   │   ├── Bacera.Gateway.TradingData/
│   │   └── Bacera.Gateway.Web/
│   │       └── Controllers/
│   │           └── BoardcastController.cs  # POST /api/v1/boardcast/publish
│   └── auth/                   # Rust — Auth service (OAuth2 token endpoint)
├── tests/                      # .NET test projects
├── tools/                      # CLI utilities
├── deployment/
│   ├── docker-compose.local.yml
│   └── k8s/                    # Kubernetes manifests
└── .github/
    └── workflows/
        ├── deploy-mono-staging.yml
        └── deploy-boardcast-staging.yml
```

## Services Overview

| Service | Language | gRPC Port | HTTP Port | Description |
|---------|----------|-----------|-----------|-------------|
| mono | .NET 8 | — | 80 | Gateway Web monolith |
| idgen | Rust | 50051 | 8080 | Snowflake ID generator |
| boardcast | Rust | 50052 | 8081 | SSE push + gRPC broadcast |
| auth | Rust | — | — | OAuth2 token endpoint |

## Quick Start — Local Development

### 1. Start Infrastructure

```bash
docker compose -f deployment/docker-compose.local.yml up -d postgres redis mysql seq pgbouncer
```

### 2. Configure App Settings

```bash
cp services/mono/Bacera.Gateway.Web/appsettings.Sample.json \
   services/mono/Bacera.Gateway.Web/appsettings.json
```

### 3. Run mono (.NET Gateway)

```bash
dotnet run --project services/mono/Bacera.Gateway.Web
# API: http://localhost:5000
```

### 4. Run idgen (Rust ID Generator)

```bash
cargo run -p idgen
# gRPC: :50051  HTTP: :8080
```

### 5. Run boardcast (Rust SSE + gRPC)

```bash
cargo run -p boardcast
# gRPC: :50052  HTTP: :8081
# SSE:  GET  http://localhost:8081/event?channel=<name>
# Push: POST http://localhost:8081/publish  {"channel":"...","message":"..."}
```

Open `services/boardcast/test-sse.html` in a browser to test SSE interactively.

### 6. Run Tests

```bash
dotnet test
cargo test -p idgen
cargo test -p boardcast
```

## Boardcast API

### SSE Subscribe (browser / curl)

```bash
curl -N http://localhost:8081/event?channel=test
```

### Publish via boardcast HTTP

```bash
curl -X POST http://localhost:8081/publish \
  -H "Content-Type: application/json" \
  -d '{"channel":"test","message":"Hello SSE!"}'
```

### Publish via mono REST API (gRPC relay)

```bash
curl -X POST http://localhost:5000/api/v1/boardcast/publish \
  -H "Content-Type: application/json" \
  -d '{"channel":"test","message":"Hello from mono!"}'
```

## Docker Build

### Build boardcast image

```bash
docker build -f services/boardcast/Dockerfile -t boardcast .
```

### Build idgen image

```bash
docker build -f services/idgen/Dockerfile -t idgen .
```

### Build mono image

```bash
docker build -f services/mono/Dockerfile -t mono .
```

## Protobuf Code Generation

Proto definitions live in `proto/api/v1/`. To regenerate C# stubs:

```bash
bash scripts/generate_proto.sh
```

Rust stubs are generated automatically at `cargo build` time via `build.rs`.

## Deployment

- **Kubernetes** — Manifests in `deployment/k8s/` (see [`deployment/k8s/README.md`](deployment/k8s/README.md))
- **CI/CD** — GitHub Actions in `.github/workflows/`; Azure Pipelines in `azure-pipelines.yml`

### Key Environment Variables

| Variable | Service | Default | Description |
|----------|---------|---------|-------------|
| `IDGEN_GRPC_ADDR` | mono | `http://idgen:50051` | idgen gRPC address |
| `BOARDCAST_GRPC_ADDR` | mono | `http://boardcast:50052` | boardcast gRPC address |
| `GRPC_ADDR` | idgen / boardcast | `:50051` / `:50052` | gRPC listen address |
| `HTTP_ADDR` | idgen / boardcast | `:8080` / `:8081` | HTTP listen address |
