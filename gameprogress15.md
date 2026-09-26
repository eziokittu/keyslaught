# KeySlaught progress 15 - issues7 progression, rewards, tutorial guidance, speed, and UI polish

Date: 2026-09-26

Previous progress file: `gameprogress14.md`

## Status verdict

The `issues7.txt` milestone is implemented and ready for user playtesting. The launch and shared-menu presentation, pause layout, readable currency/cost UI, persistent brain-cell wallet, research rewards, sequential turret unlocks, win/loss flow, forced tutorial guidance, Lore I three-level selection, persistent 1x/2x/3x speed, skip-wait control, entrance animation, and first balance pass are authored into the saved Unity scenes.

The authored-scene audit, 62 EditMode tests, 21 PlayMode tests, live launch smoke, 900 x 1600 visual review, Windows build, WebGL build, project verification, and VCS doctor passed. This is not a final balance, browser, physical-device, touch, or release sign-off; those still need human testing.

## Implemented changes

1. The launch screen holds on the splash for 2 seconds, then fades in its actions over 0.5 seconds. It now shows only a large `CONTINUE` action and a project-owned door icon at the top-right. The old `Defend the Divine Library` subtitle is removed, and the door opens a confirmation panel before quitting.
2. The approved `KeySlaught` splash artwork is now the common background for the main menu, mode selection, Lore level selection, Research, Settings, and Credits instead of the purple menu field.
3. The pause panel was re-laid out so its title, status, Resume, Settings, Main Menu, and nested audio settings remain inside the card at portrait resolution.
4. Gameplay HUD typography and spacing were enlarged. The brain-cell balance is visible beside its icon, while every turret action has a separate top-right cost or `LOCKED` marker rather than mixing currency with the family name.
5. Brain cells are now a persistent spendable progression wallet shared across runs. The menu header shows both brain cells and research points.
6. A victorious completed run credits 1 research point, plus 1 for every defeated boss in that run. Defeats save collected brain cells but award no research point, preventing loss/retry farming.
7. Turret families unlock sequentially:
   - Teacher: complete the tutorial;
   - Engineer: complete Lore I Level 1;
   - Scientist: complete Lore I Level 2;
   - President: complete Lore I Level 3.
8. Locked turret actions remain visible but cannot be purchased. This communicates the progression path without silently exposing every tower at the start.
9. Victory and defeat now open authored result overlays. They show the outcome, research reward, saved brain cells, any newly unlocked tower, and a context-aware `NEXT LEVEL`, `CONTINUE`, or `TRY AGAIN` action plus `MAIN MENU`.
10. Tutorial stages now gate movement, typed letters, and Refresh to the requested action. A pulsing focus guide highlights the relevant play/input area so unrelated input cannot bypass the guided step.
11. Lore I keeps its two-step route: select Lore I first, then choose among its three level cards. Levels unlock sequentially, and selecting a card starts that dedicated authored scene.
12. Settings and pause UI expose persistent 1x, 2x, and 3x choices. Menus stay at 1x; gameplay, tutorial, and resume restore the selected speed.
13. A `SKIP WAIT` button beneath the next-wave timer immediately advances waiting/intermission time without skipping active combat.
14. Menu/result cards use short unscaled-time fade-and-rise entrance animation. The tutorial focus uses an unscaled pulsing presentation.
15. The first pacing pass sets player speed to 5.6, intermission to 10 seconds, progressively faster enemies across Lore I Levels 1-3, and base turret shot intervals to Teacher 1.20s, Engineer 1.65s, Scientist 2.10s, and President 2.50s. Cadence upgrades use a 0.86 multiplier.

## Design decisions made explicit

- Brain cells are persistent between games because the menu now exposes an available balance and turret purchases need a durable wallet. Existing saves deserialize with a zero balance and remain compatible.
- The requested per-game research point is credited only when a game is completed successfully. Boss rewards stack on top of it. A loss keeps brain cells but gives no research point.
- Completing each progression milestone unlocks the next turret for subsequent play. The result overlay announces the unlocked family immediately.
- Levels 2 and 3 from progress 14 remain separate authored scenes and appear together in the Lore I level menu; the user still makes the second click that starts the chosen level.

## Key files

- `AgentScripts/BuildMilestone15Issues7.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Scenes/LoreOneLevelOne.unity`
- `Assets/Scenes/LoreOneLevelTwo.unity`
- `Assets/Scenes/LoreOneLevelThree.unity`
- `Assets/Art/Colorful/UI_Door_128.png`
- `Assets/Scripts/Runtime/Progression/GameShellController.cs`
- `Assets/Scripts/Runtime/Progression/GameSpeedSettings.cs`
- `Assets/Scripts/Runtime/Progression/ProgressionProfile.cs`
- `Assets/Scripts/Runtime/Progression/ProgressionService.cs`
- `Assets/Scripts/Runtime/Scene/BrainCellEconomy.cs`
- `Assets/Scripts/Runtime/Scene/DedicatedLevelSceneController.cs`
- `Assets/Scripts/Runtime/Scene/TileContextActionPanel.cs`
- `Assets/Scripts/Runtime/Scene/TutorialDirector.cs`
- `Assets/Scripts/Runtime/Scene/TutorialInputGate.cs`
- `Assets/Scripts/Runtime/Scene/WaveRunController.cs`
- `Assets/Scripts/Runtime/UI/ContextActionIconPresenter.cs`
- `Assets/Scripts/Runtime/UI/GameSpeedSelector.cs`
- `Assets/Scripts/Runtime/UI/RunResultPresenter.cs`
- `Assets/Scripts/Runtime/UI/TutorialFocusGuide.cs`
- `Assets/Scripts/Runtime/UI/UiPanelEntranceAnimator.cs`
- `Assets/Tests/EditMode/ProgressionServiceTests.cs`

