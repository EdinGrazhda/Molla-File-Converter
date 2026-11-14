#!/usr/bin/env bash
set -euo pipefail

script_dir="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
project="$script_dir/Molla.Linux/Molla.Linux.csproj"
install_dir="${XDG_DATA_HOME:-$HOME/.local/share}/molla"
applications_dir="${XDG_DATA_HOME:-$HOME/.local/share}/applications"
icons_dir="${XDG_DATA_HOME:-$HOME/.local/share}/icons/hicolor/scalable/apps"
desktop_dir="${XDG_DESKTOP_DIR:-$HOME/Desktop}"

case "$(uname -m)" in
  x86_64|amd64) rid=linux-x64 ;;
  aarch64|arm64) rid=linux-arm64 ;;
  *) echo "Unsupported Linux architecture: $(uname -m)" >&2; exit 1 ;;
esac

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Install the .NET 10 SDK to build and install Molla." >&2
  exit 1
fi

publish_dir="$(mktemp -d)"
trap 'rm -rf "$publish_dir"' EXIT

dotnet publish "$project" -c Release -r "$rid" --self-contained true -o "$publish_dir/app"

mkdir -p "$install_dir" "$applications_dir" "$icons_dir"
rm -rf "$install_dir"/* "$install_dir"/.[!.]* "$install_dir"/..?* 2>/dev/null || true
cp -a "$publish_dir/app/." "$install_dir/"
install -m 0644 "$script_dir/molla-apple.svg" "$icons_dir/molla-apple.svg"

python3 - "$install_dir/Molla" "$applications_dir/molla-apple.desktop" "$desktop_dir/Molla Apple.desktop" <<'PY'
import pathlib
import subprocess
import sys

executable, menu_entry, desktop_entry = sys.argv[1:]
content = "\n".join((
    "[Desktop Entry]",
    "Version=1.0",
    "Type=Application",
    "Name=Molla Apple",
    "Comment=Drop files onto an apple-shaped desktop converter",
    'Exec="' + executable.replace("\\", "\\\\").replace('"', '\\"') + '" --apple',
    "Icon=molla-apple",
    "Terminal=false",
    "Categories=Utility;",
    "StartupNotify=true",
    "",
))
for target in (menu_entry, desktop_entry):
    path = pathlib.Path(target)
    if target == desktop_entry and not path.parent.is_dir():
        continue
    path.write_text(content, encoding="utf-8")
    path.chmod(0o755)
    if target == desktop_entry:
        subprocess.run(
            ["gio", "set", str(path), "metadata::trusted", "true"],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            check=False,
        )
PY

if command -v update-desktop-database >/dev/null 2>&1; then
  update-desktop-database "$applications_dir" >/dev/null 2>&1 || true
fi

echo "Molla is installed in: $install_dir"
echo "Open it from your applications menu or the Molla Apple shortcut on your desktop."
