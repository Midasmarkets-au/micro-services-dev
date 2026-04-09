#!/usr/bin/env bash
# reset_rebate.sh — Reset scheduler rebate tables and Redis keys, then restart the scheduler pod.
#
# Usage:
#   ./scripts/reset_rebate.sh

set -euo pipefail

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

export PGPASSWORD="$DB_PASSWORD"
PSQL="psql -h $DB_HOST -p $DB_PORT -U $DB_USERNAME"

# Redis in k8s — use kubectl exec into the redis pod
REDIS_POD=$(kubectl get pods -n default -l app=redis -o jsonpath='{.items[0].metadata.name}' 2>/dev/null || true)
if [[ -z "$REDIS_POD" ]]; then
  REDIS_POD=$(kubectl get pods -n default | grep -i redis | awk '{print $1}' | head -1 || true)
fi
[[ -n "$REDIS_POD" ]] || warn "No Redis pod found in k8s — Redis keys will not be cleared"

redis_k8s() {
  [[ -z "$REDIS_POD" ]] && return 0
  kubectl exec -n default "$REDIS_POD" -- redis-cli "$@" 2>/dev/null
}

# ── Discover tenant DBs that have trade_rebate_k8s ───────────────────────────
step "Scanning tenant databases for trade_rebate_k8s..."

TENANT_DBS=()
while IFS=$'\x01' read -r db _; do
  db="${db// /}"
  [[ -z "$db" ]] && continue
  exists=$($PSQL -d "$db" -tAc \
    "SELECT 1 FROM information_schema.tables
     WHERE table_schema='trd' AND table_name='trade_rebate_k8s' LIMIT 1" 2>/dev/null || true)
  if [[ "${exists// /}" == "1" ]]; then
    TENANT_DBS+=("$db")
    info "Found: $db"
  fi
done < <($PSQL -d postgres -tA --field-separator $'\x01' -c \
  "SELECT datname FROM pg_database WHERE datname LIKE '${TENANT_PREFIX}%' ORDER BY datname" 2>/dev/null)

[[ "${#TENANT_DBS[@]}" -gt 0 ]] || die "No tenant DB found with trd.trade_rebate_k8s."

# ── Confirm ───────────────────────────────────────────────────────────────────
printf "\n${YELLOW}This will:${RESET}\n"
printf "  1. TRUNCATE  trd.trade_rebate_k8s  trd.rebate_k8s  core.activity_k8s  acct.wallet_transaction_k8s  core.\"_MatterK8s\"\n"
printf "  2. DELETE Redis keys (k8s): scheduler lock  +  account:tenant:map  +  trade monitor cursors  +  dedup hash\n"
printf "  3. RESTART  kubectl deployment/scheduler\n"
printf "\nTenant DBs: %s\n" "${TENANT_DBS[*]}"
printf "\n${BOLD}Continue? [y/N] ${RESET}"
read -r answer
[[ "$answer" =~ ^[Yy]$ ]] || { echo "Aborted."; exit 0; }

# ── Reset each tenant DB ──────────────────────────────────────────────────────
step "Truncating k8s partition tables..."

for db in "${TENANT_DBS[@]}"; do
  info "Processing $db..."

  $PSQL -d "$db" -c "
    BEGIN;
    TRUNCATE trd.trade_rebate_k8s;
    TRUNCATE trd.rebate_k8s;
    TRUNCATE core.activity_k8s;
    TRUNCATE acct.wallet_transaction_k8s;
    TRUNCATE core.\"_MatterK8s\";
    COMMIT;
  " > /dev/null

  ok "  Truncated all k8s rebate tables in $db"
done

# ── Clear Redis (k8s) ─────────────────────────────────────────────────────────
step "Clearing Redis keys (k8s pod: ${REDIS_POD:-none})..."

if [[ -n "$REDIS_POD" ]]; then
  # Scheduler calculate lock keys
  lock_keys=$(redis_k8s KEYS "scheduler_calculate_rebate_lock_tid:*" || true)
  if [[ -n "$lock_keys" ]]; then
    while IFS= read -r key; do
      [[ -z "$key" ]] && continue
      redis_k8s DEL "$key" > /dev/null
      ok "Deleted lock key: $key"
    done <<< "$lock_keys"
  else
    info "No scheduler lock keys found"
  fi

  # Account-tenant map cache
  if redis_k8s EXISTS "account:tenant:map" | grep -q "^1"; then
    redis_k8s DEL "account:tenant:map" > /dev/null
    ok "Deleted: account:tenant:map"
  else
    info "account:tenant:map not present"
  fi

  # MT5 trade monitor cursors — reset to 30 days ago so TradeMonitor replays recent deals
  CURSOR_TIME=$(date -u -v-30d +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || date -u -d "30 days ago" +"%Y-%m-%dT%H:%M:%SZ")
  CURSOR_TTL=$((30 * 24 * 3600))
  time_keys=$(redis_k8s KEYS "trade:monitor:last_time:*" || true)
  if [[ -n "$time_keys" ]]; then
    while IFS= read -r key; do
      [[ -z "$key" ]] && continue
      # Extract service_id from key and reset both time + deal cursors
      svc="${key##*:}"
      redis_k8s SET "$key" "$CURSOR_TIME" EX "$CURSOR_TTL" > /dev/null
      redis_k8s SET "trade:monitor:last_deal:${svc}" "0" EX "$CURSOR_TTL" > /dev/null
      ok "Reset cursor for service ${svc} → ${CURSOR_TIME}"
    done <<< "$time_keys"
  else
    info "No trade monitor cursor keys found (will default to now-24h on next start)"
  fi

  # NATS publish dedup hash
  if redis_k8s EXISTS "trade:queue:dedup" | grep -q "^1"; then
    redis_k8s DEL "trade:queue:dedup" > /dev/null
    ok "Deleted: trade:queue:dedup"
  else
    info "trade:queue:dedup not present"
  fi
else
  warn "Skipped Redis cleanup — no Redis pod found"
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
printf "\n${BOLD}${GREEN}Done.${RESET} Scheduler will recalculate rebates on the next cron tick (~2 min).\n\n"
