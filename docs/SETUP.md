# Setup and troubleshooting

## Build

Install the .NET 10 SDK for your Linux distribution. Build from the outer Molla directory:

```sh
dotnet restore src/Molla.Linux/Molla.Linux.csproj
dotnet build src/Molla.Linux/Molla.Linux.csproj -c Release --no-restore
./run-molla.sh
```

FFmpeg and FFprobe must both be on PATH to enable audio/video outputs. The app
does not install them. A graphical desktop and Avalonia's native Linux dependencies
are required. The development setup is Zorin/GNOME on Linux; other desktops need
their own testing.

## Desktop launcher

Use `run-molla.sh` as the command for an applications-menu or desktop shortcut.
The scalable icon is `docs/assets/brand/molla-icon.svg`. The application embeds
`src/Molla.Linux/Assets/molla-icon.png`; rebuild after changing that asset.

## Keyboard opening shortcut

On this development machine, **Ctrl + Alt + M** launches the apple.
**Alt + F4** closes the focused window. Closing during an active conversion
requests cancellation; close again when it finishes.

For a fresh GNOME/Zorin setup, register the shortcut below. Replace the command's
absolute path with your own clone path. Check that Ctrl+Alt+M is unused first.
The Python helper requires the system PyGObject package and preserves other
custom shortcuts:

```sh
python3 - <<'PY'
from gi.repository import Gio
path = "/org/gnome/settings-daemon/plugins/media-keys/custom-keybindings/molla/"
settings = Gio.Settings.new_with_path(
    "org.gnome.settings-daemon.plugins.media-keys.custom-keybinding", path)
settings.set_string("name", "Open Molla Apple")
settings.set_string("command", "/bin/bash /absolute/path/to/Molla-File-Converter/run-molla.sh")
settings.set_string("binding", "<Control><Alt>m")
parent = Gio.Settings.new("org.gnome.settings-daemon.plugins.media-keys")
existing = parent.get_strv("custom-keybindings")
if path not in existing:
    parent.set_strv("custom-keybindings", existing + [path])
Gio.Settings.sync()
PY
```

Once registered under that exact `molla/` settings path, open the droplet menu,
select **Shortcut**, choose modifiers and a letter or F1–F12, then save.
The editor checks GNOME desktop and other custom bindings for conflicts.
It requires Ctrl, Alt, or Super; bare letters and Shift-only combinations are rejected.
If the shortcut is not registered or settings access fails, the editor shows an error.
Non-GNOME desktops can use their own keyboard settings to launch the same command;
the in-app GNOME editor is not supported there.

## Appearance and moving

Drag the title on the large apple; drag the body on the mini apple.
A click on the mini apple opens it. Dropping local files on it opens the
converter with those files selected. Color changes affect both views, but
are not persisted across app restarts.

The glass tint is always available. True backdrop blur depends on your compositor;
transparency alone on GNOME is expected, not a failed conversion or missing image.

## Troubleshooting

- **Old design opens:** close old windows and launch the rebuilt Release DLL via `--apple`.
- **Build first message:** run the Release build command above.
- **Audio/video disabled:** verify both `ffmpeg -version` and `ffprobe -version`.
- **Font startup stalls:** the app uses its own fontconfig cache. Remove an explicitly
  set `FONTCONFIG_FILE` override if it points at an unsuitable config; do not delete
  unrelated system font caches.
- **Exports:** check `~/Downloads/Molla`. The app creates distinct names rather than
  replacing originals or existing exports.
- **Shortcut does not close the app:** the opening shortcut launches it; use × or
  the desktop close-window shortcut to close it.
