#!/bin/bash
# Sync remote PostgreSQL databases to local

set -e

# Remote DB config
REMOTE_HOST="mm-postgre-db.cbm0wukiuqza.ap-southeast-1.rds.amazonaws.com"
REMOTE_PORT="5432"
REMOTE_USER="postgres"
REMOTE_PASSWORD="auVuEDCji8NJuMeOizxo"

# Local DB config
LOCAL_USER="lucaslee"
LOCAL_PORT="5432"

# Databases to sync
DATABASES=("portal_central" "hangfire" "portal_tenant_bvi" "portal_tenant_au" "portal_tenant_sea")

export PGPASSWORD="$REMOTE_PASSWORD"

for DB in "${DATABASES[@]}"; do
  echo "=============================="
  echo "Syncing: $DB"
  echo "=============================="

  # Drop and recreate local database
  psql -U "$LOCAL_USER" -d postgres -c "DROP DATABASE IF EXISTS $DB;" 2>&1
  psql -U "$LOCAL_USER" -d postgres -c "CREATE DATABASE $DB;" 2>&1

  # Dump remote and restore to local
  pg_dump "host=$REMOTE_HOST port=$REMOTE_PORT dbname=$DB user=$REMOTE_USER sslmode=require" \
    | psql -U "$LOCAL_USER" -d "$DB" 2>&1

  echo "Done: $DB"
  echo ""
done

unset PGPASSWORD
echo "All databases synced successfully."

# Ensure all tenant records exist in _Tenant table (remote is missing au/sea entries)
echo "Ensuring _Tenant records are complete..."
psql -U "$LOCAL_USER" -d portal_central -c "
INSERT INTO core.\"_Tenant\" (\"Id\", \"Name\", \"DatabaseName\")
VALUES
  (1,     'AU Tenant',  'portal_tenant_au'),
  (10000, 'BVI Tenant', 'portal_tenant_bvi'),
  (10004, 'SEA Tenant', 'portal_tenant_sea')
ON CONFLICT (\"Id\") DO NOTHING;
" 2>&1
echo "Tenant records OK."
