# KeySlaught — Game Progress 21

Date: 2026-09-27  
Scope: itch.io WebGL portrait sizing correction

## 1. Root cause

The Unity WebGL player settings were still `960 x 600`, so the generated
desktop canvas used a landscape frame. itch.io displayed that frame correctly,
but the portrait splash and UI were cropped inside it.

## 2. Fix

- WebGL default width and height are now `576 x 1024`.
- The itch build command enforces those dimensions before future builds.
- The generated WebGL page adds an explicit viewport declaration.
- The canvas preserves the `9:16` aspect ratio and scales down uniformly when
  the browser or itch frame is smaller.
- The default Unity footer is hidden so it cannot make the portrait embed
  taller than the configured game viewport.
- `docs/ITCH_WEBGL_RELEASE.md` now records the exact itch viewport settings.

## 3. Verification completed

- Unity Editor script compilation completed successfully.
- The refreshed ZIP has 17 entries and a root `index.html`.
- The ZIP contains a `576 x 1024` canvas, viewport metadata, and responsive
  `9:16` CSS.
- The ZIP has no wrapper folder, backslash entry names, `.br`, or `.gz`
  payloads.
- Local HTTP checks returned 200 for HTML, CSS, loader, framework, data, and
  Wasm; the Wasm MIME type is `application/wasm`.
- Final ZIP size: 27,815,066 bytes.
- Final ZIP SHA-256:
  `BEB0C1AAEBFD5CED09842C75C9573B35135E230C28FBE8429002BA254F8E59C5`.

## 4. Verification not claimed

- The refreshed ZIP was not uploaded to itch.io.
- The live itch page was not visually tested after upload.
- No commit or push was performed.

## 5. Next action

Replace the itch upload with `Builds/WebGL/KeySlaught-itch-upload.zip`, set the
itch viewport to `576 x 1024`, allow fullscreen, and test once in a private
window.
