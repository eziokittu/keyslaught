# KeySlaught progress 9 - persistent progression and authored game shell

Date: 2026-09-24

Previous progress file: `gameprogress8.md`

## Status verdict

The persistent progression foundation and outside-game shell are implemented, authored into `SampleScene`, and verified. The project now has a saved profile, four functional permanent research upgrades, first-launch/tutorial/lore/endless progression flags, and authored launch, main, mode-select, research, credits, and settings panels.

This phase is functionally complete. The menu and icon visual alignment/polish reported in `issues3.txt` is intentionally queued as the next phase; the actual tutorial and lore campaign content also remain future work.

## Implemented

- JSON-backed `PlayerPrefs` profile for first launch, tutorial completion, Lore I Level 1 completion, Endless unlock, knowledge points, and research levels.
- Four data-backed permanent research definitions:
  - Player attack range.
  - Magazine capacity.
  - Library maximum health.
  - Reload speed.
- Permanent upgrades are applied to the live coordinator, combat controller, and Library endpoint before a run begins.
- A two-second launch reveal followed by Continue and Exit actions.
- Authored main menu, mode select, research, credits, and settings screens under `Game Shell Canvas`.
- Tutorial recommendation state, Lore I Level 1 entry point, and a visibly disabled Endless option until Lore I Level 1 is completed.
- Lore victory grants one knowledge point and unlocks Endless; tutorial victory records tutorial completion.
- Research purchase validation for cost, insufficient points, maximum level, and persistence.
- Persistent local music and SFX preference toggles.
- Pause-menu confirmation can return to the authored main menu.
- Menu state suspends the player, combat controller, enemy movement, and wave ticking without globally pausing Unity time. This keeps the shell safe for the Editor test runner and cleanly resumes gameplay.

## Authored scene and data

- `Persistent Progression Systems` contains the progression service and permanent-upgrade applier.
- `Game Shell Canvas` contains six editable authored panels:
  - Launch Screen.
  - Main Menu.
  - Mode Select.
  - Research.
  - Credits.
  - Settings.
- Research assets live under `Assets/Data/Research`.
- Existing gameplay remains under the authored scene and is started through the selected shell mode.

## Key files

- `Assets/Scripts/Runtime/Progression/ProgressionProfile.cs`
- `Assets/Scripts/Runtime/Progression/ProgressionStore.cs`
- `Assets/Scripts/Runtime/Progression/ProgressionService.cs`
- `Assets/Scripts/Runtime/Progression/ResearchDefinition.cs`
- `Assets/Scripts/Runtime/Progression/PermanentUpgradeApplier.cs`
- `Assets/Scripts/Runtime/Progression/GameShellController.cs`
- `Assets/Scripts/Runtime/Scene/WaveRunController.cs`
- `Assets/Scripts/Runtime/Scene/PauseMenuController.cs`
- `AgentScripts/BuildMilestone9Scene.cs`
- `Assets/Tests/EditMode/ProgressionServiceTests.cs`

## Verification

- Runtime compilation: clean.
- Builder dry compilation and execution: clean.
- Authored-scene audit: 4 research definitions, persistent service/applier, and 6 authored shell panels with mode gating.
- EditMode: 49/49 passed.
- PlayMode: 16/16 passed.
- Live Play Mode launch state:
  - Unity time scale remained `1`.
  - Player and combat components were disabled while the shell was open.
  - Wave progression was suspended.
- Live Lore I Level 1 start:
  - Active mode changed to `LoreOneLevelOne`.
  - Wave 1 entered `Spawning`.
  - Player and combat components were enabled.
- Live return to main menu:
  - Active mode returned to `None`.
  - Shell became active.
  - Player and combat components were disabled again.
  - Unity time scale remained `1`.
- Final live console: 0 errors, 0 warnings; compilation failure flag false.
- Earlier 900 x 1600 Game-view review covered the launch and mode-select layouts; the temporary review captures were removed after inspection.

## Boundaries

- `issues3.txt` identifies a real presentation follow-up: menu/submenu alignment, centering inside containers, and professional icon replacement/polish. That visual pass is not claimed complete here.
- Tutorial and Lore I Level 1 currently enter the bounded gameplay run; they do not yet contain dedicated tutorial scripting or authored lore campaign content.
- Endless is an unlockable hook and remains labelled in development, not a completed endless-mode ruleset.
- Research costs, rewards, and upgrade values are prototype balance.
- Music/SFX toggles persist preference state, but an audio playback/mixer system is not yet wired, so audible behavior is not claimed.
- Persistence is covered by EditMode tests and live scene integration; a packaged-player persistence pass has not been run.
- Physical keyboard, mouse, phone, and multi-touch feel, Windows/WebGL builds, browser deployment, and release packaging remain unverified.
- No commit or push was performed.

## Next phase

Resolve `issues3.txt` as a focused authored-UI polish phase:

1. Audit every shell and in-game icon container at the target 900 x 1600 portrait resolution.
2. Normalize anchors, pivots, padding, icon bounds, and optical centering across menu, submenu, pause, and HUD controls.
3. Replace weak placeholder icons with a consistent project-owned monochrome set while preserving the existing editable prefabs.
4. Review each screen and the live HUD from fresh Game-view captures before runtime regression testing.
5. Re-run the authored-scene audit, EditMode tests, PlayMode tests, and live console check.

After that visual pass, implement dedicated tutorial scripting and the first authored lore level rather than treating the current shared bounded run as final campaign content.

## Copy-ready next-chat prompt

Continue KeySlaught from `gameprogress9.md` and implement the `issues3.txt` authored-UI polish phase. Use the live Unity Editor. First inspect every menu, submenu, pause, and HUD icon at 900 x 1600; then normalize alignment/padding and replace unpolished icons with a consistent project-owned monochrome set. Preserve editable scene objects and reusable prefabs. Verify the result with Game-view screenshots before EditMode, PlayMode, live console, and Git-state checks. Do not commit or push without explicit permission.
