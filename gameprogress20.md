# KeySlaught — Game Progress 20

Date: 2026-09-27  
Scope: itch.io WebGL blank-screen diagnosis, release-build hardening, and fresh package

## 1. Root cause

The previous WebGL players used Brotli payloads (`.br`) while Unity's
decompression fallback was disabled. That made startup depend on the host
returning exact `Content-Encoding: br` headers. The HTML shell could load and
show `KeySlaught`, while the framework/data/WebAssembly bootstrap failed.

The old archives also had packaging hazards: `build2.zip` contained an extra
top-level `build2/` folder, while the prior itch upload ZIP stored Windows
backslash entry names.

## 2. Fix

- `Assets/Scripts/Editor/KeySlaughtBuild.cs` now exposes
  `KeySlaught > Build > WebGL for itch.io`.
- The command builds into `Builds/WebGL/KeySlaught-itch`.
- After Unity succeeds, compressed WebGL payloads are expanded to host-neutral
  `.data`, `.framework.js`, and `.wasm` files and `index.html` is rewritten.
- The command creates `Builds/WebGL/KeySlaught-itch-upload.zip` with portable
  `/` separators and `index.html` at the archive root.
- `KeySlaught > Build > Repackage existing WebGL for itch.io` can repair and
  repackage the latest completed output without rebuilding the player.
- Upload instructions are in `docs/ITCH_WEBGL_RELEASE.md`.

## 3. Verification completed

- Unity Editor compilation passed with no C# compiler errors.
- Fresh Unity WebGL player build: `Build Finished, Result: Success`.
- Final ZIP contains 17 entries with root `index.html`.
- No wrapper directory, backslash entry names, `.br`, or `.gz` payloads remain.
- Local HTTP checks returned 200 for the page, loader, framework, data, Wasm,
  and stylesheet.
- MIME checks passed, including `application/wasm` for the Wasm module.
- The 53,447,926-byte Wasm module passed standalone `WebAssembly.compile`.
- `unity projects verify --strict` passed: 734 files scanned, 0 errors,
  0 warnings.
- Final ZIP size: 27,814,827 bytes.
- Final ZIP SHA-256:
  `86B7D261E6E95B85EC75FEB78F1AA680B2CCC811D17C37227AE234F025E85F41`.

## 4. Verification not claimed

- The in-app/external browser connection was unavailable, so visual browser
  rendering and interaction were not observed in this run.
- The repaired ZIP was not uploaded to itch.io.
- The live itch.io page and its CDN/cache behavior were not tested.
- No commit or push was performed.

## 5. Next action

Upload `Builds/WebGL/KeySlaught-itch-upload.zip`, mark it as playable in the
browser, then open the itch.io game in a cache-free/private window and confirm
the launch screen, input, audio, fullscreen, and level loading.
