#!/usr/bin/env bash
# compare_rebate.sh — Compare scheduler (year-partitioned) vs mono rebate for a given TradeRebate ID.
#
# Usage:
#   ./scripts/compare_rebate.sh <new_trade_rebate_id> [year]
#   ./scripts/compare_rebate.sh 23
#   ./scripts/compare_rebate.sh 23 2026
#
# Requirements: psql, jq

set -euo pipefail

NEW_ID="${1:?Usage: $0 <new_trade_rebate_id> [year]}"
YEAR="${2:-$(date +%Y)}"

# ── Colors ────────────────────────────────────────────────────────────────────
GREEN="\033[32m"; RED="\033[31m"; YELLOW="\033[33m"
CYAN="\033[36m"; BOLD="\033[1m"; RESET="\033[0m"
ok()     { printf "${GREEN}✅${RESET}"; }
fail()   { printf "${RED}❌${RESET}"; }
warn()   { printf "${YELLOW}⚠${RESET}"; }
header() { printf "\n${BOLD}${CYAN}%s\n%s\n%s${RESET}\n" \
             "$(printf '=%.0s' $(seq 1 60))" "$1" "$(printf '=%.0s' $(seq 1 60))"; }

# ── Load .env ─────────────────────────────────────────────────────────────────
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"
[[ -f "$ENV_FILE" ]] || { echo "Error: .env not found at $ENV_FILE" >&2; exit 1; }

