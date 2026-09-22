# KeySlaught progress 2 - pure gameplay mechanics

Date: 2026-09-22

Previous progress file: `gameprogress1.md`

## Status verdict

Milestone 2 is implemented and verified. KeySlaught now has pure, testable C# mechanics for sequential enemy-word damage, closest-to-library typed targeting, the wrong-letter refresh buffer, and library arrival damage. These systems are not connected to a gameplay scene yet, so the project is mechanics-ready for the authored arena phase but is not a playable game.

## What is implemented

- Enemy words normalize to uppercase and accept only combat letters `A` through `Z`.
- Correct letters consume enemy words sequentially. For example, `HELLO` progresses through `ELLO`, `LLO`, `LO`, `O`, and then defeated.
- Typed targeting filters out enemies that are out of range, defeated, or do not require the typed letter.
- The closest valid enemy to the library is selected by finite, non-negative distance.
- Exact distance ties use a stable `tieBreakOrder`, keeping tests and replays deterministic while leaving presentation free to provide authored/spawn order.
- A typed-attack resolver joins targeting, word progress, and error-buffer behavior:
  - a valid target is hit immediately;
  - a valid letter with no target enters the error buffer;
  - a full or actively refreshing buffer blocks input;
  - non-`A-Z` input is rejected without consuming a buffer slot.
- The error buffer has configurable capacity, seconds per occupied slot, and whether Refresh is allowed before full.
- Refresh duration is calculated from the number of occupied slots at refresh start. The buffer clears only when that duration completes; a configured zero duration clears immediately.
- Library arrival damage equals an enemy's remaining letter count, including zero damage for an already-defeated word. Library health clamps at zero.

## What remains unimplemented

- The mechanics are not connected to scene objects, prefabs, input actions, UI, animation, sound, or shot presentation.
- No authored arena, tilemap, enemy path, library, turret pad, player movement, enemy movement, spawner, wave, or boss exists yet.
- No gun corruption, cadence/upgrades, brain-cell currency, drops, collection, run-ending coordinator, or wave escalation exists yet.
- No turret families, library abilities, research persistence, tutorial, lore levels, menus, audio, or final art exists yet.
- No PlayMode test or human Play Mode interaction/visual review has been performed for this milestone.
- WebGL and Windows players were not rebuilt for this milestone, and browser behavior/deployment was not tested.

## Requirements and decisions

- Mechanics remain engine-independent and do not reference `MonoBehaviour`, scene objects, physics, or projectile trajectories.
- Mutable run state lives in runtime models. Future immutable enemy, gun, turret, ability, wave, and upgrade definitions remain planned as ScriptableObjects.
- Scene and prefab objects will adapt authored transforms into `EnemyTargetSnapshot` values rather than moving distance/range rules into presentation code.
- Distance ties are deterministic by stable order. This satisfies the gameplay contract's allowance that only exact ties may use an alternate tie-break policy without introducing nondeterministic tests.
- Refresh-before-full remains configuration rather than a hard-coded rule.
- Invalid keyboard characters are ignored by combat mechanics instead of being treated as gameplay mistakes.

## Architecture

- `EnemyWordState`: validates/normalizes one word and owns only its current letter progress.
- `EnemyTargetSnapshot`: immutable targeting input containing word state, current in-range status, distance to the library, and stable tie-break order.
- `TargetingRules`: pure closest-valid-target selection.
- `TypedAttackResolver`: one-keypress rule coordinator returning a presentation-friendly outcome and optional target.
- `RefreshBufferSettings`: immutable buffer capacity/timing/early-refresh configuration.
- `ErrorRefreshBuffer`: occupied-letter state, input lock, and refresh timer.
- `LibraryDamageRules` and `LibraryState`: remaining-letter arrival damage and bounded library health.

These classes are in the `KeySlaught.Gameplay` namespace and compile inside `KeySlaught.Runtime`. The EditMode tests compile in `KeySlaught.EditModeTests` and reference the runtime assembly.

## Changed areas for Milestone 2

Runtime mechanics were added under `Assets/Scripts/Runtime/Gameplay/`:

- `EnemyWordState.cs`
- `EnemyTargetSnapshot.cs`
- `TargetingRules.cs`
- `TypedAttackResolver.cs`
- `RefreshBufferSettings.cs`
- `ErrorRefreshBuffer.cs`
- `LibraryState.cs`

EditMode coverage was added under `Assets/Tests/EditMode/`:

- `EnemyWordStateTests.cs`
- `TargetingRulesTests.cs`
- `TypedAttackResolverTests.cs`
- `ErrorRefreshBufferTests.cs`
- `LibraryStateTests.cs`

