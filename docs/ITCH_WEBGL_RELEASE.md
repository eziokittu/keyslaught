# KeySlaught itch.io WebGL release

## Build

1. Open the project in Unity 6000.3.11f1.
2. Run `KeySlaught > Build > WebGL for itch.io`.
3. Wait for Unity to report `Build Successful`.
4. Upload only `Builds/WebGL/KeySlaught-itch-upload.zip` to itch.io.

The build command creates a clean WebGL player, removes the web-server
`Content-Encoding` dependency from Unity's generated payloads, and writes a
portable ZIP whose `index.html` is at the archive root.

Do not upload `build2.zip`, a ZIP containing a top-level `build2/` folder, or
one of the older `.br` builds.

## itch.io settings

- Project kind: HTML
- Upload: mark `KeySlaught-itch-upload.zip` as the file that will be played in
  the browser.
- Embed: use an embedded page/frame, set the viewport to **576 px wide by
  1024 px high**, and allow fullscreen.
- The generated page preserves the 9:16 portrait aspect ratio if the browser
  window is smaller. A short, landscape-shaped itch frame will therefore add
  side space instead of stretching or cropping the game.
- After replacing a broken upload, reload the game page without cache or test
  in a private window so the old Unity data cache is not reused.

## Verification boundary

A successful Unity build and local HTTP checks prove that the package is
complete and serveable. The uploaded itch.io page still needs one final visual
browser check after the new ZIP is live.
