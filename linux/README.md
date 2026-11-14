# Molla for Linux

Molla's active converter is the apple-shaped Avalonia app in `Molla.Linux/`.
It reuses the Nashi .NET engine and keeps file conversion local.

From `nashi-app/`, with .NET 10 SDK installed:

```sh
dotnet build linux/Molla.Linux/Molla.Linux.csproj -c Release
bash linux/run-molla.sh --apple
```

The launcher prefers `.dotnet/dotnet` in this repository, then system `dotnet`.
Without `--apple`, it opens the earlier catalog workspace.

## Current apple

- Drop files onto a format, or select them with **Add files** first.
- Drag the title to move the expanded apple; drag the mini apple's body to move it.
- **−** collapses; clicking the mini apple expands; **×** closes.
- Dropping files on the mini apple expands with those files selected.
- Droplet menu: **Colors** (red, green, yellow, gray) and **Shortcut**.
- The GNOME shortcut editor changes an existing Molla opening binding and rejects
  recognized desktop/custom conflicts. Registration is required on new machines.
- Outputs go to `~/Downloads/Molla`; **Open exports** opens that directory.
- PNG, JPG, WebP, AVIF, TIFF, BMP; MP3, WAV, FLAC, M4A, OGG, Opus;
  MP4, MOV, MKV, WebM, AVI, WMV. Audio/video require FFmpeg and FFprobe.
- Image exports use single-frame decoding and remove metadata.
- Colors are session-only. Transparent glass is supported; actual blur depends
  on the compositor. The tested GNOME session provides transparency, not blur.

The app uses a separate fontconfig cache under `~/.cache/molla/fontconfig`
(or the corresponding XDG cache path). An explicit `FONTCONFIG_FILE` is respected.

See the outer [README](../../README.md), [setup](../../docs/SETUP.md),
[contribution guide](../../CONTRIBUTING.md), and [publication guide](../../docs/PUBLISHING.md).
The inherited `../README.md` describes upstream Nashi, not Molla's current Linux UI.
