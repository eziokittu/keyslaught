# KeySlaught progress 5 - dual input, tilemaps, and pixel-art foundation

Date: 2026-09-22

Previous progress file: `gameprogress4.md`

## Status verdict

Milestone 5 is implemented and verified as the new control, authored-map, and presentation foundation. Movement is now isolated to arrow keys on a physical keyboard, A-Z remain combat letters, Space remains Refresh, and number keys 1-9 select context actions. Matching pointer/touch-facing controls are authored on the saved Canvas: all 26 letter buttons, a circular movement joystick, Refresh, and a right-side action panel that changes according to the tile beneath the player.

The saved scene now uses editable Unity Tilemaps and a reusable Tile Palette backed by 128 x 128 pixel-art assets. Three initial turret families are represented as Teacher, Engineer, and Scientist, following the requested three-turret scope. Turret-pad build/upgrade presentation and Library repair are functional, but turrets do not fire, cost currency, or influence combat yet. The finite run loop, economy, mobile-device QA, and final art pass remain future work.

## Implemented controls

### Keyboard-only path

- Arrow keys move the player. WASD no longer drives `PlayerMover`, so typing W, A, S, or D cannot accidentally move the player.
- A-Z route through the existing deterministic typed-combat controller.
- Space starts Refresh without consuming an attack letter.
- Number keys 1-9 select the matching visible context action.
- Current gameplay actions can be completed without clicking.

### Pointer/touch-facing path

- A circular virtual joystick feeds the same bounded player mover as keyboard/gamepad input.
- Twenty-six authored A-Z buttons feed the same `GameplayCombatController` as physical letters.
- The authored Refresh button feeds the same Refresh operation as Space.
- Each visible context action is a real `Button`; clicking/tapping it feeds the same `SelectAction` path as its number key.
- Current gameplay actions can be completed without typing.

The current UI is touch-oriented, but no physical phone, touch-screen, or multi-touch session was performed. Treat mobile shipping support as a later validation and responsive-layout milestone.

## Tile-sensitive actions

- `TileContextActionPanel` resolves the player's current cell from the serialized Context Tilemap rather than using physics collisions.
- Standing on a Library context tile opens:
  - `1  REPAIR +5`, which repairs without exceeding maximum health;
  - History, Influence, and Politics placeholders, visibly marked locked for later milestones.
- Standing on an empty authored turret pad opens:
  - `1  TEACHER`;
  - `2  ENGINEER`;
  - `3  SCIENTIST`.
- A selected turret is shown using its authored sprite. Returning to that pad exposes `1  UPGRADE`; levels currently run from 1 through 3 and use a small visual scale increase.
- Leaving a context tile hides the right-side panel.
- Context action objects and labels are explicitly named 1-9 in the saved hierarchy for editable Inspector work.

## Authored map and graphics

- `Authored Tilemaps` is saved beneath `KeySlaught Gameplay` and contains:
  - `Ground Tilemap`;
  - `Enemy Path Tilemap`;
  - `Context Tilemap`.
- Tile assets are saved in `Assets/Tiles/` for ground, enemy path, Library context, and turret-pad context.
- `Assets/TilePalettes/KeySlaughtPalette.prefab` provides the project palette source for manual map painting in Unity.
- The old background sprite and old visible path line are disabled; their gameplay path data remains intact.
- Project-ready pixel assets use 128 x 128 frames and Point filtering:
  - dark divine-library ground;
  - parchment/gold enemy path;
  - Teacher turret;
  - Engineer turret;
  - Scientist turret;
  - enemy card with a clear letter-label region;
  - four individual animated player frames plus a four-frame strip.
- The player now uses `PlayerPixelAnimator`, including movement animation and horizontal facing.
- The enemy prefab uses the new card sprite while preserving its live remaining-word label.
- Authored UI and world labels use Press Start 2P. The font and its OFL license are stored together in `Assets/Fonts/PressStart2P/`.
- `Assets/Art/Pixel/ART_SOURCES.md` records transparent AI-assisted art provenance, the generated source files, prompt summaries, and mechanical resizing/cropping.

## Architecture