Unity generated the corresponding `.meta` files during the verified test import. This file, `gameprogress2.md`, is the new milestone handoff. No Milestone 2 code changed the existing scene or project settings.

## Verification evidence

- Unity editor version resolved from the project: `6000.3.11f1`.
- Command: `unity test . --mode EditMode --output .artifacts\milestone2-editmode-results.xml --timeout 600 --format json`
- Result: passed, 28 total tests, 28 passed, 0 failed, 0 skipped.
- Test duration reported by NUnit: `0.0691652` seconds. This excludes Unity startup/import time.
- The passing Unity run compiled and imported the runtime and EditMode test assemblies, so it is also the compilation gate for these changes.
- Command: `unity vcs hooks run pre-commit . --format human`
- Result: exit code 0 with no reported missing/orphan metadata, duplicate GUID, merge-marker, package-manifest, or editor-version findings.
- The NUnit artifact was read for the counts above and then removed; it is not a source deliverable.

Verification boundaries:

- EditMode mechanics and script compilation: passed.
- PlayMode/runtime integration: not implemented or tested.
- Visual and interaction review: not performed.
- WebGL build and browser execution: not performed for this milestone.
- Windows player build and execution: not performed for this milestone.
- Deployment: not performed.

## Known risks and integration notes

- Scene adapters must calculate `IsInRange` and `DistanceToLibrary` consistently from authored transforms/path progress. Those values are trusted snapshots at the mechanics boundary.
- `tieBreakOrder` must be stable and unique enough for consistent exact-tie behavior; spawn sequence is the recommended source.
- Refresh currently snapshots duration when Refresh begins. Changing upgrades/settings during an active refresh will not retroactively change that timer.
- `ApplyEnemyArrival` returns the rule damage even when the library has less health remaining; callers can compare health before/after if they need effective damage for presentation.
- The project already had unrelated/uncommitted scene, milestone-1 documentation, and scene-template-setting changes before Milestone 2 began. They were preserved and not folded into the mechanics implementation.

## Exact Git boundary

- Branch: `main`, synchronized with `origin/main` at the start and end of implementation.
- Current `HEAD`: `131f1f3c12fa4bb8fa3f2ccce12bd3d37460c4f1` (`chore: finalize Unity build and VCS settings`).
- Milestone 2 is uncommitted and unpushed.
- Pre-existing working-tree changes preserved: modified `Assets/Scenes/SampleScene.unity`, modified `gameprogress1.md`, and untracked `ProjectSettings/SceneTemplateSettings.json`.
- Milestone 2 working-tree additions: the runtime/test files and Unity-generated metadata listed above, plus this `gameprogress2.md`.
- No commit or push was performed.

## Next implementation milestone

Milestone 3 should connect these mechanics to a small authored vertical slice without building the full metagame:

1. Author a square/portrait gameplay arena with a library endpoint and at least one editable enemy path.
2. Add transform-driven player movement using the existing Input System.
3. Add an enemy runtime component/prefab that traverses the authored path and exposes word/path state to the pure mechanics.
4. Add a serialized scene coordinator and basic spawner that references authored objects; do not introduce an opaque runtime bootstrap.
5. Add a minimal range/debug presentation so target distance and arrival can be observed.
6. Add EditMode tests for path calculations and PlayMode tests for movement, traversal, spawning, and library arrival.
7. Perform a visible Play Mode smoke test and record visual/runtime findings separately from automated results.

Typed keyboard combat UI, Refresh presentation, corruption, and shot effects should remain the following phase unless a minimal hook is needed to prove the scene adapter.

## Copy-ready prompt for the next chat

```text
Continue KeySlaught from C:\CodingStuff\GameDev\UnityProjects\KeySlaught.

Read docs/GAMEPLAY_CONTRACT.md, gameprogress1.md, and gameprogress2.md, then inspect the live Git and Unity state before changing anything. Milestone 2 added pure mechanics and 28 passing EditMode tests for enemy-word progress, closest valid targeting, typed-attack/error-buffer routing, configurable Refresh, and remaining-letter library damage. These mechanics are not scene-wired yet.

Implement Milestone 3 as an authored vertical slice: a square/portrait arena with a serialized library endpoint and editable enemy path, transform-driven player movement, an enemy prefab/component that traverses the path, and a basic serialized spawner/coordinator that adapts scene state into the existing pure mechanics. Preserve scene/prefab authorship and Inspector wiring; do not introduce an opaque runtime bootstrap. Add appropriate EditMode and PlayMode tests, then perform a visible Play Mode smoke test if the Editor is available.

Preserve unrelated existing working-tree changes. Verify compilation and tests, clearly separate automated runtime evidence from visual review, create gameprogress3.md, and do not commit or push without explicit permission.
```
