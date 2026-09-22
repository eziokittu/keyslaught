# KeySlaught progress 3 - authored vertical slice

Date: 2026-09-22

Previous progress file: `gameprogress2.md`

## Status verdict

Milestone 3 is implemented and verified. `SampleScene` now contains an authored, Inspector-wired vertical slice with a visible portrait arena, editable enemy path, Input System player movement component, enemy prefab and ScriptableObject definitions, automatic spawning, transform-driven path traversal, library arrival damage, and debug presentation.

This is the first visible gameplay scene, but it is not yet the complete combat loop. Enemies can spawn, move, and damage the library; the player can move, but keyboard attacks, Refresh UI, corruption, currency, waves, and run-ending behavior remain unimplemented.

## What is implemented

- A saved `KeySlaught Gameplay` root in `Assets/Scenes/SampleScene.unity` while preserving the pre-existing camera, global light, Grid, and Tilemap roots.
- A near-square/portrait prototype arena with a border, dark playfield, title, and movement hint.
- A six-point authored `WaypointPath`; moving its child waypoint transforms changes the route.
- Visible path rendering plus pure path length, interpolation, and remaining-distance calculations.
- A scene-authored player with:
  - `PlayerMover` component;
  - serialized reference to `Assets/InputSystem_Actions.inputactions`;
  - `Player/Move` action using the existing WASD, arrow, gamepad, joystick, and XR bindings;
  - transform-driven normalized movement;
  - configurable speed and arena bounds;
  - visible attack-range preview.
- `EnemyDefinition` ScriptableObjects for `BOOK` and `HISTORY`, containing base word and movement speed.
- An authored `EnemyPrototype.prefab` with an `EnemyAgent`, sprite presentation, and visible remaining-word label.
- Transform-driven `EnemyAgent` traversal along the authored path, remaining path distance, stable spawn order, mechanics target snapshots, and a single arrival event.
- A serialized `EnemySpawner` referencing the prefab, path, definitions, and `Active Enemies` scene root.
- A `LibraryEndpoint` with runtime `LibraryState`, visible health text, and remaining-letter arrival damage.
- A serialized `GameplaySceneCoordinator` referencing the player, spawner, and library. It adapts active enemies into the pure targeting mechanics and routes enemy arrival into library damage/removal.
- A reproducible Editor builder at `AgentScripts/BuildMilestone3Scene.cs`. It updates only the dedicated `KeySlaught Gameplay` root and creates/updates the milestone assets through Unity APIs.

## What remains unimplemented

- Physical-key typed attacks are not connected to `TypedAttackResolver` yet.
- Enemy labels do not yet update from live typed damage because combat input is not scene-wired.
- No error-buffer/Refresh HUD or Refresh control exists in the scene.
- No direct-shot presentation, attack cadence, gun definition, upgrade, or corruption behavior exists.
- The library reaching zero does not end the run; the prototype spawner continues.
- No brain-cell drops/currency, wave schedule, boss, turret placement, turret families, or abilities exist.
- No menus, tutorial, lore levels, research persistence, audio, or final art exists.
- WebGL and Windows players were not rebuilt in this milestone. Browser execution and deployment were not tested.

## Requirements and decisions

- The scene is authored and saved, not generated at runtime by an opaque bootstrap.
- Scene references are serialized in the Inspector. The live Editor confirmed the player input asset/action name, spawner prefab/path/definitions/root, and coordinator player/spawner/library references.
- Enemy base data uses ScriptableObjects; word progress, path progress, spawn order, active-enemy lists, and library health remain runtime state.
- Movement, traversal, and arrival use transforms and distances only. No physics collision or projectile trajectory was introduced.
- The solid prototype sprite, line material, prefab, and definition assets are project-owned so the presentation does not rely on unstable built-in UI sprites.
- The scene builder is versioned by explicitly unignoring its one reproducible file while other `AgentScripts` scratch content remains ignored.

## Architecture

- `PathMath`: pure polyline length, distance evaluation, and distance-remaining rules.
- `WaypointPath`: serialized transform adapter and path preview renderer.
- `PlayerMover`: Input System adapter and bounded transform movement.
- `EnemyDefinition`: immutable authored base word and movement speed.
- `EnemyAgent`: runtime word/path progress, target snapshot adapter, presentation label, and arrival event.
- `EnemySpawner`: serialized prefab/data/path references and active-enemy ownership.
- `LibraryEndpoint`: scene adapter around pure `LibraryState`.
- `GameplaySceneCoordinator`: serialized coordination and scene-to-mechanics snapshot boundary.