- `PlayerMover`: combines arrow keys, gamepad stick, and virtual movement; exposes the last input for animation.
- `VirtualJoystick`: single-pointer drag adapter with configurable dead zone and bounded handle travel.
- `OnScreenLetterButton` and `OnScreenRefreshButton`: thin UI adapters into the existing combat controller.
- `PlayerPixelAnimator`: frame animation and facing presentation without owning movement rules.
- `TileContextActionPanel`: tile-cell detection, 1-9 keyboard routing, visible action state, Library repair, and turret-pad selection.
- `ContextActionButton`: pointer/touch adapter into the same numbered action selection.
- `TurretPadController`: authored pad cell plus selected family, level, and sprite presentation.
- `LibraryState.Repair` and `LibraryEndpoint.Repair`: clamped pure-state repair plus scene-label refresh.
- `AgentScripts/BuildMilestone5Scene.cs`: reproducibly imports/configures the art, creates tiles/palette, builds saved tilemaps and pads, updates player/enemy presentation, and authors the dual-input Canvas through Unity APIs.

## Changed areas for Milestone 5

- `Assets/Scenes/SampleScene.unity`
- `Assets/Scripts/Runtime/KeySlaught.Runtime.asmdef`
- `Assets/Scripts/Runtime/Gameplay/LibraryState.cs`
- `Assets/Scripts/Runtime/Scene/PlayerMover.cs`
- `Assets/Scripts/Runtime/Scene/LibraryEndpoint.cs`
- `Assets/Scripts/Runtime/Scene/VirtualJoystick.cs`
- `Assets/Scripts/Runtime/Scene/OnScreenLetterButton.cs`
- `Assets/Scripts/Runtime/Scene/OnScreenRefreshButton.cs`
- `Assets/Scripts/Runtime/Scene/PlayerPixelAnimator.cs`
- `Assets/Scripts/Runtime/Scene/TurretPadController.cs`
- `Assets/Scripts/Runtime/Scene/TileContextActionPanel.cs`
- `Assets/Scripts/Runtime/Scene/ContextActionButton.cs`
- `Assets/Art/Pixel/`
- `Assets/Fonts/PressStart2P/`
- `Assets/Tiles/`
- `Assets/TilePalettes/KeySlaughtPalette.prefab`
- `Assets/Prefabs/EnemyPrototype.prefab`
- `Assets/Tests/EditMode/LibraryStateTests.cs`
- `Assets/Tests/PlayMode/SceneVerticalSliceTests.cs`
- `Assets/Tests/PlayMode/KeySlaught.PlayModeTests.asmdef`
- `AgentScripts/BuildMilestone5Scene.cs`
- `.gitignore`
- Unity-generated `.meta` files
- `gameprogress5.md`

## Verification evidence

### Compilation and automated tests

- Live Editor recompile completed with `failed=false` and no compiler errors.
- Milestone 5 scene builder executed successfully in the connected Unity Editor.
- Final builder dry compile completed with no diagnostics.
- Final EditMode run: 40 total, 40 passed, 0 failed, 0 skipped.
  - Adds clamped Library repair coverage while retaining all prior pure mechanics/path coverage.
- Final PlayMode run: 10 total, 10 passed, 0 failed, 0 skipped.
  - Adds clamped/resettable virtual player movement.
  - Adds an on-screen letter-button route through the same combat controller.
  - Adds authored turret build, upgrade, and reset presentation.
- Final Unity console ground truth: compilation failure false, 0 errors, 0 warnings.
- Unity source-integrity/pre-commit hook: exit code 0.

### Saved-scene and visible Play Mode verification

- Live hierarchy inspection confirmed the three authored Tilemaps, three turret pads, player animator, dual-input Canvas, all A-Z buttons, joystick, Refresh, nine context buttons, and Input System EventSystem.
- A 900 x 1000 composited Game-view capture confirmed that the portrait playfield, all 26 letter buttons, circular joystick, and Refresh control render together.
- Moving the live player to an authored empty turret-pad cell visibly opened the right-side `BUILD TURRET` panel with Teacher, Engineer, and Scientist options.
- Invoking the first live UI button's click event built a level-1 Teacher turret and changed its renderer to `Turret_Teacher_128`; the panel changed to `TEACHER LV 1` and `1 UPGRADE`.
- Moving the live player to the Library tile visibly opened Repair plus the locked History, Influence, and Politics entries.
- Invoking the live Library Repair button increased health from 4 to 9, confirming the click route and exact +5 effect.
- Play Mode exited cleanly and verification captures were removed from project assets.

### Verification boundaries

- Script compilation: passed.
- EditMode tests: passed, 40/40.
- PlayMode automated tests: passed, 10/10.
- Saved hierarchy and serialized authored content: inspected.
- Visible default, turret-pad, built-turret, and Library contexts: passed.
- Physical keyboard feel: not performed in this automated session.
- Physical mouse/touch feel: not performed; UI click events and virtual-input routes were exercised programmatically.
- Phone layout, device touch, and multi-touch: not performed.
- Windows player build/execution: not performed.
- WebGL build/browser execution: not performed.
- Deployment: not performed.

