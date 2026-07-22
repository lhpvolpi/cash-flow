#!/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/../../src/CashFlow.Consolidation.Infrastructure"
STARTUP_PROJECT="$SCRIPT_DIR/../../src/CashFlow.Consolidation.Web"
CONTEXT="ApplicationDbContext"

dotnet ef migrations remove \
  --project "$PROJECT" \
  --startup-project "$STARTUP_PROJECT" \
  --context "$CONTEXT"