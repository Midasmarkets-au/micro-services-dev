#!/usr/bin/env bash
# reset_rebate.sh — Reset scheduler rebate tables and Redis keys, then restart the scheduler pod.
#
# Usage:
#   ./scripts/reset_rebate.sh [year]
#   ./scripts/reset_rebate.sh          # defaults to current year
#   ./scripts/reset_rebate.sh 2026

set -euo pipefail

YEAR="${1:-$(date +%Y)}"

# ── Colors ────────────────────────────────────────────────────────────────────
GREEN="\033[32m"; RED="\033[31m"; YELLOW="\033[33m"
CYAN="\033[36m"; BOLD="\033[1m"; RESET="\033[0m"
info()   { printf "${CYAN}[info]${RESET}  %s\n" "$*"; }
ok()     { printf "${GREEN}[ok]${RESET}    %s\n" "$*"; }
warn()   { printf "${YELLOW}[warn]${RESET}  %s\n" "$*"; }
die()    { printf "${RED}[error]${RESET} %s\n" "$*" >&2; exit 1; }
step()   { printf "\n${BOLD}── %s${RESET}\n" "$*"; }

# ── Load .env ─────────────────────────────────────────────────────────────────
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"
[[ -f "$ENV_FILE" ]] || die ".env not found at $ENV_FILE"

