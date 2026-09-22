# KeySlaught progress 4 - typed combat, Refresh, and corruption

Date: 2026-09-22

Previous progress file: `gameprogress3.md`

## Status verdict

Milestone 4 is implemented and verified. The authored vertical slice now connects A-Z keyboard input to the existing deterministic typed-attack rules, updates enemy words immediately, removes defeated enemies, presents direct hits without projectile physics, exposes the wrong-letter buffer and timed Refresh in the saved scene, and disables attacks during transform-contact gun corruption.

The core player typing loop is ready for hands-on keyboard-feel review, but KeySlaught is not yet a complete run. The automated mechanics and scene integration passed, and the combat HUD was visibly inspected in Play Mode. Physical keyboard injection could not be performed because the available Windows automation helper was unavailable, so real key feel and the WASD/A-Z overlap remain manual review items.

## What is implemented

- `KeyboardCombatInput` reads physical A-Z key-down events through Unity's Input System.
- Space triggers Refresh, preserving `R` as a combat letter.
- `GameplayCombatController` adapts the serialized scene coordinator's active-enemy snapshots into `TypedAttackResolver` calls.
- Correct letters:
  - select the closest-to-library matching enemy in the player's current range;
  - consume one sequential letter;
  - update the visible remaining-word label immediately;
  - display a short direct-shot line from player to enemy;
  - remove the enemy from the spawner and scene when its last letter is consumed.
- Letters with no valid target fill the existing bounded error buffer.
- A full buffer visibly locks typed input until Refresh begins.
- Refresh duration remains occupied-slot based at the prototype value of 0.5 seconds per slot.
- Early Refresh is enabled as authored configuration and is stated in the HUD.
- The authored HUD shows:
  - each occupied or empty buffer slot;
  - full-buffer lock state;
  - Refresh percentage and time remaining;
  - ready controls and early-Refresh configuration;
  - red gun-corruption time and `INPUT DISABLED` state.
- `GunCorruptionState` provides independently tested corruption timing.
- `GameplayCombatController` detects enemy contact by transform distance, without collision physics. Corruption is applied when an enemy enters the configured contact radius; remaining in contact does not repeatedly retrigger until that enemy first leaves and re-enters.
- The saved `Gameplay Coordinator` owns serialized references to the scene coordinator, HUD labels, shot line, and input adapter.
- `AgentScripts/BuildMilestone4Scene.cs` reproducibly updates the Milestone 3 gameplay root with the authored Milestone 4 presentation and wiring through Unity APIs.

## Input and tuning decisions

- A-Z are combat keys, including W, A, S, and D.
- Space is the Refresh key so Refresh does not consume an attack letter.
- The current movement map still supports WASD and arrow keys. Consequently, pressing WASD for movement also counts as typed combat input. This follows the current all-A-Z contract but needs hands-on design review; arrows are the clean movement-only option in the current prototype.
- Error-buffer capacity: 4.
- Refresh time: 0.5 seconds per occupied slot.
- Refresh before full: enabled.
- Corruption duration: 2.5 seconds.
- Transform contact distance: 0.8 world units.
- Direct-shot visibility: 0.1 seconds.

## Architecture

- `GunCorruptionState`: pure finite-duration corruption runtime state.
- `GameplayCombatController`: scene adapter for targeting, buffer/Refresh state, corruption contact, defeated-enemy removal, and combat presentation.
- `KeyboardCombatInput`: thin Input System adapter for A-Z and Space; it does not own gameplay rules.
- `GameplaySceneCoordinator`: remains the serialized boundary for the player, spawner, library, range, and active-enemy snapshots; it now also resolves snapshots back to scene enemies and removes defeated enemies.
- `EnemySpawner.Despawn`: removes a defeated enemy from active ownership before deferred GameObject destruction.
- `EnemyAgent.RefreshLabel`: remains the presentation hook after pure word state changes.

