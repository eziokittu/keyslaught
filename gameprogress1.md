# KeySlaught progress 1 - project foundation

Date: 2026-09-21

## Status verdict

The project foundation is complete, but **gameplay has not been implemented yet**. The current Unity scene and both player builds show the blank Universal 2D template scene. This is expected for milestone 1; it proves that the editor, source layout, automation, repository, and build targets work, not that a playable game exists.

## What is implemented

- Unity 6000.3.11f1 project created from the Universal 2D template.
- Desktop WebGL selected as the primary target, with Windows 64-bit available for local testing.
- Reference viewport set to 900 x 1000 for a near-square, slightly portrait play area.
- Unity CLI, Unity Pipeline, and project-local Codex MCP configuration set up for command-line and live-Editor control.
- Runtime, editor, EditMode-test, and PlayMode-test assembly boundaries created.
- Command-line build automation added for WebGL and Windows.
- Git ignore, Git LFS, UnityYAMLMerge, and pre-commit source-integrity checks configured.
- Endless-mode rules and architecture documented in `docs/GAMEPLAY_CONTRACT.md`.
- Public repository created and pushed: https://github.com/eziokittu/keyslaught

## What is not implemented

- No player character or movement.
- No arena, tilemap, enemy path, library, turret pads, or authored gameplay scene.
- No enemy definitions, spawning, movement, word stacks, waves, or bosses.
- No keyboard combat, target selection, error buffer, refresh action, gun corruption, or shot presentation.
- No library health, run-ending logic, brain-cell currency, pickups, or upgrades.
- No turrets, library abilities, research progression, menus, tutorial, lore levels, audio, or final art.
- No gameplay tests yet. The test assembly definitions exist, but contain no test cases.

## Confirmed gameplay direction

- First playable scope: the full endless-mode mechanics before tutorial, lore levels, or menu polish.
- A correct matching keypress fires immediately at the valid in-range enemy closest to the library; continuous correct typing is not interrupted by a normal magazine reload.
- A wrong or currently unusable letter occupies a bounded error/refresh-buffer slot.
- When that buffer is full, input is blocked until Refresh clears it. Refresh duration initially scales at approximately 0.5 seconds per occupied slot and remains configurable.
- Enemy damage is sequential: `HELLO` becomes `ELLO`, then `LLO`, `LO`, `O`, and defeated.
- An enemy reaching the library deals damage equal to its remaining letters.
- Gameplay is transform-driven, with deterministic target selection and no physics-based bullet trajectory.
- Use scene/prefab-authored objects and serialized Inspector references. ScriptableObjects hold immutable definitions; runtime models/components hold changing state. Do not make the final game depend on an opaque runtime bootstrap.

The complete agreed contract is in `docs/GAMEPLAY_CONTRACT.md`; the original concept remains in `gameidea.txt`.

## Verification evidence for this milestone

- Unity script compilation passed with no compiler errors.
- Source-integrity scan passed with zero findings for missing/orphan metadata, duplicate GUIDs, merge markers, package-manifest validity, and editor-version drift.
- WebGL player build succeeded with 0 errors. Output size: 12,534,745 bytes, including `index.html` and compressed `.wasm`, `.data`, and framework files.
- Windows 64-bit player build succeeded with 0 errors. Output size: 101,570,399 bytes, including `KeySlaught.exe` and player data.
- The last verified Git state was `main` synchronized with `origin/main` at commit `131f1f3c12fa4bb8fa3f2ccce12bd3d37460c4f1`.
- These checks do **not** count as gameplay, Play Mode interaction, visual-quality, or browser-deployment verification.

## Next implementation milestone

Milestone 2 should implement the mechanics as pure, testable C# before building their scene presentation:

1. Sequential enemy-word progress.
2. Valid in-range target filtering and closest-to-library selection.
3. Wrong-letter error buffer, full-buffer input lock, and configurable Refresh.
4. Library damage derived from remaining letters.
5. EditMode tests for normal behavior, ties, empty/no-match cases, capacity boundaries, and refresh timing.

After that milestone is implemented and verified, create `gameprogress2.md`. Do not overwrite this file with milestone 2 status.

## Numbered progress-file convention

- `gameprogress1.md` is the milestone-1 foundation record and should change only to correct factual mistakes.
- Each meaningful future milestone gets a new file: `gameprogress2.md`, `gameprogress3.md`, `gameprogress4.md`, and so on.
- The newest progress file must be self-contained and identify the previous progress file.
- Every progress file must separate completed work from unimplemented work and include requirements/decisions, architecture, changed areas, verification evidence, known risks, next steps, and the exact Git boundary.
- Build success, runtime behavior, visual review, WebGL browser behavior, and deployment are separate verification gates and must be reported separately.
- Do not claim work is committed or pushed without checking the live Git state. Do not push without the user's explicit permission.

## Copy-ready prompt for the next chat

```text
Continue KeySlaught from C:\CodingStuff\GameDev\UnityProjects\KeySlaught.

Read gameidea.txt, docs/GAMEPLAY_CONTRACT.md, and gameprogress1.md, then inspect the live Git and Unity state before changing anything. Milestone 1 contains project/tooling/build/repository setup only; the visible scene is intentionally still the blank Universal 2D template.

Implement milestone 2: pure, tested C# mechanics for sequential enemy-word progress, valid in-range closest-to-library target selection, the wrong-letter error buffer and configurable Refresh behavior, and library damage from remaining letters. Use EditMode tests and keep runtime state separate from ScriptableObject definitions. Preserve the plan for scene/prefab-authored GameObjects with serialized Inspector wiring; do not introduce an opaque runtime-only bootstrap.

Verify compilation and EditMode tests. Then create gameprogress2.md as a self-contained handoff containing completed versus unimplemented work, decisions, architecture, changed files, exact test evidence, risks, next steps, and current Git commit/status. Do not overwrite gameprogress1.md, and do not push without explicit permission.
```
