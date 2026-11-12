#!/usr/bin/env bash
set -euo pipefail
repo_dir="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
project="$repo_dir/src/Molla.Linux/Molla.Linux.csproj"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Molla requires the .NET 10 SDK. See docs/SETUP.md." >&2
  exit 1
fi

dotnet run --project "$project" -c Release -- --apple "$@"