The Milestone 2 pure mechanics remain independent of MonoBehaviours. Milestone 3 components adapt scene transforms and authored assets into those models.

## Changed areas for Milestone 3

- `Assets/Scenes/SampleScene.unity`
- `Assets/Scripts/Runtime/KeySlaught.Runtime.asmdef` (adds the `Unity.InputSystem` assembly reference)
- `Assets/Scripts/Runtime/Scene/`
  - `PathMath.cs`
  - `WaypointPath.cs`
  - `PlayerMover.cs`
  - `EnemyDefinition.cs`
  - `EnemyAgent.cs`
  - `EnemySpawner.cs`
  - `LibraryEndpoint.cs`
  - `GameplaySceneCoordinator.cs`
- `Assets/Tests/EditMode/PathMathTests.cs`
- `Assets/Tests/PlayMode/SceneVerticalSliceTests.cs`
- `Assets/Data/Enemies/Book.asset`
- `Assets/Data/Enemies/History.asset`
- `Assets/Prefabs/EnemyPrototype.prefab`
- `Assets/Art/Prototype/PrototypeLine.mat`
- `Assets/Art/Prototype/SolidSprite.asset`
- `AgentScripts/BuildMilestone3Scene.cs`
- `.gitignore` (unignores only the reproducible Milestone 3 builder)
- Unity-generated `.meta` files for the new assets and directories

## Verification evidence

### Compilation and automated tests

- Live Editor recompile: completed with `failed=false`, no compiler errors.
- Final EditMode run: 34 total, 34 passed, 0 failed, 0 skipped.
  - Includes all 28 Milestone 2 mechanics cases.
  - Adds six path calculation/boundary cases.
- Final PlayMode run: 4 total, 4 passed, 0 failed, 0 skipped.
  - bounded transform-driven player movement;
  - multi-segment enemy traversal and single arrival event;
  - prefab spawning at the authored path start;
  - serialized-style coordinator flow from spawn through library damage and enemy removal.
- An initial PlayMode run exposed test auto-spawning after a manual spawn. `EnemySpawner.SetAutomaticSpawning` was added, the test was isolated correctly, and the final rerun passed 4/4.

### Saved-scene and asset verification

- Live hierarchy inspection confirmed the saved `KeySlaught Gameplay` root and its authored arena, path with six waypoint children, library, player, spawner, active-enemy root, coordinator, and presentation labels.
- Saved `PlayerMover` properties confirmed:
  - input asset: `Assets/InputSystem_Actions.inputactions`;
  - action: `Player/Move`;
  - speed: `5`;
  - bounds: `(-7.3, -8)` to `(7.3, 8)`.
- Saved `EnemySpawner` properties confirmed references to `EnemyPrototype.prefab`, the scene path, both enemy definitions, and `Active Enemies`; automatic interval is four seconds.
- Saved coordinator properties confirmed serialized references to player, spawner, and library with attack range `4`.
- Unity source-integrity/pre-commit hook: exit code 0.
- Unity Console after tests and smoke run: compilation failure false, 0 errors, 0 warnings.

### Visible Play Mode smoke evidence

- The 900 x 1000 Game view was inspected before Play Mode and showed the arena, route, gold library, cyan player/range, title, and unclipped movement hint.
- Play Mode entered successfully and the live hierarchy showed `Enemy_000_BOOK` instantiated beneath the authored `Active Enemies` root.
- A Play Mode screenshot showed the `BOOK` enemy on the visible route with its word label.
- At accelerated time scale for observation, multiple `BOOK`/`HISTORY` enemies visibly advanced along the path and the saved coordinator reduced library health from `30/30` to `0/30` as enemies arrived.
- Play Mode exited cleanly and the Editor returned to stopped/ready state.
- Physical keyboard movement was not manually exercised during the visual smoke. Movement logic passed PlayMode coverage and the saved Input System asset/action reference was inspected, but human key-feel remains a manual review item.
- Screenshots were written under ignored `Temp/` paths and are not source deliverables.

### Verification boundaries

