#!/bin/sh
set -eu

: "${POSTGRES_DB:?POSTGRES_DB is required}"
: "${POSTGRES_USER:?POSTGRES_USER is required}"
: "${POSTGRES_PASSWORD:?POSTGRES_PASSWORD is required}"

BACKUP_RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-30}"

while true; do
  TIMESTAMP="$(date +%Y%m%d_%H%M%S)"
  BACKUP_FILE="/backups/backup_${TIMESTAMP}.sql.gz"

  echo "Creating backup: ${BACKUP_FILE}"
  PGPASSWORD="$POSTGRES_PASSWORD" pg_dump -h postgres -U "$POSTGRES_USER" "$POSTGRES_DB" | gzip > "$BACKUP_FILE"

  echo "Removing backups older than ${BACKUP_RETENTION_DAYS} days"
  find /backups -name 'backup_*.sql.gz' -mtime "+${BACKUP_RETENTION_DAYS}" -delete

  sleep 86400
done
