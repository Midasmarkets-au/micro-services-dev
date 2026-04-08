# Deployment Guide

## Architecture Overview

```
                        ┌─────────────────────────────────────────┐
                        │              Kubernetes Cluster          │
                        │                                          │
Internet ──► Ingress ──►│  mono (.NET)  ──gRPC──► idgen (Rust)   │
             (NGINX)    │      │                                   │
                        │      └──gRPC──► boardcast (Rust)        │
                        │                    │                     │
             /event ───►│              boardcast:9003              │
             (SSE)      │                                          │
                        │  Redis   PostgreSQL   MySQL              │
                        └─────────────────────────────────────────┘
```

| Service | Container | Image | Port(s) | Description |
|---|---|---|---|---|
| Gateway Web | `bacera-gateway-web` | `bacera-gateway-web:local` | `9005` (HTTP), `50005` (gRPC) | .NET 8 gateway monolith |
| ID Generator | `bacera-idgen` | `bacera-idgen:local` | `50001` (gRPC), `8080` (HTTP) | Rust Snowflake ID generator |
| Boardcast | `bacera-boardcast` | `bacera-boardcast:local` | `50003` (gRPC), `9003` (HTTP/SSE) | Rust SSE push + gRPC broadcast |
| PostgreSQL | `bacera-postgres` | `postgres:15-alpine` | `5432` | Primary relational database |
| PgBouncer | `bacera-pgbouncer` | `pgbouncer/pgbouncer:latest` | `6432` | PostgreSQL connection pooler |
| Redis | `bacera-redis` | `redis:7-alpine` | `6379` | Cache & session store |
| MySQL | `bacera-mysql` | `mysql:8.0` | `3306` | Website database |
| Seq | `bacera-seq` | `datalust/seq:latest` | `5341` (ingest), `5342` (UI) | Structured log aggregation |

---

## Local Development Setup

### Prerequisites

- Docker & Docker Compose
- .NET SDK (for EF migrations and seed tool)

### 1. Start Infrastructure Services

```bash
cd deployment
docker compose -f docker-compose.local.yml up -d
```

To start only infrastructure (without app services):

```bash
docker compose -f docker-compose.local.yml up -d postgres redis mysql seq pgbouncer
```

### 2. Environment Configuration

Copy and configure the environment file:

```bash
cp deployment/.env.local .env
```

Key environment variables:

| Variable | Default | Description |
|---|---|---|
| `DB_HOST` | `localhost` | PostgreSQL host |
| `DB_PORT` | `5432` | PostgreSQL port |
| `DB_DATABASE` | `portal_central` | Primary database name |
| `DB_USERNAME` | `postgres` | PostgreSQL user |
| `DB_PASSWORD` | — | PostgreSQL password |
| `REDIS_CONNECTION` | `localhost:6379` | Redis connection string |
| `REDIS_PASSWORD` | — | Redis password |
| `IDGEN_GRPC_ADDR` | `http://idgen:50001` | idgen gRPC address (mono uses this) |
| `BOARDCAST_GRPC_ADDR` | `http://boardcast:50003` | boardcast gRPC address (mono uses this) |

---

## Service Details

### Gateway Web (`bacera-gateway-web`)

- **Dockerfile:** `services/mono/Dockerfile`
- **Port:** `9005` (HTTP), `50005` (gRPC)
- Depends on: `postgres` (healthy), `redis` (healthy), `idgen`, `boardcast`

Key environment variables:

| Variable | Value |
|---|---|
| `ASPNETCORE_URLS` | `http://+:80` |
| `ASPNETCORE_ENVIRONMENT` | `Development` |
| `REDIS_CONNECTION` | `redis:6379` |
| `IDGEN_GRPC_ADDR` | `http://idgen:50001` |
| `BOARDCAST_GRPC_ADDR` | `http://boardcast:50003` |

### ID Generator (`bacera-idgen`)

- **Dockerfile:** `services/idgen/Dockerfile`
- **gRPC port:** `50001`, **HTTP port:** `8080`

### Boardcast (`bacera-boardcast`)

- **Dockerfile:** `services/boardcast/Dockerfile`
- **gRPC port:** `50003`, **HTTP port:** `9003`
- **SSE 订阅:** `GET http://localhost:9003/event?channel=<name>`
- **消息发布:** `POST http://localhost:9003/publish`  `{"channel":"...","message":"..."}`

```bash
# 订阅频道
curl -N http://localhost:9003/event?channel=test

# 发布消息
curl -X POST http://localhost:9003/publish \
  -H "Content-Type: application/json" \
  -d '{"channel":"test","message":"Hello!"}'

# 通过 mono REST API 发布（走 gRPC 链路）
curl -X POST http://localhost:9000/api/v1/boardcast/publish \
  -H "Content-Type: application/json" \
  -d '{"channel":"test","message":"Hello from mono!"}'
```

浏览器测试页：`services/boardcast/test-sse.html`

### PostgreSQL (`bacera-postgres`)

- **Image:** `postgres:15-alpine` | **Port:** `5432`
- **Health check:** `pg_isready -U postgres -d portal_central`

### PgBouncer (`bacera-pgbouncer`)

- **Port:** `6432` → `postgres:5432` | **Pool mode:** `transaction`

### Redis (`bacera-redis`)

- **Image:** `redis:7-alpine` | **Port:** `6379`
- **Persistence:** AOF enabled | **Eviction:** `allkeys-lru`

### MySQL (`bacera-mysql`)

- **Image:** `mysql:8.0` | **Port:** `3306` | **Database:** `website`

### Seq (`bacera-seq`)

- **Web UI:** http://localhost:5342 | **Ingestion:** `5341`

---

## Database Setup

### Run EF Core Migrations

```bash
source /etc/bacera-gateway/.env

dotnet ef database update --context AuthDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_central;Username=postgres;Password=$DB_PASSWORD"

dotnet ef database update --context CentralDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_central;Username=postgres;Password=$DB_PASSWORD"

dotnet ef database update --context TenantDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_tenant_1;Username=postgres;Password=$DB_PASSWORD"

# Seed admin user
cd deployment/Tools/SeedDatabaseConsole
dotnet run -- "Host=localhost;Port=5432;Username=postgres;Password=$DB_PASSWORD;Database=portal_central;"
```

---

## Authentication

```bash
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&client_id=api&username=<email>&password=<password>
```

---

## Health Checks

```bash
# PostgreSQL
docker exec bacera-postgres pg_isready -U postgres -d portal_central

# Redis
docker exec bacera-redis redis-cli -a <password> ping

# idgen HTTP
curl http://localhost:8080/api/v1/health

# boardcast SSE (连接即健康)
curl -N http://localhost:9003/event?channel=health-check

# Seq
curl -f http://localhost:5341/health
```