The Milestone 2 mechanics stay engine-independent. The Milestone 3 authored scene, prefab, path, movement, and library integration remain intact.

## Changed areas for Milestone 4

- `Assets/Scenes/SampleScene.unity`
- `Assets/Scripts/Runtime/Gameplay/GunCorruptionState.cs`
- `Assets/Scripts/Runtime/Gameplay/TypedAttackResolver.cs`
- `Assets/Scripts/Runtime/Scene/GameplayCombatController.cs`
- `Assets/Scripts/Runtime/Scene/KeyboardCombatInput.cs`
- `Assets/Scripts/Runtime/Scene/GameplaySceneCoordinator.cs`
- `Assets/Scripts/Runtime/Scene/EnemySpawner.cs`
- `Assets/Tests/EditMode/GunCorruptionStateTests.cs`
- `Assets/Tests/PlayMode/SceneVerticalSliceTests.cs`
- `AgentScripts/BuildMilestone4Scene.cs`
- `.gitignore` (unignores the reproducible Milestone 4 builder)
- Unity-generated `.meta` files for the new runtime and test scripts
- `gameprogress4.md`

## Verification evidence

### Compilation and automated tests

- Live Editor recompile completed with `failed=false` and no compiler errors.
- Final EditMode run: 39 total, 39 passed, 0 failed, 0 skipped.
  - Retains all Milestone 2 and 3 mechanics/path cases.
  - Adds corruption duration, expiry, extension, and invalid-duration cases.
- Final PlayMode run: 7 total, 7 passed, 0 failed, 0 skipped.
  - Retains movement, traversal, spawning, and library-arrival coverage.
  - Adds sequential typed hits and defeated-enemy removal.
  - Adds no-match buffering, full lock, and occupied-slot Refresh timing.
  - Adds corruption blocking and post-corruption attack restoration.
- The first Milestone 4 PlayMode run passed 6/7 and exposed a fixture with the test player and enemy at the identical transform. Contact correctly re-applied corruption after expiry. The fixture was separated while keeping the enemy in attack range, and the full final rerun passed 7/7.
- Final Unity console ground truth: compilation failure false, 0 errors, 0 warnings.
- Unity source-integrity/pre-commit hook: exit code 0.

### Saved-scene and visual Play Mode verification

- The Milestone 4 builder completed in the live Editor with no diagnostics and saved `SampleScene`.
- Saved-scene inspection confirmed `GameplayCombatController` and `KeyboardCombatInput` on the authored `Gameplay Coordinator`, with the combat controller's serialized coordinator reference intact.
- A 900 x 1000 Game view capture showed the typed-combat title, four empty buffer slots, ready/early-Refresh status, updated movement/combat hint, arena, player, enemies, route, and library.
- A live `TargetHit` call with the player positioned in range visibly showed the compact cyan shot line and immediate enemy-word change.
- Four guaranteed no-match letters visibly produced `[Z] [Q] [X] [V]` and `BUFFER LOCKED // SPACE TO REFRESH`.
- Starting Refresh and advancing 0.5 seconds visibly produced `REFRESHING 25% // 1.5s` for the four-slot/two-second Refresh.
- Triggering corruption visibly produced `GUN CORRUPTED 2.5s // INPUT DISABLED` in red.
- Play Mode exited cleanly, generated captures were deleted, and the Editor returned to ready Edit Mode.

### Verification boundaries

- Script compilation: passed.
- EditMode tests: passed, 39/39.
- PlayMode automated tests: passed, 7/7.
- Visible combat/HUD Play Mode review: passed.
- Physical keyboard input and player feel: not performed; the Windows computer-control helper failed to connect before input could be sent.
- WebGL build/browser execution: not performed for this milestone.
- Windows player build/execution: not performed for this milestone.
- Deployment: not performed.

## Known risks and prototype limitations

