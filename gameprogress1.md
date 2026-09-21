# KeySlaught progress 1 - project foundation

Date: 2026-09-21

## Confirmed direction

- Primary target: desktop WebGL with Windows 64-bit for local testing.
- Layout: near-square/slightly portrait so the top HUD and typing/refresh area do not crush the playfield.
- Unity: 6000.3.11f1, Universal 2D (URP).
- Repository: public `eziokittu/keyslaught`.
- First game milestone: complete endless-mode mechanics before tutorial, lore levels, or menu polish.
- Player input: a matching letter fires immediately. The magazine concept is now an error/refresh buffer used to recover from wrong or unusable letters, not a normal ammunition reload.

## Completed foundation

- Created and registered the Unity project from the Universal 2D template.
- Selected WebGL as the active build target; WebGL and Windows build support are installed.
- Installed Unity Pipeline and project-pinned Codex MCP integration for live Editor control.
- Added runtime, editor, EditMode-test, and PlayMode-test assembly boundaries.
- Added repository ignore/LFS/YAML-merge rules while excluding the original assignment PDF and machine-local Codex files.
- Documented the endless-mode gameplay contract and staged implementation order in `docs/GAMEPLAY_CONTRACT.md`.
- Created and pushed the public repository: https://github.com/eziokittu/keyslaught

## Verification evidence

- Unity script compilation: passed with no compiler errors.
- Unity project source-integrity scan: passed with zero findings across missing metadata, orphan metadata, duplicate GUIDs, merge markers, package-manifest validity, and editor-version drift.
- WebGL player build: succeeded with 0 errors; 12,534,745-byte output containing `index.html` and compressed `.wasm`, `.data`, and framework files.
- Windows 64-bit player build: succeeded with 0 errors; 101,570,399-byte output containing `KeySlaught.exe` and its player data.
- Git: `main` is synchronized with `origin/main`; Unity pre-commit integrity hooks and UnityYAMLMerge are configured.

The two builds currently contain the blank Universal 2D sample scene. They prove the source/toolchain/build foundation, not gameplay completion or a visual playtest.

## Architecture decision

Gameplay will use scene/prefab-authored objects with serialized Inspector references. ScriptableObjects hold immutable definitions; runtime state remains in runtime models/components. UI and presentation consume events/public methods and remain replaceable. The project will not depend on a hidden runtime bootstrap that generates the final hierarchy.

## Next implementation slice

Build pure, tested mechanics for sequential word damage, closest-to-library valid targeting, wrong-letter buffer/refresh, and library damage before connecting them to scene presentation.
