# Super Mario Unity

A small Mario-style 2D platformer made in **Unity 6 (6000.6.0f1)**, developed with an AI coding agent (Kiro).

All art and sounds are generated in code (pixel-art sprites + square-wave SFX), so the project has no external assets. The level is built at runtime from a tile grid.

## Features
- Run / walk, variable-height jump, coyote time and jump buffering
- Goombas that patrol and can be stomped
- Brick and `?` blocks (hit them from below), collectible coins
- Pits, 3 lives, score and coin HUD
- Goal flag at the end of the level
- Side-scrolling camera

## Controls
| Key | Action |
|---|---|
| A / D or Left / Right | Move |
| Space / W / Up | Jump (hold for higher) |
| Left Shift | Run |
| R | Restart after Game Over / Level Complete |

## How to run
1. Open the folder with **Unity Hub** (Unity 6000.6.0f1 or compatible).
2. Open `Assets/Scenes/Main.unity` and press **Play**.
   (The `GameManager` bootstraps itself, so it also works in any empty scene.)

## Project structure
- `Assets/Scripts/GameManager.cs` - bootstrap, camera setup, score/lives, HUD
- `Assets/Scripts/LevelBuilder.cs` - procedural level layout
- `Assets/Scripts/PlayerController.cs` - player movement and interactions
- `Assets/Scripts/EnemyPatrol.cs` - Goomba behaviour
- `Assets/Scripts/Block.cs`, `Coin.cs`, `Goal.cs` - level elements
- `Assets/Scripts/SpriteFactory.cs`, `SoundFx.cs` - generated art and sound
