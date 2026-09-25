# KeySlaught progress 13 - issues5 gameplay, tutorial, turret, and Lore selection pass

Date: 2026-09-25

Previous progress file: `gameprogress12.md`

## Status verdict

The `issues5.txt` change pass is implemented and ready for user review. The saved Sample and Lore I scenes now include the revised presentation, the tutorial completes its movement-buffer-refresh-CAT sequence without requiring the player to walk to the enemy spawn, turret family selection continues into letter-variant selection, and Lore I opens a three-level menu instead of launching Level 1 immediately.

This is not a release/build-complete milestone: Windows/WebGL builds, browser behavior, phone/touch feel, full Lore I balance, and Levels 2-3 content remain unverified or intentionally unimplemented.

## Implemented changes

1. Brain-cell rewards are credited directly when an enemy is defeated outside the player's playable bounds; in-bounds enemies still create collectible drops.
2. The tutorial begins with a second visible, draggable circular controller and explicit drag-to-move instruction.
3. The tutorial now interactively teaches the wand buffer before combat:
   - press highlighted C, A, and T buttons to load the buffer with no target;
   - press the highlighted Refresh button and wait for the buffer to clear;
   - continue to a stationary CAT placed inside player range;
   - type C, A, T to produce three immediate hits and the aligned `WORD DEFEATED` panel.
4. Tutorial guidance title/body/button RectTransforms were rebuilt for centered wrapped text, and the redundant `TYPE A-Z TO LOAD // SPACE RELOAD` text beside the wand buffer was hidden.
5. The existing intermission skip action is now a compact `>>` fast-forward icon button next to the next-wave countdown.
6. The player attack range is always visible with a cyan alpha pulse. A placed turret creates a gold range indicator that is visible only while the player occupies that turret tile.
7. Turret families now have serialized letter variants with one shared family cost:
   - Teacher: A-F, G-M, N-T, U-Z
   - Engineer: A-M, N-Z
   - Scientist: A-Z
   - President: AEIOU plus S and T
8. Turret targeting filters by the selected variant before choosing the eligible enemy closest to the Library. A regression verifies that an A-F Teacher skips `KNOWLEDGE` and hits `BOOK` before `CAR` when appropriate.
9. Enemy cards now update after every hit using remaining length across fixed bands: white at 0, yellow at 10, orange at 20, red at 30, pink at 40, violet at 50, and deep purple at 64, with interpolation at every intermediate length. Text becomes dark at orange and below.
10. The reusable text-button art is now a true 256 x 96 project-owned bordered surface. Settings, tutorial, mode, and Lore buttons have readable height around their labels.
11. Selecting Lore I now opens an authored level-select panel with Level 1 playable, Levels 2-3 locked/reserved, and Lore II locked.
12. Lore I Level 1 records a persistent best star rating and fastest completion time. Rating rules are currently:
    - 3 stars: no Library arrivals and completion within 180 seconds;
    - 2 stars: at most one Library arrival and completion within 300 seconds;
    - 1 star: any other victory.

## Key files

- `AgentScripts/BuildMilestone13Changes.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Scenes/LoreOneLevelOne.unity`
- `Assets/Scripts/Runtime/Scene/TutorialDirector.cs`
- `Assets/Scripts/Runtime/Scene/RangeCircleIndicator.cs`
- `Assets/Scripts/Runtime/Scene/TurretDefinition.cs`
- `Assets/Scripts/Runtime/Scene/TurretPadController.cs`
- `Assets/Scripts/Runtime/Scene/TileContextActionPanel.cs`
- `Assets/Scripts/Runtime/Scene/BrainCellEconomy.cs`
- `Assets/Scripts/Runtime/Scene/EnemyLengthPalette.cs`
- `Assets/Scripts/Runtime/Progression/ProgressionService.cs`
- `Assets/Scripts/Runtime/Progression/GameShellController.cs`
- `Assets/Art/Colorful/UI_Button_256x96.png`

## Verification

- Runtime compilation: clean (`compilationFailed=false`).
- Milestone 13 authored-scene audit: passed.
- EditMode: 57/57 passed.
- PlayMode: 20/20 passed.
- New PlayMode coverage verifies off-map auto-collection, range-indicator setup, variant state, and A-F Teacher eligible-target skipping.
- Live tutorial smoke at 900 x 1600 passed:
  - first CAT input produced three `LoadedIntoMagazine` results;
  - Refresh completed and cleared the buffer;
  - the tutorial CAT spawned inside range;
  - second CAT input produced three `TargetHit` results, zero buffered letters, zero active enemies, and the `TypingMessage`/`WORD DEFEATED` state.
- 900 x 1600 visual review completed for Settings button height, Lore level selection, movement controller/objective, wand-buffer explanation, and corrected `WORD DEFEATED` alignment.
- Player pulsing range was visible in live Play Mode.
- Final Unity console: 0 errors, 0 warnings.

The first EditMode request again caused the Unity Pipeline test-status endpoint to stall, matching the infrastructure issue already recorded in `gameprogress12.md`. The saved project was cleanly restarted by its verified Unity PID; the rerun completed 57/57, followed by PlayMode 20/20.

## Boundaries

- No Windows or WebGL build was produced.
- No browser or packaged-player test was performed.
- No physical phone/touch-device pass was performed; tutorial touch behavior was driven in Editor Play Mode.
- Turret tuning uses the existing family range/cadence values; this pass implemented variant coverage and selection, not a full balance pass.
- Lore I Levels 2 and 3 are represented as locked slots but do not yet have definitions or scenes.
- Star thresholds are initial authored rules and need balance review after a full Level 1 playtest.
- No commit or push was performed.

## Suggested next phase

1. User-playtest the full tutorial on mouse, keyboard, controller, and touch, including highlighted keys and Refresh timing.
2. Play Lore I Level 1 end to end and tune its 180/300-second star thresholds, Library-arrival allowances, wave words, route pressure, and brain-cell economy.
3. Validate every turret variant through the tile menu and compare its practical range/fire-rate profile.
4. Author Lore I Level 2 only after Level 1 and the star rules are accepted.
5. Run Windows and WebGL builds, then perform browser input/layout checks.

## Copy-ready next-chat prompt

Continue KeySlaught from `gameprogress13.md`. First user-playtest the complete interactive tutorial and Lore I Level 1 at 900 x 1600. Verify the tutorial controller, C-A-T buffer loading, Refresh clearing, in-range CAT defeat, off-map brain-cell credit, player/turret range circles, all Teacher/Engineer/Scientist/President letter variants, fast-forward icon, changing enemy colors, and Lore I star result. Tune the star thresholds and Level 1 pacing from observed play. Preserve connected prefabs, editable Tilemaps, serialized Inspector wiring, the 20/60/20 HUD, and existing progression saves. Then run Windows and WebGL builds and browser checks. Do not commit or push without explicit permission.