while IFS='=' read -r key rest; do
  [[ "$key" =~ ^[[:space:]]*# ]] && continue
  [[ -z "${key// /}" ]] && continue
  val="${rest%%#*}"
  val="${val#"${val%%[! ]*}"}"
  val="${val%"${val##*[! ]}"}"
  val="${val%\"*}"; val="${val#\"}"; val="${val%\'*}"; val="${val#\'}"
  export "${key// /}=$val" 2>/dev/null || true
done < "$ENV_FILE"

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USERNAME="${DB_USERNAME:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-}"
TENANT_PREFIX="${TENANT_DB_NAME_PREFIX:-portal_tenant_}"

export PGPASSWORD="$DB_PASSWORD"

# ── psql helpers ──────────────────────────────────────────────────────────────
# Trim leading/trailing whitespace (preserves internal spaces)
trim() { local s="$1"; s="${s#"${s%%[! ]*}"}"; s="${s%"${s##*[! ]}"}"; printf '%s' "$s"; }

psql_q() {
  local db="$1"; shift
  psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USERNAME" -d "$db" -tA --field-separator $'\x01' -c "$1" 2>/dev/null || true
}
psql1() { psql_q "$1" "$2" | head -1; }

# Read psql output (SOH-delimited) into an array of lines
read_rows() {
  local db="$1" sql="$2"
  psql_q "$db" "$sql"
}

# Extract a single field (1-indexed) from a SOH-delimited row
field() { printf '%s' "$1" | cut -d$'\x01' -f"$2"; }

# ── Find tenant DB ────────────────────────────────────────────────────────────
echo "Searching _TradeRebate_${YEAR} id=${NEW_ID} across tenant databases..."
TENANT_DB=""
while IFS=$'\x01' read -r db _; do
  db="$(trim "$db")"
  [[ -z "$db" ]] && continue
  found=$(psql1 "$db" "SELECT 1 FROM trd.\"_TradeRebate_${YEAR}\" WHERE \"Id\" = ${NEW_ID} LIMIT 1")
  if [[ "$(trim "$found")" == "1" ]]; then
    TENANT_DB="$db"; break
  fi
done < <(psql_q postgres "SELECT datname FROM pg_database WHERE datname LIKE '${TENANT_PREFIX}%' ORDER BY datname")

[[ -n "$TENANT_DB" ]] || { echo "Error: id=${NEW_ID} not found in any tenant DB." >&2; exit 1; }
printf "Found in: ${BOLD}%s${RESET}\n" "$TENANT_DB"

q()  { psql_q  "$TENANT_DB" "$1"; }
q1() { psql1   "$TENANT_DB" "$1"; }

# ── Fetch new TradeRebate ─────────────────────────────────────────────────────
NEW_TR=$(q1 "SELECT \"Id\",\"AccountId\",\"Ticket\",\"AccountNumber\",\"Symbol\",\"Volume\",
                    \"Status\",\"ClosedOn\",\"OpenedOn\",\"TradeServiceId\",\"ReferPath\"
             FROM trd.\"_TradeRebate_${YEAR}\" WHERE \"Id\" = ${NEW_ID}")
[[ -n "$NEW_TR" ]] || { echo "Error: record not found." >&2; exit 1; }

N_ID=$(field "$NEW_TR" 1); N_ACCT=$(field "$NEW_TR" 2); N_TICKET=$(field "$NEW_TR" 3)
N_ACCNUM=$(field "$NEW_TR" 4); N_SYM=$(field "$NEW_TR" 5); N_VOL=$(field "$NEW_TR" 6)
N_STATUS=$(field "$NEW_TR" 7); N_CLOSED=$(field "$NEW_TR" 8); N_OPENED=$(field "$NEW_TR" 9)
N_SVC=$(field "$NEW_TR" 10); N_PATH=$(field "$NEW_TR" 11)

# ── Fetch old TradeRebate by Ticket + AccountNumber ───────────────────────────
OLD_TR=$(q1 "SELECT \"Id\",\"AccountId\",\"Ticket\",\"AccountNumber\",\"Symbol\",\"Volume\",
                    \"Status\",\"ClosedOn\",\"OpenedOn\",\"TradeServiceId\",\"ReferPath\"
             FROM trd.\"_TradeRebate\"
             WHERE \"Ticket\" = $(trim "$N_TICKET") AND \"AccountNumber\" = $(trim "$N_ACCNUM")
             ORDER BY \"Id\" LIMIT 1")

if [[ -n "$OLD_TR" ]]; then
  O_ID=$(field "$OLD_TR" 1); O_ACCT=$(field "$OLD_TR" 2); O_TICKET=$(field "$OLD_TR" 3)
  O_ACCNUM=$(field "$OLD_TR" 4); O_SYM=$(field "$OLD_TR" 5); O_VOL=$(field "$OLD_TR" 6)
  O_STATUS=$(field "$OLD_TR" 7); O_CLOSED=$(field "$OLD_TR" 8); O_OPENED=$(field "$OLD_TR" 9)
  O_SVC=$(field "$OLD_TR" 10); O_PATH=$(field "$OLD_TR" 11)
else
  O_ID=""; O_ACCT="(未找到)"; O_TICKET="-"; O_ACCNUM="-"; O_SYM="-"; O_VOL="-"
  O_STATUS="-"; O_CLOSED="-"; O_OPENED="-"; O_SVC="-"; O_PATH="-"
fi

# ── Compare helper ────────────────────────────────────────────────────────────
cmp_row() {
  local label="$1" nv="$(trim "$2")" ov="$(trim "$3")" note="${4:-}"
  local status
  if [[ -n "$note" ]]; then
    status="$(warn) $note"
  elif [[ "$nv" == "$ov" ]]; then
    status="$(ok)"
  else
    status="$(fail)"
  fi
  printf "  %-20s %-36s %-36s %b\n" "$label" "$nv" "$ov" "$status"
}

# ── Section 1: TradeRebate source comparison ──────────────────────────────────
header "TradeRebate 源数据对比  (新 id=${NEW_ID}, Ticket=$(trim "$N_TICKET"))"
printf "  %-20s %-36s %-36s %s\n" "字段" "新表 _TradeRebate_${YEAR}" "旧表 _TradeRebate" "一致"
printf "  %s\n" "$(printf '─%.0s' $(seq 1 100))"
cmp_row "Id"             "$N_ID"     "$O_ID"     "IDs 不同（设计如此）"
cmp_row "AccountId"      "$N_ACCT"   "$O_ACCT"
cmp_row "Ticket"         "$N_TICKET" "$O_TICKET"
cmp_row "AccountNumber"  "$N_ACCNUM" "$O_ACCNUM"
cmp_row "Symbol"         "$N_SYM"    "$O_SYM"
cmp_row "Volume"         "$N_VOL"    "$O_VOL"
cmp_row "Status"         "$N_STATUS" "$O_STATUS"
cmp_row "ClosedOn"       "$N_CLOSED" "$O_CLOSED"
cmp_row "OpenedOn"       "$N_OPENED" "$O_OPENED"
cmp_row "TradeServiceId" "$N_SVC"    "$O_SVC"

# ── Fetch Rebate records (store as indexed arrays) ────────────────────────────
NEW_REBATES=()
while IFS= read -r line; do
  [[ -n "$line" ]] && NEW_REBATES+=("$line")
done < <(q "SELECT r.\"Id\",r.\"AccountId\",r.\"Amount\",m.\"StateId\",r.\"Information\"
  FROM trd.\"_Rebate_${YEAR}\" r
  JOIN core.\"_Matter_${YEAR}\" m ON r.\"Id\" = m.\"Id\"
  WHERE r.\"TradeRebateId\" = ${NEW_ID} ORDER BY r.\"Id\"" 2>/dev/null)

OLD_REBATES=()
if [[ -n "$O_ID" ]]; then
  while IFS= read -r line; do
    [[ -n "$line" ]] && OLD_REBATES+=("$line")
  done < <(q "SELECT r.\"Id\",r.\"AccountId\",r.\"Amount\",m.\"StateId\",r.\"Information\"
    FROM trd.\"_Rebate\" r
    JOIN core.\"_Matter\" m ON r.\"Id\" = m.\"Id\"
    WHERE r.\"TradeRebateId\" = $(trim "$O_ID") ORDER BY r.\"Id\"" 2>/dev/null)
fi

N_CNT="${#NEW_REBATES[@]}"
O_CNT="${#OLD_REBATES[@]}"
MAX_ROWS=$(( N_CNT > O_CNT ? N_CNT : O_CNT ))

# ── Section 2: Rebate summary ─────────────────────────────────────────────────
header "Rebate 记录汇总对比  (新 TradeRebateId=${NEW_ID} / 旧 TradeRebateId=$(trim "${O_ID:--}"))"
printf "  %-4s %-12s %-20s %-20s %-14s %-14s %s\n" \
  "#" "AccountId" "Amount (新)" "Amount (旧)" "StateId (新)" "StateId (旧)" "金额"
printf "  %s\n" "$(printf '─%.0s' $(seq 1 100))"

ALL_AMT_MATCH=true
for (( i=0; i<MAX_ROWS; i++ )); do
  NR="${NEW_REBATES[$i]:-}"
  OR="${OLD_REBATES[$i]:-}"

  N_RACCT=$(field "$NR" 2); N_AMT=$(field "$NR" 3); N_STATE=$(field "$NR" 4)
  O_RACCT=$(field "$OR" 2); O_AMT=$(field "$OR" 3); O_STATE=$(field "$OR" 4)

  N_RACCT="$(trim "${N_RACCT:--}")"; N_AMT="$(trim "${N_AMT:--}")"; N_STATE="$(trim "${N_STATE:--}")"
  O_RACCT="$(trim "${O_RACCT:--}")"; O_AMT="$(trim "${O_AMT:--}")"; O_STATE="$(trim "${O_STATE:--}")"

  # Numeric compare (strip trailing zeros after decimal)
  strip_zeros() { echo "$1" | sed 's/\.\([0-9]*[1-9]\)0*$/.\1/;s/\.0*$//'; }
  N_AMT_N="$(strip_zeros "$N_AMT")"; O_AMT_N="$(strip_zeros "$O_AMT")"

  if [[ "$N_AMT_N" == "$O_AMT_N" ]]; then
    AMT_STATUS="$(ok)"
  else
    AMT_STATUS="$(fail)"
    ALL_AMT_MATCH=false
  fi

  printf "  %-4s %-12s %-20s %-20s %-14s %-14s %b\n" \
    "$((i+1))" "$N_RACCT" "$N_AMT" "$O_AMT" "$N_STATE" "$O_STATE" "$AMT_STATUS"
done

# ── Section 3: Information JSON per depth ─────────────────────────────────────
header "Information JSON 字段对比"

jqv() { printf '%s' "$1" | jq -r "$2" 2>/dev/null || echo ""; }

cmp_json() {
  local label="$1" nv="$2" ov="$3" is_price="${4:-}"
  # Numeric strip trailing .0 / .00 etc
  local nv_n ov_n
  nv_n=$(echo "$nv" | sed 's/\.\([0-9]*[1-9]\)0*$/.\1/;s/\.0*$//')
  ov_n=$(echo "$ov" | sed 's/\.\([0-9]*[1-9]\)0*$/.\1/;s/\.0*$//')
  local status
  if [[ -n "$is_price" ]]; then
    status="$(warn) 实时价格不同"
  elif [[ "$nv_n" == "$ov_n" ]]; then
    status="$(ok)"
  else
    status="$(fail)"
  fi
  printf "  %-34s %-28s %-28s %b\n" "$label" "$nv" "$ov" "$status"
}

for (( i=0; i<MAX_ROWS; i++ )); do
  NR="${NEW_REBATES[$i]:-}"
  OR="${OLD_REBATES[$i]:-}"

  # Information is the 5th field (index 4); may contain SOH inside JSON — use cut
  N_INFO=$(printf '%s' "$NR" | cut -d$'\x01' -f5)
  O_INFO=$(printf '%s' "$OR" | cut -d$'\x01' -f5)

  printf "\n  ${BOLD}── Information (depth %s) ──${RESET}\n" "$((i+1))"
  printf "  %-34s %-28s %-28s %s\n" "字段" "新 scheduler" "旧 mono" "一致"
  printf "  %s\n" "$(printf '─%.0s' $(seq 1 95))"

  cmp_json "version"      "$(jqv "$N_INFO" '.version')"      "$(jqv "$O_INFO" '.version')"
  [[ -n "$(jqv "$N_INFO" '.depth')" ]] && \
    cmp_json "depth"      "$(jqv "$N_INFO" '.depth')"        "$(jqv "$O_INFO" '.depth')"
  cmp_json "exchangeRate" "$(jqv "$N_INFO" '.exchangeRate')" "$(jqv "$O_INFO" '.exchangeRate')"

  printf "  ${BOLD}baseRebate:${RESET}\n"
  cmp_json "  .rate"       "$(jqv "$N_INFO" '.baseRebate.rate')"       "$(jqv "$O_INFO" '.baseRebate.rate')"
  cmp_json "  .pip"        "$(jqv "$N_INFO" '.baseRebate.pip')"        "$(jqv "$O_INFO" '.baseRebate.pip')"
  cmp_json "  .commission" "$(jqv "$N_INFO" '.baseRebate.commission')" "$(jqv "$O_INFO" '.baseRebate.commission')"
  cmp_json "  .price"      "$(jqv "$N_INFO" '.baseRebate.price')"      "$(jqv "$O_INFO" '.baseRebate.price')" "price"

  # remainRebate
  RN_TYPE=$(jqv "$N_INFO" '.remainRebate | type')
  RO_TYPE=$(jqv "$O_INFO" '.remainRebate | type')
  if [[ "$RN_TYPE" == "object" || "$RO_TYPE" == "object" ]]; then
    printf "  ${BOLD}remainRebate:${RESET}\n"
    cmp_json "  .rate"       "$(jqv "$N_INFO" '.remainRebate.rate')"       "$(jqv "$O_INFO" '.remainRebate.rate')"
    cmp_json "  .pip"        "$(jqv "$N_INFO" '.remainRebate.pip')"        "$(jqv "$O_INFO" '.remainRebate.pip')"
    cmp_json "  .commission" "$(jqv "$N_INFO" '.remainRebate.commission')" "$(jqv "$O_INFO" '.remainRebate.commission')"
  elif [[ "$RN_TYPE" == "number" || "$RO_TYPE" == "number" ]]; then
    printf "  ${BOLD}remainRebate:${RESET}\n"
    cmp_json "  (scalar)" "$(jqv "$N_INFO" '.remainRebate')" "$(jqv "$O_INFO" '.remainRebate')"
  fi

  # allocationSchemaItem
  if [[ "$(jqv "$N_INFO" '.allocationSchemaItem | type')" == "object" ]] || \
     [[ "$(jqv "$O_INFO" '.allocationSchemaItem | type')" == "object" ]]; then
    printf "  ${BOLD}allocationSchemaItem:${RESET}\n"
    cmp_json "  .cid" "$(jqv "$N_INFO" '.allocationSchemaItem.cid')" "$(jqv "$O_INFO" '.allocationSchemaItem.cid')"
    cmp_json "  .r"   "$(jqv "$N_INFO" '.allocationSchemaItem.r')"   "$(jqv "$O_INFO" '.allocationSchemaItem.r')"
  fi

  # allocationRebate
  if [[ "$(jqv "$N_INFO" '.allocationRebate | type')" == "object" ]] || \
     [[ "$(jqv "$O_INFO" '.allocationRebate | type')" == "object" ]]; then
    printf "  ${BOLD}allocationRebate:${RESET}\n"
    cmp_json "  .total" "$(jqv "$N_INFO" '.allocationRebate.total')" "$(jqv "$O_INFO" '.allocationRebate.total')"
  fi

  # rebateDirectSchemaItem (Direct mode)
  if [[ "$(jqv "$N_INFO" '.rebateDirectSchemaItem | type')" == "object" ]] || \
     [[ "$(jqv "$O_INFO" '.rebateDirectSchemaItem | type')" == "object" ]]; then
    printf "  ${BOLD}rebateDirectSchemaItem:${RESET}\n"
    cmp_json "  .rate"       "$(jqv "$N_INFO" '.rebateDirectSchemaItem.rate')"       "$(jqv "$O_INFO" '.rebateDirectSchemaItem.rate')"
    cmp_json "  .pips"       "$(jqv "$N_INFO" '.rebateDirectSchemaItem.pips')"       "$(jqv "$O_INFO" '.rebateDirectSchemaItem.pips')"
    cmp_json "  .commission" "$(jqv "$N_INFO" '.rebateDirectSchemaItem.commission')" "$(jqv "$O_INFO" '.rebateDirectSchemaItem.commission')"
    cmp_json "  .symbolCode" "$(jqv "$N_INFO" '.rebateDirectSchemaItem.symbolCode')" "$(jqv "$O_INFO" '.rebateDirectSchemaItem.symbolCode')"
  fi

  # note (last allocation depth)
  N_NOTE=$(jqv "$N_INFO" '.note // empty')
  O_NOTE=$(jqv "$O_INFO" '.note // empty')
  if [[ -n "$N_NOTE" || -n "$O_NOTE" ]]; then
    cmp_json "note" "$N_NOTE" "$O_NOTE"
  fi
done

# ── Summary ───────────────────────────────────────────────────────────────────
header "汇总"
if [[ "$N_CNT" == "$O_CNT" ]]; then
  printf "  Rebate 条数: 新=%s, 旧=%s  %b 一致\n"    "$N_CNT" "$O_CNT" "$(ok)"
else
  printf "  Rebate 条数: 新=%s, 旧=%s  %b 不一致\n"  "$N_CNT" "$O_CNT" "$(fail)"
fi

if $ALL_AMT_MATCH; then
  printf "  金额:        %b 全部一致\n" "$(ok)"
else
  printf "  金额:        %b 存在差异\n" "$(fail)"
fi
echo