while IFS='=' read -r key rest; do
  [[ "$key" =~ ^[[:space:]]*# ]] && continue
  [[ -z "${key// /}" ]] && continue
  val="${rest%%#*}"
  val="${val#"${val%%[! ]*}"}"; val="${val%"${val##*[! ]}"}"
  val="${val%\"*}"; val="${val#\"}"; val="${val%\'*}"; val="${val#\'}"
  export "${key// /}=$val" 2>/dev/null || true
done < "$ENV_FILE"

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USERNAME="${DB_USERNAME:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-}"
TENANT_PREFIX="${TENANT_DB_NAME_PREFIX:-portal_tenant_}"
REDIS_HOST="${REDIS_CONNECTION%%:*}"
REDIS_HOST="${REDIS_HOST:-localhost}"
REDIS_PORT="${REDIS_CONNECTION##*:}"
REDIS_PORT="${REDIS_PORT:-6379}"
REDIS_PASS="${REDIS_PASSWORD:-}"

export PGPASSWORD="$DB_PASSWORD"
PSQL="psql -h $DB_HOST -p $DB_PORT -U $DB_USERNAME"

redis_cmd() {
  if [[ -n "$REDIS_PASS" ]]; then
    redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$REDIS_PASS" "$@" 2>/dev/null
  else
    redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" "$@" 2>/dev/null
  fi
}

# ── Discover tenant DBs that have _TradeRebate_{YEAR} ────────────────────────
step "Scanning tenant databases for _TradeRebate_${YEAR}..."

TENANT_DBS=()
while IFS=$'\x01' read -r db _; do
  db="${db// /}"
  [[ -z "$db" ]] && continue
  exists=$($PSQL -d "$db" -tAc \
    "SELECT 1 FROM information_schema.tables
     WHERE table_schema='trd' AND table_name='_TradeRebate_${YEAR}' LIMIT 1" 2>/dev/null || true)
  if [[ "${exists// /}" == "1" ]]; then
    TENANT_DBS+=("$db")
    info "Found: $db"
  fi
done < <($PSQL -d postgres -tA --field-separator $'\x01' -c \
  "SELECT datname FROM pg_database WHERE datname LIKE '${TENANT_PREFIX}%' ORDER BY datname" 2>/dev/null)

[[ "${#TENANT_DBS[@]}" -gt 0 ]] || die "No tenant DB found with _TradeRebate_${YEAR}."

# ── Confirm ───────────────────────────────────────────────────────────────────
printf "\n${YELLOW}This will:${RESET}\n"
printf "  1. DROP   trd._Rebate_%s  +  core._Matter_%s  in each tenant DB\n" "$YEAR" "$YEAR"
printf "  2. RESET  trd._TradeRebate_%s  Status → 0  (completed rows only)\n" "$YEAR"
printf "  3. DELETE Redis keys: scheduler_calculate_rebate_lock_tid:*  +  account:tenant:map\n"
printf "  4. RESTART  kubectl deployment/scheduler\n"
printf "\nTenant DBs: %s\n" "${TENANT_DBS[*]}"
printf "\n${BOLD}Continue? [y/N] ${RESET}"
read -r answer
[[ "$answer" =~ ^[Yy]$ ]] || { echo "Aborted."; exit 0; }

# ── Reset each tenant DB ──────────────────────────────────────────────────────
step "Resetting database tables (year=${YEAR})..."

for db in "${TENANT_DBS[@]}"; do
  info "Processing $db..."

  # Count completed TradeRebate records before reset
  completed=$($PSQL -d "$db" -tAc \
    "SELECT COUNT(*) FROM trd.\"_TradeRebate_${YEAR}\" WHERE \"Status\" = 2" 2>/dev/null || echo 0)
  completed="${completed// /}"

  $PSQL -d "$db" -c "
    BEGIN;
    DROP TABLE IF EXISTS trd.\"_Rebate_${YEAR}\";
    DROP TABLE IF EXISTS core.\"_Matter_${YEAR}\";
    UPDATE trd.\"_TradeRebate_${YEAR}\" SET \"Status\" = 0, \"UpdatedOn\" = NOW() WHERE \"Status\" = 2;
    COMMIT;
  " > /dev/null

  # Verify
  pending=$($PSQL -d "$db" -tAc \
    "SELECT COUNT(*) FROM trd.\"_TradeRebate_${YEAR}\" WHERE \"Status\" = 0" 2>/dev/null || echo 0)
  pending="${pending// /}"

  rebate_gone=$($PSQL -d "$db" -tAc \
    "SELECT COUNT(*) FROM information_schema.tables
     WHERE table_schema='trd' AND table_name='_Rebate_${YEAR}'" 2>/dev/null || echo 1)
  matter_gone=$($PSQL -d "$db" -tAc \
    "SELECT COUNT(*) FROM information_schema.tables
     WHERE table_schema='core' AND table_name='_Matter_${YEAR}'" 2>/dev/null || echo 1)

  [[ "${rebate_gone// /}" == "0" ]] && ok "  _Rebate_${YEAR} dropped" || warn "  _Rebate_${YEAR} still exists"
  [[ "${matter_gone// /}" == "0" ]] && ok "  _Matter_${YEAR} dropped" || warn "  _Matter_${YEAR} still exists"
  ok "  _TradeRebate_${YEAR}: reset ${completed} completed → pending (now ${pending} pending)"
done

# ── Clear Redis ───────────────────────────────────────────────────────────────
step "Clearing Redis keys..."

# Scheduler calculate lock keys (one per tenant)
lock_keys=$(redis_cmd KEYS "scheduler_calculate_rebate_lock_tid:*" 2>/dev/null || true)
if [[ -n "$lock_keys" ]]; then
  while IFS= read -r key; do
    [[ -z "$key" ]] && continue
    redis_cmd DEL "$key" > /dev/null
    ok "Deleted lock key: $key"
  done <<< "$lock_keys"
else
  info "No scheduler lock keys found"
fi

# Account-tenant map cache
if redis_cmd EXISTS "account:tenant:map" | grep -q "^1"; then
  redis_cmd DEL "account:tenant:map" > /dev/null
  ok "Deleted: account:tenant:map"
else
  info "account:tenant:map not present"
fi

# ── Restart scheduler ─────────────────────────────────────────────────────────
step "Restarting scheduler deployment..."

if ! kubectl get deployment scheduler -n default &>/dev/null; then
  warn "deployment/scheduler not found in namespace 'default', skipping restart"
else
  kubectl rollout restart deployment/scheduler -n default
  info "Waiting for rollout to complete..."
  kubectl rollout status deployment/scheduler -n default
  ok "Scheduler restarted"
fi

# ── Summary ───────────────────────────────────────────────────────────────────
printf "\n${BOLD}${GREEN}Done.${RESET} Scheduler will recalculate rebates on the next cron tick (~2 min).\n"
printf "Run ${BOLD}bash scripts/compare_rebate.sh <id>${RESET} after processing to verify results.\n\n"
