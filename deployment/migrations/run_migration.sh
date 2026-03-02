#!/usr/bin/env bash
# =============================================================================
# OpenIddict Migration Runner
# Usage:
#   ./run_migration.sh                        # uses .env from repo root
#   DB_HOST=x DB_PORT=5432 DB_DATABASE=y \
#   DB_USERNAME=z DB_PASSWORD=w \
#   ./run_migration.sh
#
# Steps executed:
#   01  Create OpenIddict tables (EF-generated idempotent SQL)
#   02  Migrate IS4 Clients/Scopes → OpenIddict Applications/Scopes
#   03  (optional) Drop IS4 tables — commented out by default, uncomment when ready
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"

# ---------------------------------------------------------------------------
# Load .env if present and env vars not already set
# ---------------------------------------------------------------------------
if [[ -f "$ENV_FILE" ]]; then
  echo "Loading environment from $ENV_FILE"
  while IFS= read -r line; do
    line="${line%%#*}"          # strip inline comments
    line="${line//\"/}"         # strip quotes
    [[ -z "${line// }" ]] && continue
    [[ "$line" != *=* ]] && continue
    key="${line%%=*}"
    val="${line#*=}"
    # Only set if not already in environment
    [[ -z "${!key+x}" ]] && export "$key=$val"
  done < "$ENV_FILE"
fi

# ---------------------------------------------------------------------------
# Resolve connection params (support both DB_* and DB_CONNECTION formats)
# ---------------------------------------------------------------------------
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_DATABASE="${DB_DATABASE:-portal_central}"
DB_USERNAME="${DB_USERNAME:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-}"

# If DB_CONNECTION is set (Host=...;Database=...;Username=...;Password=...)
# parse it as fallback
if [[ -n "${DB_CONNECTION:-}" ]]; then
  _parse() { echo "$DB_CONNECTION" | grep -oP "(?<=$1=)[^;]+" || true; }
  DB_HOST="${DB_HOST:-$(_parse Host)}"
  DB_PORT="${DB_PORT:-5432}"
  DB_DATABASE="${DB_DATABASE:-$(_parse Database)}"
  DB_USERNAME="${DB_USERNAME:-$(_parse Username)}"
  DB_PASSWORD="${DB_PASSWORD:-$(_parse Password)}"
fi

# ---------------------------------------------------------------------------
# Check psql is available
# ---------------------------------------------------------------------------
if ! command -v psql &>/dev/null; then
  echo "ERROR: psql not found. Install postgresql-client and retry."
  exit 1
fi

PSQL="psql -h $DB_HOST -p $DB_PORT -U $DB_USERNAME -d $DB_DATABASE"
export PGPASSWORD="$DB_PASSWORD"

echo ""
echo "============================================================"
echo "  OpenIddict Migration"
echo "  Host:     $DB_HOST:$DB_PORT"
echo "  Database: $DB_DATABASE"
echo "  User:     $DB_USERNAME"
echo "============================================================"
echo ""

# ---------------------------------------------------------------------------
# Confirm before proceeding (skip with --yes flag)
# ---------------------------------------------------------------------------
if [[ "${1:-}" != "--yes" ]]; then
  read -rp "Proceed with migration? [y/N] " confirm
  [[ "$confirm" =~ ^[Yy]$ ]] || { echo "Aborted."; exit 0; }
fi

run_sql() {
  local label="$1"
  local file="$2"
  echo ""
  echo ">>> [$label] Running: $(basename "$file")"
  $PSQL -f "$file"
  echo ">>> [$label] Done."
}

# ---------------------------------------------------------------------------
# Step 01: Create OpenIddict tables (idempotent — safe to re-run)
# ---------------------------------------------------------------------------
run_sql "01" "$SCRIPT_DIR/01_openiddict_create_tables.sql"

# ---------------------------------------------------------------------------
# Step 02: Migrate IS4 Clients → OpenIddict Applications
# Skipped automatically if IS4 tables don't exist
# ---------------------------------------------------------------------------
IS4_EXISTS=$($PSQL -tAc \
  "SELECT EXISTS (
     SELECT 1 FROM information_schema.tables
     WHERE table_schema = 'identity' AND table_name = 'Clients'
   );" 2>/dev/null || echo "f")

if [[ "$IS4_EXISTS" == "t" ]]; then
  run_sql "02" "$SCRIPT_DIR/02_migrate_is4_clients_to_openiddict.sql"
else
  echo ""
  echo ">>> [02] SKIPPED: identity.\"Clients\" table not found (IS4 not installed or already removed)"
fi

# ---------------------------------------------------------------------------
# Verification summary
# ---------------------------------------------------------------------------
echo ""
echo "============================================================"
echo "  Migration complete. Verification:"
echo "============================================================"
$PSQL -c "
SELECT
  'OpenIddictApplications' AS table_name,
  COUNT(*) AS rows
FROM identity.\"OpenIddictApplications\"
UNION ALL
SELECT 'OpenIddictScopes', COUNT(*) FROM identity.\"OpenIddictScopes\"
UNION ALL
SELECT 'OpenIddictTokens', COUNT(*) FROM identity.\"OpenIddictTokens\"
UNION ALL
SELECT 'OpenIddictAuthorizations', COUNT(*) FROM identity.\"OpenIddictAuthorizations\";
"

echo ""
echo "Next steps:"
echo "  1. Verify the rows above match your IS4 client count"
echo "  2. Proceed to Step 3: Configure OpenIddict server in Startup.IdentityServer.cs"
echo "  3. When Step 3-9 are complete, run 03_drop_is4_tables.sql to clean up"
echo ""
