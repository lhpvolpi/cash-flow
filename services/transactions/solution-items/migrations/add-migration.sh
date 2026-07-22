#!/bin/bash

set -e

if [ -z "$1" ]; then
  echo "Usage: ./add-migration.sh <MigrationName>"
  exit 1
fi

MIGRATION_NAME=$1
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/../../src/CashFlow.Transactions.Infrastructure"
STARTUP_PROJECT="$SCRIPT_DIR/../../src/CashFlow.Transactions.Web"
CONTEXT="ApplicationDbContext"

dotnet ef migrations add "$MIGRATION_NAME" \
  --project "$PROJECT" \
  --startup-project "$STARTUP_PROJECT" \
  --context "$CONTEXT"