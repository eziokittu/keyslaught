# KeySlaught

KeySlaught is a 2D typing-action tower-defense game. The player moves through a compact battlefield, protects a divine library, places and upgrades letter-firing defenses, and survives escalating enemy-word waves.

The first development milestone targets the complete endless-mode mechanics with placeholder presentation. Desktop WebGL is the primary release target; Windows 64-bit is the local test target.

## Technical baseline

- Unity `6000.3.11f1`
- Universal 2D / URP `17.3.0`
- Input System `1.19.0`
- Unity Test Framework `1.6.0`
- Unity Pipeline `0.7.0-exp.1`
- Reference viewport: `900 x 1000` (slightly portrait)

## Combat contract

- A valid typed letter fires immediately; there is no conventional reload cycle.
- Enemies are words and must lose letters from left to right.
- The player targets an in-range enemy whose next required letter matches the typed key.
- Among valid targets, the enemy closest to the library is selected first.
- A typed letter with no valid target is retained as an error in a bounded refresh buffer.
- Refresh clears that buffer after a timed delay; it is recovery from mistakes, not ammunition reloading.
- Turrets fire according to their allowed letter sets, range, and cooldown without using the player's refresh buffer.

The detailed and still-editable contract is in [docs/GAMEPLAY_CONTRACT.md](docs/GAMEPLAY_CONTRACT.md).

## Project structure

```text
Assets/
  Scenes/                 Authored Unity scenes
  Scripts/
    Runtime/              Shippable gameplay code
    Editor/               Editor-only tooling and setup
  Tests/
    EditMode/             Fast rules/data tests
    PlayMode/             Runtime integration tests
docs/                     Design and architecture decisions
```

Gameplay components will be scene/prefab authored and connected through serialized Inspector references. ScriptableObjects will contain immutable definitions; changing run state will remain in runtime models/components.

## Open locally

```powershell
unity open .
```

Run source-integrity validation:

```powershell
unity projects verify . --expect-editor 6000.3.11f1 --strict
```

The repository excludes Unity-generated folders, build output, machine-local Codex integration, and the original assignment PDF.