## Verification

- Runtime/test compilation: clean (`compilationFailed=false`).
- Milestone 15 authored-scene audit: passed across Sample and all three Lore scenes. It verifies bounded pause/result/skip/currency/unlock wiring plus delayed launch actions, exit confirmation, shared artwork, currency header, speed controls, and forced tutorial focus.
- EditMode: 62/62 passed.
- PlayMode: 21/21 passed.
- New regression coverage verifies persistent/spendable brain cells, sequential turret unlocks, 1x-3x speed clamping, and forced tutorial input stages.
- Live 900 x 1600 Game-view review covered:
  - the clean launch splash, large Continue, delayed action area, and top-right door;
  - shared artwork and currency header in the main menu;
  - the bounded pause card;
  - the victory/reward/unlock result overlay;
  - tutorial instructions and joystick placement; the final serialized focus outline target was then audit-verified;
  - visible HUD brain-cell balance and distinct turret cost/lock presentation.
- Final live launch smoke after both builds: Play Mode entered successfully; compilation clean; console 0 errors and 0 warnings.
- Windows build: succeeded at `Builds/Windows/KeySlaught.exe`; 111,620,219 bytes; 20.345 seconds; 0 errors and 1 non-gameplay Pipeline configuration warning.
- WebGL build: succeeded at `Builds/WebGL`; 19,307,298 bytes; 79.589 seconds; 0 errors and the same 1 non-gameplay Pipeline configuration warning.
- The warning says no `RuntimePipelineConfig` is assigned, so remote Pipeline control is disabled in Player builds. Gameplay is unaffected, and the package should remain disabled for production players unless remote QA control is deliberately required.
- `unity projects verify . --strict`: passed with 642 files scanned and 0 findings.
- `unity vcs doctor`: passed with 0 errors, warnings, notices, or repairs required.

## Boundaries and remaining human checks

- The Windows artifact was built but not manually played as a standalone executable during this pass.
- The WebGL artifact was built but not served or exercised in a real browser. Browser rendering, keyboard focus, touch emulation, persistence, and audio policy still need verification.
- No physical phone/tablet, notch, touch keyboard, or controller pass was performed.
- The complete three-level, five-wave difficulty curve, brain-cell economy, tower unlock pacing, 1x/2x/3x usability, boss reward cadence, and star thresholds need end-to-end human playtesting. The current values are a deliberate first balance pass, not final tuning.
- The result screen was visually staged and reviewed, while full natural win/loss runs still need interaction testing for every route (`NEXT LEVEL`, `TRY AGAIN`, and `MAIN MENU`).
- The final tutorial focus target is serialized around the movement controller. Automated gating passed; a human should confirm that every highlight feels obvious and that the forced sequence is not frustrating on touch.
- The generated door icon is project-owned at 128 x 128 and follows the existing UI asset-size convention. No online or Asset Store artwork was added.
- `git diff --check` still reports Unity-generated trailing whitespace in serialized scene YAML; it was not hand-edited. Unity integrity checks are the authoritative serialization gate.
- The worktree also contains the preceding uncommitted issues6/progress14 milestone. No commit or push was performed.

## Suggested next phase

1. Play tutorial through completion on keyboard and touch, checking every forced highlight/input transition and the Teacher unlock result.
2. Complete and lose each Lore level naturally, exercising all result actions and verifying Engineer, Scientist, and President unlock announcements.
3. Tune wave speeds, 10-second waits, brain-cell costs/income, turret cadence, boss pressure, research rewards, and star thresholds from observed full runs at 1x, 2x, and 3x.
4. Run the Windows player and serve `Builds/WebGL` in a real browser; verify save persistence, keyboard focus, audio, pause/settings, skip wait, scene transitions, and portrait scaling.
5. Test at least one notched touch device before packaging a release candidate.

## Copy-ready next-chat prompt

Continue KeySlaught from `gameprogress15.md`. First perform a human tutorial and full Lore I Levels 1-3 playtest at 1x, 2x, and 3x. Verify forced tutorial highlights/input, persistent brain cells, victory-only base research plus boss bonuses, sequential Teacher/Engineer/Scientist/President unlocks, skip-wait behavior, and every win/loss result route. Tune movement, enemy pacing, turret cadence, costs/income, bosses, and star thresholds from observed play. Then run the Windows player and serve `Builds/WebGL` in a real browser, checking keyboard/touch input, persistence, audio, pause/settings, shared menu artwork, and portrait safe areas. Preserve authored scenes/prefabs, serialized Inspector wiring, existing saves, project-owned art, and the progress14 audio/corruption work. Do not commit or push without explicit permission.
