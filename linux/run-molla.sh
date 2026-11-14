#!/usr/bin/env bash
set -euo pipefail
molla_root="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
molla_dll="$molla_root/linux/Molla.Linux/bin/Release/net10.0/Molla.dll"
if [[ ! -f "$molla_dll" ]]; then
  echo "Build Molla first: dotnet build \"$molla_root/linux/Molla.Linux/Molla.Linux.csproj\" -c Release" >&2
  exit 1
fi
if [[ -x "$molla_root/.dotnet/dotnet" ]]; then
  exec "$molla_root/.dotnet/dotnet" "$molla_dll" "$@"
elif command -v dotnet >/dev/null 2>&1; then
  exec dotnet "$molla_dll" "$@"
else
  echo "Molla requires the .NET 10 runtime. Install it or provide nashi-app/.dotnet/dotnet." >&2
  exit 1
fi
