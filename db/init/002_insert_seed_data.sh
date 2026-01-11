#!/bin/bash
set -e

if [ "$INSERT_SEED" = "true" ]; then
  for f in /docker-entrypoint-initdb.d/seeds/*.sql; do
    [ -e "$f" ] || continue  # skip if no files
    echo "Running seed $f..."
    psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB" < "$f"
  done
else
  echo "Skipping seed data"
fi