- WASD keys currently move and type simultaneously. This may be intended pressure or unwanted buffer pollution; confirm the intended control design through hands-on play before expanding progression systems.
- Enemy word labels still overlap when enemies bunch together. The visible smoke made this especially clear near the library.
- The automatic spawner still continues after library health reaches zero, so the smoke scene can reach `0/30` before observation finishes.
- Direct-shot presentation is deliberately minimal and has no audio, animation, cadence gating, recoil, or hit flash yet.
- Corruption uses a configurable transform radius and entry detection; it does not yet have a dedicated visible contact-radius preview.
- Path evaluation still allocates waypoint positions per query, as recorded in Milestone 3.
- Plain `git diff --check` reports seven trailing spaces on Unity-generated empty `m_Name: ` fields in `SampleScene.unity`. The Unity integrity hook passes, and milestone source files were not hand-edited to rewrite Unity's serialization.

## Exact Git boundary

- Branch: `main`; local tracking delta against `origin/main` remains `0 0`.
- Current `HEAD`: `131f1f3c12fa4bb8fa3f2ccce12bd3d37460c4f1` (`chore: finalize Unity build and VCS settings`).
- Milestones 2, 3, and 4 remain uncommitted and unpushed.
- Pre-existing changes remain preserved: `gameprogress1.md`, `ProjectSettings/SceneTemplateSettings.json`, and the scene/settings boundary recorded in prior progress files.
- Milestone 2 and 3 runtime, tests, data, prefab, art, builder, scene, and progress additions remain in the same working tree.
- Milestone 4 adds the combat/corruption runtime, tests, builder, saved-scene wiring, and this handoff listed above.
- No commit or push was performed.

## Next implementation milestone

Before adding broader systems, perform a real keyboard session and decide whether WASD should also feed typed attacks. Then Milestone 5 should turn the combat slice into a bounded run loop:

1. Stop spawning and end the run when library health reaches zero.
2. Add a simple authored wave schedule and finite wave state before endless escalation.
3. Add brain-cell drops on typed defeat and automatic collection into run currency.
4. Add a compact top HUD for wave, time, currency, and library state.
5. Add a restart flow that resets the authored scene state without an opaque runtime bootstrap.
6. Add EditMode and PlayMode regressions for run end, spawn stop, drops, currency, wave transition, and restart.
7. Perform Windows player and WebGL browser smoke checks only after the input-control decision and bounded run loop are stable.

Do not add turrets, abilities, permanent research, menus, or the full endless progression until the input conflict and bounded run lifecycle are resolved.

## Copy-ready prompt for the next chat

```text
Continue KeySlaught from C:\CodingStuff\GameDev\UnityProjects\KeySlaught.

Read docs/GAMEPLAY_CONTRACT.md and gameprogress1.md through gameprogress4.md, then inspect the live Git and Unity state before changing anything. Milestone 4 connected A-Z input to deterministic closest-valid typed attacks, live enemy-word updates/removal, a direct-shot line, the four-slot error buffer and Space Refresh HUD, and transform-contact gun corruption. The final automated evidence is 39/39 EditMode and 7/7 PlayMode, with clean compilation and Unity console. Visible Play Mode confirmed target hit, full lock, 25% Refresh progress, and corruption presentation.

First perform a real physical-keyboard Play Mode session and decide with me whether WASD should simultaneously move and feed typed attacks; the prior automation helper was unavailable, so this is still unverified. Preserve the current option that arrow keys move without attack input until that decision is made.

After the control decision, implement Milestone 5 as a bounded run loop: stop spawning/end at zero library health, add a simple authored finite wave schedule, typed-kill brain-cell drops with automatic collection/run currency, a compact run HUD, and restart/reset flow. Keep scene and UI references serialized, add EditMode/PlayMode coverage, and visibly review the runtime. Do not add turrets, abilities, research, menus, or full endless progression yet.

Preserve all existing working-tree changes. Create gameprogress5.md only after implementation and verification, and do not commit or push without explicit permission.
```