- Script compilation: passed.
- EditMode tests: passed, 34/34.
- PlayMode automated tests: passed, 4/4.
- Visible spawn/traversal/library-arrival review: passed.
- Physical keyboard feel: not manually reviewed.
- Combat input/Refresh/corruption: not implemented.
- WebGL build/browser execution: not performed for this milestone.
- Windows player build/execution: not performed for this milestone.
- Deployment: not performed.

## Known risks and prototype limitations

- Multiple enemy word labels can overlap when enemies bunch together; later UI/presentation work should resolve this.
- Path evaluation currently builds a waypoint-position array on each query. This is acceptable for the vertical slice but should be cached or invalidated when scale increases.
- The automatic spawner has no wave cap or library-destroyed stop condition yet.
- The library label uses prototype world-space text and will be replaced by the gameplay HUD.
- The player range preview is presentation-only; typed combat will use the serialized coordinator range when connected.
- Plain `git diff --check` reports five trailing spaces on Unity-generated empty `m_Name: ` YAML fields in `SampleScene.unity`. The Unity integrity hook passes, milestone source files have no trailing whitespace, and the live serialized scene was intentionally not hand-edited to suppress Unity's normal representation.

## Exact Git boundary

- Branch: `main`; local tracking delta against `origin/main` is `0 0`.
- Current `HEAD`: `131f1f3c12fa4bb8fa3f2ccce12bd3d37460c4f1` (`chore: finalize Unity build and VCS settings`).
- Milestones 2 and 3 remain uncommitted and unpushed.
- Pre-existing changes preserved from before Milestone 2: `gameprogress1.md` and `ProjectSettings/SceneTemplateSettings.json`; the prior dirty `SampleScene.unity` Grid/Tilemap and serialization changes were retained and extended with the intentional Milestone 3 scene.
- Milestone 2 additions remain in the same working tree: pure gameplay mechanics, EditMode tests, and `gameprogress2.md`.
- Milestone 3 additions are the authored scene/runtime/data/prefab/art/test/builder areas listed above plus this `gameprogress3.md`.
- No commit or push was performed.

## Next implementation milestone

Milestone 4 should connect the existing pure typed-combat rules to this authored vertical slice:

1. Add a scene input adapter that converts physical `A-Z` keypresses into `TypedAttackResolver` calls.
2. Use the coordinator's active-enemy snapshots and attack range for target selection.
3. Update the hit enemy's visible remaining-word label immediately and remove defeated enemies.
4. Add compact shot/hit presentation without physics-based trajectories.
5. Add an authored error-buffer/Refresh HUD showing occupied slots, full-lock state, refresh progress, and early-refresh configuration.
6. Add gun corruption state and a visible disabled-input indicator.
7. Add EditMode and PlayMode regressions for successful shots, no-match buffering, full lock, refresh timing, defeated-enemy removal, and corruption blocking.
8. Perform physical keyboard and visible Play Mode review separately from automated tests.

Do not add currency, turrets, abilities, or the full wave system until the player typing/Refresh loop is stable.

## Copy-ready prompt for the next chat

```text
Continue KeySlaught from C:\CodingStuff\GameDev\UnityProjects\KeySlaught.

Read docs/GAMEPLAY_CONTRACT.md, gameprogress1.md, gameprogress2.md, and gameprogress3.md, then inspect the live Git and Unity state before changing anything. Milestone 3 created a saved authored vertical slice with an editable path, Input System player movement, enemy prefab/definitions, spawning, traversal, library arrival damage, 34 passing EditMode tests, and 4 passing PlayMode tests. Visible Play Mode confirmed spawn, traversal, and library health changes. Milestones 2 and 3 are still uncommitted and unpushed.

Implement Milestone 4: connect physical A-Z input to the existing TypedAttackResolver through the serialized scene coordinator; update enemy labels/removal; add compact direct-shot presentation; add an authored error-buffer/Refresh HUD with lock/progress behavior; and add gun corruption with a visible disabled-input state. Keep scene/prefab/UI references serialized and do not introduce an opaque runtime bootstrap. Add EditMode/PlayMode tests and perform a physical-keyboard visual smoke test separately from automated evidence.

Preserve all existing working-tree changes. Create gameprogress4.md after verification, and do not commit or push without explicit permission.
```
