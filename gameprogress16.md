# KeySlaught Milestone 16 — Tutorial and UI Polish

Date: 2026-09-26

## Completed

- Replaced the yellow tutorial highlight with a full-screen black spotlight and a soft circular opening around the required control.
- Added persistent tutorial objective cards with checkboxes, success/error feedback, and ordered input gating.
- Movement training now requires up, down, left, and right before advancing.
- Buffer training requires C, then A, then T; incorrect letters explain the next required key.
- CAT now spawns at the authored route start, moves along the path, pauses in player attack range, and is defeated by ordered player input.
- The Library damage demonstration locks movement and combat while LOSS follows the route to the Library.
- Fixed tutorial combat edge cases caused by inherited wave suspension and contact corruption.
- Increased and outlined UI text throughout gameplay, menus, buttons, options, and overlays.
- Increased brain-cell and timer icons/text and retained separate dark backing cards.
- Added an always-visible high-contrast `LIBRARY HP` HUD card and enlarged the in-world Library HP label with a backplate.
- Main-menu brain-cell display is icon plus number only.
- Rebuilt Controls as four animated visual cards: keyboard letters, arrow keys, controller, and on-screen keys.
- Lore I now opens its level picker. Levels 1, 2, and 3 are visible and launch their separate scenes.
- Lore I Level 3 now alternates enemies between two distinct route starts that share the Library endpoint.

## Verification

- Authored-scene audit: passed.
- EditMode: 65/65 passed.
- PlayMode: 22/22 passed.
- Unity console after final tests: 0 errors, 0 warnings.
- Overlay-inclusive runtime captures reviewed for tutorial spotlight/objectives, Controls, normal HUD/Library HP, Lore picker, and main menu currency.
- Live tutorial probe:
  - all four movement objectives ticked before advancement;
  - CAT advanced from route position `(-9.50, -5.50)` to `(-7.06, -5.50)` (`2.44` path units);
  - entering range changed the stage to `DefeatWord` and locked movement;
  - C, A, and T each returned `TargetHit` in sequence;
  - CAT was removed and the tutorial advanced to `TypingMessage`.

## Explicitly not performed

- No Windows or WebGL build was created, per the user's instruction.
- No commit or push was performed.
- Physical-device touch/controller checks and human audio review remain separate manual gates.

## Resume prompt

Open `gameprogress16.md` and `issues7.txt`. Do not create any build unless I explicitly ask. Continue from Milestone 16, visually test the complete tutorial on a physical target device, record any usability issues, and only then proceed to the next requested phase.
