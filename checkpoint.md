# Blobs Game - Development Checkpoint
**Last Updated:** 2026-02-04 13:24

---

## ✅ What's Been Done

### Core Architecture (MVP Pattern)
- [x] `GamePresenter.cs` - Game state, scoring, win condition
- [x] `GridPresenter.cs` - Grid management, level loading
- [x] `BlobPresenter.cs` - Individual blob logic (MVP)
- [x] `BlobModel.cs` / `BlobView.cs` - Data & visual separation
- [x] `CommandManager.cs` - Undo system (Command Pattern)
- [x] `InputManager.cs` - Keyboard + Mouse input
- [x] `MoveService.cs` - Merge validation & execution
- [x] `ServiceLocator.cs` - Dependency injection

### 6 Blob Types (Strategy Pattern)
| Type | Behavior | Can Initiate? |
|------|----------|---------------|
| Normal | Standard merge | ✅ Yes |
| Trail | Leaves trail behind | ✅ Yes |
| Ghost | Respawns at source pos | ❌ No |
| Flag | Same color clears both | ❌ No |
| Rock | Obstacle | ❌ No |
| Switch | Toggles laser | ❌ No |

### Level System
- [x] `LevelData.cs` - ScriptableObject for level config
- [x] `LevelProgressManager.cs` - Star saving (PlayerPrefs)
- [x] Level loading from `LevelData` (grid size, blob positions)

### UI System
- [x] `MainMenuController.cs` - Level selection, animations
- [x] `UIManager.cs` - Win panel, pause menu, score display
- [x] Star rating system (based on undo count: 0=3⭐, 1-2=2⭐, 3-4=1⭐, 5+=0⭐)
- [x] Real-time score display with punch animation
- [x] Undo button (disabled when nothing to undo)

### Audio System
- [x] `AudioManager.cs` - BGM & SFX with inspector clips
- [x] Menu BGM (`menu`), Gameplay BGM (`gameplay`)
- [x] SFX: `ui button`, `correct`, `miss`, `undo`, `win`

### Animations (DOTween)
- [x] Idle, Select, Deselect, Move, Merge, Spawn, Despawn, Shake

---

## 📁 Project Structure
```
Assets/Scripts/
├── Core/
│   ├── UIManager.cs
│   ├── MainMenuController.cs
│   ├── LevelProgressManager.cs
│   └── GameEnums.cs
├── Blobs/
│   ├── IMergeBehavior.cs
│   ├── NormalMergeBehavior.cs
│   ├── TrailMergeBehavior.cs
│   ├── GhostMergeBehavior.cs
│   ├── FlagMergeBehavior.cs
│   ├── RockMergeBehavior.cs
│   └── SwitchMergeBehavior.cs
├── Commands/
│   ├── ICommand.cs
│   ├── MergeCommand.cs
│   └── CommandManager.cs
├── Models/
│   ├── BlobModel.cs
│   └── GameModel.cs
├── Presenters/
│   ├── GamePresenter.cs
│   ├── GridPresenter.cs
│   └── BlobPresenter.cs
├── Views/
│   ├── BlobView.cs
│   └── TileView.cs
├── Services/
│   ├── AudioManager.cs
│   ├── MoveService.cs
│   └── ServiceLocator.cs
└── Input/
    └── InputManager.cs
```

---

## 🎮 Scenes
| Scene | Description |
|-------|-------------|
| `Menu` | Main menu + level selection |
| `MVPGameplay` | Gameplay scene |

---

## 📋 TODO Next
- [ ] Tutorial system (forced moves, step-by-step)
- [ ] Sigil tile mechanic
- [ ] Laser obstacle + Switch interaction
- [ ] Colorblind mode (pattern overlays)

---

## 🔗 Reference
- GDD: `Resources/gdd_content.txt`

