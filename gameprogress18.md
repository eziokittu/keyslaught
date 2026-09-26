# KeySlaught Milestone 18 — Issues 9 Systems, Progression, and Route Pass

Date: 2026-09-27

## Completed

- Standardized menu navigation: Research and Lore Back buttons now live at the top-left.
- Removed brain cells from the main menu and made them strictly run-scoped; every new/restarted level begins at zero.
- Kept Knowledge Points menu-only, enlarged their presentation, and retained them as the permanent research currency.
- Rebuilt level selection as a cyclic Lore carousel. Lore I contains the six level buttons; Lore II intentionally shows `IN DEVELOPMENT`; both arrow controls wrap between the two pages.
- Added an angled `BEST mm:ss` corner tag to completed level buttons so completion time no longer overflows the button label.
- Added a completed-tutorial replay confirmation with Continue and Cancel choices.
- Added a tilted, animated `RECOMMENDED` tag to Endless mode.
- Replaced the one-wave Endless prototype with 30 escalating waves. Every fifth wave is a boss wave containing the boss plus normal enemies.
- Added one-Knowledge-point boss rewards and a Continue/End Run checkpoint after non-final Endless bosses.
- Added a 30-second pre-wave preparation countdown to Lore and Endless runs, using the same working fast-forward control as later intermissions. The guided Tutorial keeps its immediate scripted start.
- Increased enemy counts, movement pressure, and spawn cadence across Lore I Levels 1–6 so later chapters increasingly require turret support.
- Changed base Library HP to 15 and expanded Library HP research to 20 ranks at +5 HP per rank with increasing Knowledge costs.
- Preserved the new-player turret gate: no turret family is unlocked before Tutorial completion.
- Reduced base turret range and cadence, then split permanent turret research into independent range and attack-speed branches for Teacher, Engineer, Scientist, and President.
- Reworked movement, hit, Library-damage, and result particles into small soft-cloud or themed fleck effects; removed the large blue-square presentation.
- Repainted each gameplay path Tilemap from the actual waypoint routes, disabled differently colored route lines, and reused one shared path-sprite set across multi-entry routes.
- Re-aligned the top HUD into separate wave/skip, brain-cell, and time cards with no overlap.
- Added the reproducible authored builder and audit at `AgentScripts/BuildMilestone18Issues9.cs`.
- Reset local progression after verification: first launch is true, Tutorial is incomplete, Knowledge is 0, and all research/level completion is cleared.

## Verification

- Milestone 18 authored-scene audit: passed across `SampleScene` and all six Lore scenes.
- Compilation: clean (`compilationFailed=false`).
- EditMode: 66/66 passed.
- PlayMode: 24/24 passed, including new coverage for the 30-second skippable opening and boss checkpoint state.
- Unity console after final live review: 0 errors, 0 warnings.
- Unity VCS pre-commit integrity hook: passed.
- Portrait Game-view review at 900×1600 confirmed:
  - menu-only enlarged Knowledge Points and no menu brain-cell card;
  - top-left Research Back button and all twelve readable research branches;
  - cyclic Lore I/Lore II pages;
  - angled best-time corner tag;
  - Tutorial replay confirmation;
  - animated Endless recommendation;
  - 30-second `FIRST WAVE` timer with a visible fast-forward control;
  - aligned brain icon/count and 15/15 default Library HP;
  - shared-color painted route tiles in the multi-entry Lore Level 3 scene.
- Live Endless probe reached a 30-wave definition in `Intermission` with 30.0 seconds remaining.
- Live movement-particle probe completed with zero console errors/warnings.

## Explicitly not performed

- No Windows or WebGL build was created, following the existing no-build instruction.
- No commit or push was performed.
- No full human playthrough of all 30 Endless waves or every expanded Lore wave was performed; wave data, state transitions, scene wiring, automated tests, targeted runtime probes, and portrait visual captures were verified.
- Physical-device keyboard/touch/controller review and human audio review remain separate manual gates.

## Resume prompt

Open `gameprogress18.md` and `issues9.txt`. Start from a fresh local profile. Perform a full balance playthrough of Lore I Levels 1–6 and Endless waves 1–30, recording completion time, Library damage, brain-cell income/spend, turret purchases, and Knowledge rewards at each boss checkpoint. Tune enemy counts, cadence, turret research costs, and Library HP costs from those measurements. Keep authored-scene audit, EditMode/PlayMode tests, live behavior, portrait visual review, builds, device/browser checks, and Git state as separate verification gates. Do not build, commit, or push unless explicitly requested.
