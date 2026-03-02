# Deployment Guide

## Architecture Overview

This project uses a microservice architecture with the following services:

| Service | Container | Image | Port(s) | Description |
|---|---|---|---|---|
| Gateway Web | `bacera-gateway-web` | `bacera-gateway-web:local` | `9000:80` | Mono .NET gateway service |
| ID Generator | `bacera-idgen` | `bacera-idgen:local` | `50051` (gRPC), `8080` (HTTP) | Rust gRPC + HTTP ID generation service |
| PostgreSQL | `bacera-postgres` | `postgres:15-alpine` | `5432:5432` | Primary relational database |
| PgBouncer | `bacera-pgbouncer` | `pgbouncer/pgbouncer:latest` | `6432:5432` | PostgreSQL connection pooler |
| Redis | `bacera-redis` | `redis:7-alpine` | `6379:6379` | Cache & session store |
| MySQL | `bacera-mysql` | `mysql:8.0` | `3306:3306` | Website database |
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
| `DB_WEBSITE_HOST` | `bacera-mysql` | MySQL host |
| `DB_WEBSITE_PORT` | `3306` | MySQL port |
| `DB_WEBSITE_DATABASE` | `website` | MySQL database name |
| `HANGFIRE_DATABASE` | `portal_hangfire` | Hangfire job database |
| `IDENTITY_AUTHORITY` | `http://localhost:5000` | OpenIddict authority URL |

---

## Service Details

### PostgreSQL (`bacera-postgres`)

- **Image:** `postgres:15-alpine`
- **Port:** `5432`
- **Default DB:** `portal_central`
- **User:** `postgres`
- **Volume:** `postgres_data`
- **Health check:** `pg_isready -U postgres -d portal_central`

### PgBouncer (`bacera-pgbouncer`)

- **Image:** `pgbouncer/pgbouncer:latest`
- **Port:** `6432` → forwards to `postgres:5432`
- **Pool mode:** `transaction`
- **Max client connections:** `1000`
- **Default pool size:** `20`
- Depends on: `postgres`

### Redis (`bacera-redis`)

- **Image:** `redis:7-alpine`
- **Port:** `6379`
- **Volume:** `redis_data`
- **Persistence:** AOF enabled (`appendonly yes`)
- **Eviction policy:** `allkeys-lru`
- **Health check:** `redis-cli -a <password> ping`

### MySQL (`bacera-mysql`)

- **Image:** `mysql:8.0`
- **Port:** `3306`
- **Database:** `website`
- **User:** `website_user`
- **Volume:** `mysql_data`
- **Root password:** controlled by `MYSQL_WEBSITE_PASSWORD` env var

### Seq (`bacera-seq`)

- **Image:** `datalust/seq:latest`
- **Ingestion port:** `5341`
- **Web UI port:** `5342`
- **Volume:** `seq_data`
- **Web UI:** http://localhost:5342
- **Health check:** `GET http://localhost:5341/health`

### Gateway Web (`bacera-gateway-web`)

- **Dockerfile:** `services/mono/Dockerfile`
- **Port:** `9000` → internal `80`
- **Environment:** `Development`
- Depends on: `postgres` (healthy), `redis` (healthy)

Key environment variables:

| Variable | Value |
|---|---|
| `ASPNETCORE_URLS` | `http://+:80` |
| `ASPNETCORE_ENVIRONMENT` | `Development` |
| `REDIS_CONNECTION` | `redis:6379` |
| `DB_HOST` | `postgres` |
| `DB_PORT` | `5432` |
| `DB_DATABASE` | `portal_central` |

### ID Generator (`bacera-idgen`)

- **Dockerfile:** `services/idgen/Dockerfile`
- **gRPC port:** `50051`
- **HTTP port:** `8080`

Key environment variables:

| Variable | Value |
|---|---|
| `GRPC_ADDR` | `[::]:50051` |
| `HTTP_ADDR` | `[::]:8080` |

---

## Database Setup

### Reset & Re-migrate (if needed)

```bash
# Stop the application to prevent connections
sudo docker stop bacera-gateway-web

# Reset the database completely (if needed)
sudo docker exec bacera-postgres psql -U postgres -c "DROP DATABASE portal_central;"
sudo docker exec bacera-postgres psql -U postgres -c "CREATE DATABASE portal_central;"
sudo docker exec bacera-postgres psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE portal_central TO postgres;"
```

### Run EF Core Migrations

```bash
cd /tmp/source/MM-Back
source /etc/bacera-gateway/.env

# 1. Auth + OpenIddict tables (users, roles, OpenIddictApplications/Tokens/Scopes/Authorizations)
dotnet ef database update --context AuthDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_central;Username=postgres;Password=$DB_PASSWORD;Command Timeout=600" \
  --verbose

# 2. Central tables
dotnet ef database update --context CentralDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_central;Username=postgres;Password=$DB_PASSWORD;Command Timeout=600" \
  --verbose

# 3. Tenant tables
dotnet ef database update --context TenantDbContext \
  --project services/mono/Bacera.Gateway.Infrastructure \
  --connection "Host=localhost;Port=5432;Database=portal_tenant_1;Username=postgres;Password=$DB_PASSWORD;Command Timeout=600" \
  --verbose

# 4. Seed admin user
cd deployment/Tools/SeedDatabaseConsole
dotnet run -- "Host=localhost;Port=5432;Username=postgres;Password=$DB_PASSWORD;Database=portal_central;"

# NOTE: OpenIddict clients (api, mobile) and scopes are seeded automatically
# by OpenIddictSeedService on first application startup — no manual step needed.

# 5. (Existing environment only) Migrate IS4 client data → OpenIddict
#    Skip this step on a fresh install.
cd /tmp/source/MM-Back
bash deployment/migrations/run_migration.sh

# Restart the application
sudo docker start bacera-gateway-web
```

---

## Authentication

### Token Endpoint

```
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&client_id=api&username=<email>&password=<password>
```

---

## Verification Queries

### Verify OpenIddict Tables

```sql
SELECT 'OpenIddictApplications' AS tbl, COUNT(*) FROM identity."OpenIddictApplications"
UNION ALL SELECT 'OpenIddictScopes',         COUNT(*) FROM identity."OpenIddictScopes"
UNION ALL SELECT 'OpenIddictTokens',         COUNT(*) FROM identity."OpenIddictTokens"
UNION ALL SELECT 'OpenIddictAuthorizations', COUNT(*) FROM identity."OpenIddictAuthorizations";
```

### Check Service Health

```bash
# PostgreSQL
docker exec bacera-postgres pg_isready -U postgres -d portal_central

# Redis
docker exec bacera-redis redis-cli -a <password> ping

# Seq
curl -f http://localhost:5341/health

# ID Generator HTTP
curl http://localhost:8080/health
```
