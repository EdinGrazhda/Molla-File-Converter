# Contributing to Molla

The active converter lives in `src/Molla.Linux`.
The repository contains the Avalonia converter and its focused verification project.

## Build and check

From the outer repository root, with the .NET 10 SDK installed:

```sh
dotnet build src/Molla.Verify/Molla.Verify.csproj -c Release
dotnet src/Molla.Verify/bin/Release/net10.0/Molla.Verify.dll
```

For the UI smoke test, use a graphical GNOME session with the Molla opening
shortcut registered as described in [setup](docs/SETUP.md):

```sh
FONTCONFIG_FILE="$PWD/src/Molla.Linux/fonts.conf" \
  dotnet src/Molla.Verify/bin/Release/net10.0/Molla.Verify.dll --ui
```

The UI test briefly opens windows, generates previews in `/tmp`, saves the
current shortcut unchanged, checks that Alt+F4 is rejected as a conflict, and
converts a generated image through the drop handler. It cleans its own test
input/output files. It does not validate a physical keyboard shortcut press or
a physical file-manager drag; check those manually.

## Changes

- Preserve the apple silhouette, both views, and readable contrast across all tints.
- Keep transparent hit areas large without enlarging visible chrome beyond the apple.
- Never overwrite input files or existing exports.
- Keep conversions local and cancellation safe.
- Keep conversion-core changes narrow and covered by verification.
- Include before/after screenshots for visible UI changes.
- Treat GNOME global shortcuts and compositor blur as platform-dependent capabilities.

Report failures with Linux distribution, desktop/compositor, .NET version,
input and target format, steps to reproduce, and the error message.
Use a generated or non-sensitive sample when sharing files.