## Known risks and prototype limitations

- Teacher, Engineer, and Scientist are buildable visual/data choices only. They have no targeting, cadence, damage, special rules, costs, refunds, or persistence yet.
- Upgrades currently change only level state and presentation scale.
- Library Repair has no resource cost yet.
- The context panel deliberately advertises History, Influence, and Politics as locked placeholders; their mechanics are not implemented.
- The desktop verification layout reserves side rails for the joystick and context actions around a portrait playfield. A responsive phone composition still needs device/aspect testing.
- `VirtualJoystick` currently tracks one pointer. Simultaneous joystick movement plus letter tapping needs physical multi-touch validation before a mobile release.
- The player frames are prototype animation art and use a simple timed walk cycle, not directional animation sets.
- Enemy word labels can still overlap when enemies bunch together.
- The automatic spawner and missing run-end behavior remain unchanged from Milestone 4.
- The generated art needs a final human consistency, silhouette, accessibility, and animation pass before release.
- Plain `git diff --check` reports Unity-generated trailing spaces on empty serialized fields such as `m_Name: ` and `m_Text: ` in `SampleScene.unity`. The Unity integrity hook passes, and authored C# and Markdown files have no trailing-whitespace findings.

## Exact Git boundary

- Branch: `main`; no branch change was made.
- Current `HEAD`: `131f1f3c12fa4bb8fa3f2ccce12bd3d37460c4f1` (`chore: finalize Unity build and VCS settings`).
- Milestones 2 through 5 remain uncommitted and unpushed.
- All earlier working-tree changes were preserved.
- No commit or push was performed.

## Next implementation milestone

Milestone 6 should make the new turret/context foundation materially affect a bounded run:

1. Add finite authored waves and stop spawning when the Library reaches zero.
2. Add typed-kill brain-cell drops and run currency.
3. Give Teacher, Engineer, and Scientist distinct data-driven combat behavior, targeting, cadence, and costs.
4. Spend run currency for pad building, turret upgrades, and Library repair; disable unaffordable actions in both keyboard and pointer paths.
5. Add a compact run HUD for wave, time, currency, and Library state.
6. Add a restart/reset flow that clears spawned enemies, pads, currency, wave state, buffer, and corruption.
7. Add EditMode and PlayMode regressions for economy, turret combat, wave completion, run end, action affordability, and restart.
8. Perform hands-on keyboard-only, mouse-only, and touch/multi-touch sessions before claiming complete dual-input support for a build.

Do not add permanent research, menus, the boss, or full endless scaling until the bounded wave/economy/turret loop is stable.

## Copy-ready prompt for the next chat

```text
Continue KeySlaught from C:\CodingStuff\GameDev\UnityProjects\KeySlaught.

Read docs/GAMEPLAY_CONTRACT.md and gameprogress1.md through gameprogress5.md, then inspect the live Git and Unity state before changing anything. Milestone 5 replaced WASD movement with arrow-only keyboard movement, kept A-Z combat and Space Refresh, added 1-9 context actions, and authored matching pointer/touch-facing A-Z buttons, circular joystick, Refresh, and right-side tile-context UI. It also added editable Ground/Enemy Path/Context Tilemaps, a Tile Palette, 128 x 128 prototype pixel art for the ground/path/three turrets/player animation/card enemy, and Press Start 2P UI. Teacher, Engineer, and Scientist can be selected and visually upgraded on authored pads; Library Repair +5 works. Final evidence is 40/40 EditMode and 10/10 PlayMode, clean compilation/console, saved hierarchy inspection, and visible Play Mode checks of the default, turret, built-turret, and Library contexts.

Implement Milestone 6 as a bounded wave/economy/turret loop: finite authored waves, stop/end at zero Library health, typed-kill brain-cell currency, distinct data-driven combat for Teacher/Engineer/Scientist, costs and affordability for build/upgrade/repair, a compact run HUD, and full restart/reset. Preserve serialized scene/prefab/UI wiring and both keyboard-only and pointer/touch-facing action paths. Add EditMode and PlayMode coverage and visibly review the runtime. Treat actual keyboard, mouse, phone, and multi-touch feel as separate manual gates.

Preserve all existing working-tree changes. Create gameprogress6.md only after implementation and verification, and do not commit or push without explicit permission.
```
