# KeySlaught Milestone 17 — Issues 8 Expansion and Polish

Date: 2026-09-26

## Completed

- Reworked the main-menu top row so Back, brain cells, Knowledge Points, and Settings no longer overlap.
- Renamed the Lore entry to `LEVELS` and rebuilt Lore I as a six-level picker with persistent scene-loading listeners.
- Added authored Lore I Level 4, Level 5, and Level 6 scenes and enabled all six level scenes in Build Settings.
- Fixed the dedicated result flow so `NEXT LEVEL` advances through Levels 1–6; Level 6 returns to the main menu.
- Expanded every Lore I level to six waves. Each wave contains 5–20 child-friendly word enemies, difficulty and movement speed increase across levels, and every level ends with a visually enlarged boss word.
- Replaced tutorial bracket placeholders with real rich-text checkbox glyphs and green checkmarks; added brain, wand, and Library icons to the objective card.
- Improved the Library tutorial: the player view is moved beside the Library, LOSS moves at demonstration speed, damage stays visible, and a follow-up step repairs the Library to full health.
- Player attack range now appears only while moving and uses a thinner line.
- Added event-driven particles for movement, enemy hits, Library attacks, and run victory/defeat.
- Tutorial completion continues to unlock the Teacher, the first turret family.
- Expanded permanent research from four general upgrades to eight branches, adding independent Teacher, Engineer, Scientist, and President upgrades.
- Rebalanced progression: Knowledge research begins at 2–5 points and scales by 2–3 per rank; turret build costs are now Teacher 14, Engineer 18, Scientist 22, and President 20 brain cells. Lore victories still award Knowledge and defeated words still award brain cells, so upgrades require repeat play without blocking early progress.
- Added a reproducible authored builder and audit at `AgentScripts/BuildMilestone17Issues8.cs`.

## Verification

- Milestone 17 authored-scene audit: passed.
- Compilation: clean (`compilationFailed=false`).
- EditMode: 66/66 passed.
- PlayMode: 22/22 passed.
- Unity console ground truth after final tests: 0 errors, 0 warnings.
- Runtime visual review at the active portrait Game view:
  - main-menu header no longer overlaps;
  - six-level picker is readable and complete;
  - eight-branch research tree is readable on a dark header backplate;
  - tutorial uses real green ticks, objective icons, a thinner moving-only range ring, and visible movement particles.
- Live Library tutorial probe:
  - player moved to 2.83 units from the Library;
  - LOSS reduced Library HP from 35 to 31;
  - repair restored Library HP from 31 to 35.
- Live scene-routing probes:
  - Level 1 `NEXT LEVEL` path loaded `LoreOneLevelTwo`, wired as Level 2 with six waves;
  - direct Level 6 load reached `LoreOneLevelSix`, wired as Level 6 with six waves and a final `BOSS WAVE`.
- Project Auditor was unavailable because the Project Auditor package is not installed; the project-specific authored audit was used instead.

## Explicitly not performed

- No Windows or WebGL build was created, following the existing no-build instruction.
- No commit or push was performed.
- Physical-device keyboard/touch/controller review and human audio review remain separate manual gates.
- A full human playthrough of all 36 expanded waves was not performed; data shape, scene wiring, automated tests, targeted live routing, tutorial behavior, and visual captures were verified.

## Resume prompt

Open `gameprogress17.md` and `issues8.txt`. Do not create a build unless I explicitly ask. Start with a physical-device/full-playthrough balance pass across all six Lore I levels, record completion time, Library damage, turret purchases, and Knowledge/brain-cell earnings per level, then tune the economy from those measurements. Keep authored scenes, ScriptableObject level data, automated tests, runtime probes, visual review, builds, and Git state as separate verification gates.
