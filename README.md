# Blobs Game

A 2D puzzle game where players control adorable blob creatures with unique behaviors and must merge and strategize their way through increasingly complex levels on a tiled board.

## 🎮 Game Overview

| Attribute | Description |
|-----------|-------------|
| **Genre** | Puzzle / Strategy |
| **Engine** | Unity (2D) |
| **Art Style** | Top-down with bubbly, soft-edged cartoon aesthetics |
| **Platforms** | Web, iOS, Android |

### Core Gameplay Loop
1. **Select** a Blob
2. **Merge** toward a valid blob (same row/column, different color)
3. **Activate** Blob Behaviors (Trail, Ghost, etc.)
4. **Repeat** until no blobs are left

---

## 🏗️ Architecture Overview

This project implements the **MVP (Model-View-Presenter)** pattern combined with **SOLID principles** for maintainable, testable, and extensible code.

### Project Structure

```
Assets/Scripts/
├── Models/           # Pure data classes (no Unity logic)
│   ├── BlobModel.cs
│   ├── GridModel.cs
│   └── GameStateModel.cs
├── Views/            # Visual representation (Unity components)
│   ├── BlobView.cs
│   ├── GridView.cs
│   └── TileView.cs
├── Presenters/       # Business logic coordinators
│   ├── BlobPresenter.cs
│   ├── GridPresenter.cs
│   ├── GamePresenter.cs
│   └── ServiceLocator.cs
├── Services/         # SRP-compliant specialized services
│   ├── InputService.cs
│   ├── SelectionService.cs
│   ├── MoveService.cs
│   ├── FeedbackService.cs
│   └── AudioManager.cs
├── Interfaces/       # Abstraction contracts
│   ├── IBlobModel.cs
│   ├── IBlobView.cs
│   ├── IBlobPresenter.cs
│   ├── IGridPresenter.cs
│   ├── IGamePresenter.cs
│   └── I*Service.cs
├── Blobs/            # Strategy pattern implementations
│   ├── IMergeBehavior.cs
│   ├── NormalMergeBehavior.cs
│   ├── TrailMergeBehavior.cs
│   ├── GhostMergeBehavior.cs
│   ├── FlagMergeBehavior.cs
│   ├── RockMergeBehavior.cs
│   └── SwitchMergeBehavior.cs
├── Commands/         # Command pattern for undo/redo
│   ├── ICommand.cs
│   ├── MergeCommand.cs
│   └── CommandManager.cs
├── Core/             # UI and level management
│   ├── UIManager.cs
│   ├── MainMenuController.cs
│   ├── LevelData.cs
│   └── LevelProgressManager.cs
└── Input/
    └── InputManager.cs
```

---

## 🎯 MVP (Model-View-Presenter) Pattern - Deep Dive

### Why MVP?

MVP separates the application into three distinct concerns, making the codebase:
- **Testable**: Models contain pure logic, testable without Unity
- **Maintainable**: Changes to UI don't affect business logic
- **Scalable**: New features can be added without touching existing layers

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER INPUT                               │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      PRESENTER LAYER                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │GamePresenter│  │GridPresenter│  │    BlobPresenter        │  │
│  │             │  │             │  │                         │  │
│  │• Game State │  │• Level Load │  │• Select/Deselect        │  │
│  │• Scoring    │  │• Spawn Blob │  │• MoveTo                 │  │
│  │• Win Check  │  │• Remove Blob│  │• ExecuteMerge           │  │
│  │• Pause/Resume│ │• Grid Query │  │• Coordinates M↔V        │  │
│  └──────┬──────┘  └──────┬──────┘  └────────────┬────────────┘  │
└─────────┼────────────────┼──────────────────────┼───────────────┘
          │                │                      │
          │     ┌──────────┴──────────┐           │
          │     │                     │           │
          ▼     ▼                     ▼           ▼
┌─────────────────────┐       ┌─────────────────────────────────┐
│    MODEL LAYER      │       │          VIEW LAYER             │
│  ┌───────────────┐  │       │  ┌─────────────────────────┐    │
│  │  BlobModel    │  │       │  │       BlobView          │    │
│  │               │  │       │  │                         │    │
│  │ • Type        │  │       │  │ • SpriteRenderer        │    │
│  │ • Color       │  │       │  │ • BlobAnimator          │    │
│  │ • GridPosition│  │       │  │ • Color Palette         │    │
│  │ • CanMergeWith│◄─┼───────┼──│ • Play*Animation()     │    │
│  └───────────────┘  │       │  └─────────────────────────┘    │
│  ┌───────────────┐  │       │  ┌─────────────────────────┐    │
│  │  GridModel    │  │       │  │       GridView          │    │
│  │               │  │       │  │                         │    │
│  │ • Width/Height│  │       │  │ • TileView[]            │    │
│  │ • BlobDict    │  │       │  │ • GridToWorldPosition   │    │
│  │ • AddBlob     │  │       │  │ • SpawnBlobView         │    │
│  │ • RemoveBlob  │  │       │  │ • CreateTileViews       │    │
│  └───────────────┘  │       │  └─────────────────────────┘    │
│  ┌───────────────┐  │       │                                 │
│  │GameStateModel │  │       │  No business logic here!        │
│  │               │  │       │  Views only respond to          │
│  │ • Score       │  │       │  commands from Presenters.      │
│  │ • MoveCount   │  │       │                                 │
│  │ • GameState   │  │       │                                 │
│  └───────────────┘  │       │                                 │
└─────────────────────┘       └─────────────────────────────────┘
     Pure Data Only                 Visual Only (Unity)
     No Unity Dependencies          No Business Logic
```

---

### Model Layer - Detailed

**Purpose**: Pure data storage with validation logic. No Unity MonoBehaviour dependencies.

**Key Characteristics**:
- ✅ Serializable
- ✅ Unit testable in isolation
- ✅ Contains only data and validation logic
- ❌ No rendering
- ❌ No input handling
- ❌ No Unity lifecycle (Start, Update, etc.)

#### BlobModel.cs
```csharp
[System.Serializable]
public class BlobModel : IBlobModel
{
    // Pure data fields
    private BlobType _type;
    private BlobColor _color;
    private Vector2Int _gridPosition;
    private int _size;

    // Properties expose data
    public BlobType Type => _type;
    public BlobColor Color => _color;
    public Vector2Int GridPosition { get; set; }
    
    // Computed property - business rule in model
    public bool CanInitiateMerge => _type == BlobType.Normal || _type == BlobType.Trail;

    // Validation logic belongs in Model
    public bool CanMergeWith(IBlobModel other)
    {
        if (other == null) return false;
        if (other == this) return false;
        
        // Rock cannot be merged with
        if (other.Type == BlobType.Rock) return false;
        
        // Flag requires same color
        if (other.Type == BlobType.Flag)
            return other.Color == this.Color;
        
        // Normal rule: different colors can merge
        if (other.Color == this.Color) return false;
        
        return true;
    }
}
```

#### GridModel.cs
```csharp
public class GridModel
{
    private int _width;
    private int _height;
    private Dictionary<Vector2Int, IBlobPresenter> _blobs;
    
    public int Width => _width;
    public int Height => _height;
    public int BlobCount => _blobs.Count;
    
    // Pure data operations
    public void AddBlob(Vector2Int position, IBlobPresenter blob)
    {
        _blobs[position] = blob;
    }
    
    public void RemoveBlob(Vector2Int position)
    {
        _blobs.Remove(position);
    }
    
    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _width && 
               pos.y >= 0 && pos.y < _height;
    }
    
    public bool IsPositionOccupied(Vector2Int pos)
    {
        return _blobs.ContainsKey(pos);
    }
}
```

---

### View Layer - Detailed

**Purpose**: Visual representation only. No business logic.

**Key Characteristics**:
- ✅ MonoBehaviour components
- ✅ Handles sprites, colors, animations
- ✅ Responds to Presenter commands
- ❌ No game logic decisions
- ❌ No direct Model manipulation
- ❌ No input processing

#### BlobView.cs
```csharp
[RequireComponent(typeof(BlobAnimator))]
public class BlobView : MonoBehaviour, IBlobView
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private BlobAnimator animator;
    private MaterialPropertyBlock materialPropertyBlock;
    
    // Color palettes - visual concern only
    private static readonly Color[] BaseColorPalette = new Color[]
    {
        HexToColor("FF80B3"),   // Pink
        HexToColor("47BDFF"),   // Blue
        HexToColor("FF6666"),   // Red
        // ... more colors
    };
    
    public bool IsAnimating => animator?.IsAnimating ?? false;
    
    // View receives commands, executes visuals
    public void Initialize(BlobType type, BlobColor color)
    {
        UpdateVisual(type, color);
        animator.SetOriginalScale(transform.localScale);
    }
    
    public void UpdateVisual(BlobType type, BlobColor color)
    {
        // Apply shader properties based on type/color
        Color baseColor = BaseColorPalette[(int)color];
        materialPropertyBlock.SetColor("_BaseColor", baseColor);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    
    // Animation commands - View doesn't decide WHEN, only HOW
    public void PlaySelectAnimation() => animator?.PlaySelectAnimation();
    public void PlayDeselectAnimation() => animator?.PlayDeselectAnimation();
    public void PlayIdleAnimation() => animator?.StartIdleAnimation();
    
    public void PlayMoveAnimation(Vector3 target, Action onComplete)
    {
        animator?.AnimateMoveTo(target, onComplete);
    }
    
    public void PlayDespawnAnimation(Action onComplete)
    {
        animator?.PlayDespawnAnimation(onComplete);
    }
    
    public void Destroy()
    {
        if (gameObject != null)
            Object.Destroy(gameObject);
    }
}
```

---

### Presenter Layer - Detailed

**Purpose**: Orchestrates Model and View interaction. Contains business logic.

**Key Characteristics**:
- ✅ Owns references to both Model and View
- ✅ Handles business logic decisions
- ✅ Coordinates data updates and visual updates
- ✅ Responds to service events
- ❌ No direct Unity rendering

#### BlobPresenter.cs
```csharp
public class BlobPresenter : IBlobPresenter
{
    // Presenter owns Model and View
    private readonly BlobModel _model;
    private readonly IBlobView _view;
    private readonly IMergeBehavior _mergeBehavior;  // Strategy injection
    private bool _isSelected;
    
    // Expose interfaces, not concrete types
    public IBlobModel Model => _model;
    public IBlobView View => _view;
    public bool IsSelected => _isSelected;
    
    public BlobPresenter(BlobModel model, IBlobView view, IMergeBehavior mergeBehavior)
    {
        _model = model;
        _view = view;
        _mergeBehavior = mergeBehavior;
    }
    
    // Business logic: Select only if allowed
    public void Select()
    {
        if (_isSelected || !_model.CanInitiateMerge) return;  // Business rule
        
        _isSelected = true;
        _view?.PlaySelectAnimation();  // Delegate visuals to View
    }
    
    public void Deselect()
    {
        if (!_isSelected) return;
        
        _isSelected = false;
        _view?.PlayDeselectAnimation();
    }
    
    // Presenter coordinates Model update + View animation
    public void MoveTo(Vector2Int targetPosition, Action onComplete = null)
    {
        // 1. Update Model (data)
        _model.GridPosition = targetPosition;
        
        // 2. Update View (visual)
        if (_view != null)
        {
            Vector3 worldPos = ServiceLocator.Grid?.GridToWorldPosition(targetPosition) 
                               ?? Vector3.zero;
            _view.PlayMoveAnimation(worldPos, () =>
            {
                _view.PlayIdleAnimation();
                onComplete?.Invoke();
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    // Delegate to Strategy Pattern
    public void ExecuteMerge(IBlobPresenter target)
    {
        IGridPresenter grid = ServiceLocator.Grid;
        
        // Strategy Pattern - behavior is injected, not hard-coded
        _mergeBehavior.OnMerge(this, target, grid);
    }
    
    public void PlayInvalidMoveEffect()
    {
        _view?.PlayShakeAnimation();
    }
    
    public void Dispose()
    {
        _view?.Destroy();
    }
}
```

#### GridPresenter.cs
```csharp
public class GridPresenter : MonoBehaviour, IGridPresenter
{
    [Header("View Reference")]
    [SerializeField] private GridView gridView;
    
    private GridModel _model;
    private LevelData _currentLevel;
    
    public int Width => _model?.Width ?? 0;
    public int Height => _model?.Height ?? 0;
    
    private void Awake()
    {
        _model = new GridModel(5, 5);
        ServiceLocator.RegisterGrid(this);  // Self-register
    }
    
    public void LoadLevel(LevelData levelData)
    {
        ClearGrid();
        _currentLevel = levelData;
        _model = new GridModel(levelData.width, levelData.height);
        
        // Create View tiles
        gridView.CreateTileViews(levelData.width, levelData.height);
        
        // Spawn blobs from level data
        foreach (var blobData in levelData.blobs)
        {
            SpawnBlob(blobData.position, blobData.type, blobData.color);
        }
    }
    
    public IBlobPresenter SpawnBlob(Vector2Int position, BlobType type, BlobColor color)
    {
        if (!_model.IsValidPosition(position)) return null;
        if (_model.IsPositionOccupied(position)) return null;
        
        // Create View (visual)
        BlobView view = gridView.SpawnBlobView(position, type, color);
        
        // Create Model (data)
        BlobModel model = new BlobModel(type, color, position);
        
        // Create behavior via Factory (OCP)
        IMergeBehavior mergeBehavior = CreateMergeBehavior(type);
        
        // Create Presenter (coordinates both)
        BlobPresenter presenter = new BlobPresenter(model, view, mergeBehavior);
        
        // Register in grid model
        _model.AddBlob(position, presenter);
        
        return presenter;
    }
    
    // Factory method - OCP compliance
    private IMergeBehavior CreateMergeBehavior(BlobType type)
    {
        return type switch
        {
            BlobType.Normal => new NormalMergeBehavior(),
            BlobType.Trail  => new TrailMergeBehavior(),
            BlobType.Ghost  => new GhostMergeBehavior(),
            BlobType.Flag   => new FlagMergeBehavior(),
            BlobType.Rock   => new RockMergeBehavior(),
            BlobType.Switch => new SwitchMergeBehavior(),
            _               => new NormalMergeBehavior()
        };
    }
}
```

#### GamePresenter.cs
```csharp
public class GamePresenter : MonoBehaviour, IGamePresenter
{
    private GameStateModel _gameState;
    
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GridPresenter gridPresenter;
    
    private void Awake()
    {
        _gameState = new GameStateModel();
        ServiceLocator.RegisterGame(this);
    }
    
    private void Start()
    {
        SubscribeToMoveService();
        SubscribeToCommandManager();
    }
    
    // Subscribe to events (SRP - GamePresenter handles game progression)
    private void SubscribeToMoveService()
    {
        IMoveService moveService = ServiceLocator.Move;
        if (moveService != null)
        {
            moveService.OnMergeExecuted += HandleMergeExecuted;
        }
    }
    
    private void HandleMergeExecuted(IBlobPresenter source, IBlobPresenter target)
    {
        IncrementMoveCount();
        AddScore(100);
        UpdateScoreUI();
        CheckWinCondition();
    }
    
    public void CheckWinCondition()
    {
        int remainingBlobs = gridPresenter.GetPlayableBlobCount();
        
        if (remainingBlobs <= 1)
        {
            int stars = CalculateStars();
            SetGameState(GameState.Won);
            uiManager.ShowWinPanel(_gameState.Score, stars);
        }
    }
    
    private int CalculateStars()
    {
        int undoCount = CommandManager.Instance?.HistoryCount ?? 0;
        
        if (undoCount == 0) return 3;
        if (undoCount <= 2) return 2;
        if (undoCount <= 4) return 1;
        return 0;
    }
}
```

---

## ⚙️ SOLID Principles - Deep Dive

### S - Single Responsibility Principle (SRP)

> "A class should have only one reason to change."

#### Problem Before Refactoring

The original `InputPresenter` had **8 responsibilities**:

```csharp
// ❌ BAD: Violates SRP - Too many responsibilities
public class InputPresenter : MonoBehaviour
{
    void Update()
    {
        // 1. Input polling
        if (Input.GetMouseButtonDown(0)) { ... }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { ... }
        
        // 2. Hit testing
        RaycastHit2D hit = Physics2D.Raycast(...);
        
        // 3. Selection state
        _selectedBlob = blob;
        
        // 4. Move-finding
        IBlobPresenter target = FindTargetInDirection(...);
        
        // 5. Move validation
        if (!CanMerge(source, target)) { ... }
        
        // 6. Command execution
        var command = new MergeCommand(...);
        command.Execute();
        
        // 7. Game progression
        _moveCount++;
        CheckWinCondition();
        
        // 8. Feedback
        PlaySFX("merge");
        PlayAnimation("shake");
    }
}
```

#### Solution After Refactoring

Split into focused, single-purpose services:

```
┌───────────────────────────────────────────────────────────────────┐
│                    BEFORE: InputPresenter                          │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │ Input → HitTest → Selection → FindMove → Validate →          │ │
│  │ Execute → GameProgress → Feedback                             │ │
│  │                                                               │ │
│  │ 8 RESPONSIBILITIES IN ONE CLASS!                              │ │
│  └──────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────┘
                               │
                               │ REFACTORED TO
                               ▼
┌───────────────────────────────────────────────────────────────────┐
│                    AFTER: Separated Services                       │
│                                                                    │
│  ┌────────────────┐  ┌───────────────────┐  ┌─────────────────┐  │
│  │ InputService   │  │ SelectionService  │  │  MoveService    │  │
│  │                │  │                   │  │                 │  │
│  │ • Poll input   │  │ • Track selected  │  │ • Find target   │  │
│  │ • Emit events  │  │ • Select/Deselect │  │ • Validate merge│  │
│  │                │  │ • Hit testing     │  │ • Execute merge │  │
│  └───────┬────────┘  └─────────┬─────────┘  └────────┬────────┘  │
│          │                     │                     │           │
│  ┌───────▼───────────────────────────────────────────▼────────┐  │
│  │                     Event Bus                                │  │
│  └───────┬───────────────────────────────────────────┬────────┘  │
│          │                                           │           │
│  ┌───────▼────────┐                        ┌─────────▼────────┐  │
│  │ GamePresenter  │                        │ FeedbackService  │  │
│  │                │                        │                  │  │
│  │ • Move count   │                        │ • Play SFX       │  │
│  │ • Win check    │                        │ • Play VFX       │  │
│  │ • Scoring      │                        │ • UI feedback    │  │
│  └────────────────┘                        └──────────────────┘  │
│                                                                    │
│  EACH SERVICE HAS EXACTLY ONE RESPONSIBILITY!                      │
└───────────────────────────────────────────────────────────────────┘
```

#### Code After Refactoring

```csharp
// ✅ GOOD: InputService - Only polls input
public class InputService : MonoBehaviour, IInputService
{
    public event Action<Vector2Int> OnDirectionInput;
    public event Action<Vector3> OnClickInput;
    
    void Update()
    {
        // Only responsibility: detect and emit input
        if (Input.GetKeyDown(KeyCode.UpArrow))
            OnDirectionInput?.Invoke(Vector2Int.up);
        
        if (Input.GetMouseButtonDown(0))
            OnClickInput?.Invoke(Input.mousePosition);
    }
}

// ✅ GOOD: SelectionService - Only manages selection
public class SelectionService : MonoBehaviour, ISelectionService
{
    private IBlobPresenter _selectedBlob;
    
    public IBlobPresenter SelectedBlob => _selectedBlob;
    
    public void Select(IBlobPresenter blob)
    {
        _selectedBlob?.Deselect();
        _selectedBlob = blob;
        _selectedBlob?.Select();
    }
    
    public void ClearSelection()
    {
        _selectedBlob?.Deselect();
        _selectedBlob = null;
    }
}

// ✅ GOOD: MoveService - Only handles moves
public class MoveService : MonoBehaviour, IMoveService
{
    public event Action<IBlobPresenter, IBlobPresenter> OnMergeExecuted;
    
    public IBlobPresenter FindTargetInDirection(IBlobPresenter source, Vector2Int dir) { ... }
    public MoveValidationResult ValidateMerge(IBlobPresenter src, IBlobPresenter tgt) { ... }
    
    public void TryMerge(IBlobPresenter source, IBlobPresenter target)
    {
        var validation = ValidateMerge(source, target);
        if (validation != MoveValidationResult.Valid) return;
        
        CommandManager.Instance.ExecuteCommand(new MergeCommand(source, target));
        OnMergeExecuted?.Invoke(source, target);
    }
}

// ✅ GOOD: FeedbackService - Only handles feedback
public class FeedbackService : MonoBehaviour, IFeedbackService
{
    public void PlayMergeFeedback(IBlobPresenter blob) { ... }
    public void PlayInvalidMoveFeedback(IBlobPresenter blob) { ... }
    public void ShowToast(string message) { ... }
}
```

---

### O - Open/Closed Principle (OCP)

> "Software entities should be open for extension, but closed for modification."

#### Implementation: Strategy Pattern for Blob Behaviors

```csharp
// ✅ OPEN for extension: Add new behaviors without changing existing code
public interface IMergeBehavior
{
    void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid);
}

// Each behavior is a separate class
public class NormalMergeBehavior : IMergeBehavior
{
    public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
    {
        // Standard merge: remove both blobs
        grid.RemoveBlob(source);
        grid.RemoveBlob(target);
    }
}

public class TrailMergeBehavior : IMergeBehavior
{
    public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
    {
        Vector2Int sourcePos = source.Model.GridPosition;
        Vector2Int targetPos = target.Model.GridPosition;
        
        // Leave trail blobs along the path
        Vector2Int direction = GetDirection(sourcePos, targetPos);
        Vector2Int current = sourcePos + direction;
        
        while (current != targetPos)
        {
            grid.SpawnBlob(current, BlobType.Normal, source.Model.Color);
            current += direction;
        }
        
        grid.RemoveBlob(target);
    }
}

public class GhostMergeBehavior : IMergeBehavior
{
    public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
    {
        Vector2Int sourcePos = source.Model.GridPosition;
        
        // Remove target, move source
        grid.RemoveBlob(target);
        source.MoveTo(target.Model.GridPosition);
        
        // Spawn ghost at original position
        grid.SpawnBlob(sourcePos, BlobType.Ghost, source.Model.Color);
    }
}
```

#### Adding a New Blob Type (OCP in Action)

To add a **new blob type** (e.g., "Bomb Blob"):

```csharp
// Step 1: Create new behavior - NO CHANGES to existing classes
public class BombMergeBehavior : IMergeBehavior
{
    public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
    {
        // Explode! Remove all adjacent blobs
        Vector2Int pos = target.Model.GridPosition;
        
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, 
                                    Vector2Int.left, Vector2Int.right })
        {
            var adjacent = grid.GetBlobAt(pos + dir);
            if (adjacent != null)
                grid.RemoveBlob(adjacent);
        }
        
        grid.RemoveBlob(source);
        grid.RemoveBlob(target);
    }
}

// Step 2: Update factory (only one change point)
private IMergeBehavior CreateMergeBehavior(BlobType type)
{
    return type switch
    {
        // ... existing cases
        BlobType.Bomb => new BombMergeBehavior(),  // Add new case
        _ => new NormalMergeBehavior()
    };
}
```

---

### L - Liskov Substitution Principle (LSP)

> "Objects of a superclass should be replaceable with objects of its subclasses without affecting correctness."

#### Implementation

All concrete classes implement interfaces, allowing substitution:

```csharp
// Code depends on interface, not concrete class
public void ProcessBlob(IBlobPresenter blob)
{
    blob.Select();
    blob.MoveTo(new Vector2Int(0, 0));
    blob.ExecuteMerge(target);
}

// Any IBlobPresenter implementation works correctly
IBlobPresenter normalBlob = new BlobPresenter(model1, view1, new NormalMergeBehavior());
IBlobPresenter trailBlob = new BlobPresenter(model2, view2, new TrailMergeBehavior());
IBlobPresenter ghostBlob = new BlobPresenter(model3, view3, new GhostMergeBehavior());

// All of these work identically
ProcessBlob(normalBlob);  // ✅ Works
ProcessBlob(trailBlob);   // ✅ Works
ProcessBlob(ghostBlob);   // ✅ Works
```

---

### I - Interface Segregation Principle (ISP)

> "Clients should not be forced to depend on interfaces they do not use."

#### Implementation: Focused Interfaces

```csharp
// ✅ GOOD: Small, focused interfaces

// For Models - only data concerns
public interface IBlobModel
{
    BlobType Type { get; }
    BlobColor Color { get; }
    Vector2Int GridPosition { get; set; }
    bool CanInitiateMerge { get; }
    bool CanMergeWith(IBlobModel other);
}

// For Views - only visual concerns
public interface IBlobView
{
    Transform Transform { get; }
    bool IsAnimating { get; }
    
    void PlaySelectAnimation();
    void PlayDeselectAnimation();
    void PlayMoveAnimation(Vector3 target, Action onComplete);
    void PlayShakeAnimation();
    void Destroy();
}

// For Presenters - only coordination concerns
public interface IBlobPresenter
{
    IBlobModel Model { get; }
    IBlobView View { get; }
    bool IsSelected { get; }
    
    void Select();
    void Deselect();
    void MoveTo(Vector2Int target, Action onComplete = null);
    void ExecuteMerge(IBlobPresenter target);
    void Dispose();
}

// For Services - each with single concern
public interface IInputService
{
    event Action<Vector2Int> OnDirectionInput;
    event Action<Vector3> OnClickInput;
}

public interface ISelectionService
{
    IBlobPresenter SelectedBlob { get; }
    void Select(IBlobPresenter blob);
    void ClearSelection();
}

public interface IMoveService
{
    event Action<IBlobPresenter, IBlobPresenter> OnMergeExecuted;
    IBlobPresenter FindTargetInDirection(IBlobPresenter source, Vector2Int dir);
    MoveValidationResult ValidateMerge(IBlobPresenter source, IBlobPresenter target);
    void TryMerge(IBlobPresenter source, IBlobPresenter target);
}
```

---

### D - Dependency Inversion Principle (DIP)

> "High-level modules should not depend on low-level modules. Both should depend on abstractions."

#### Implementation: ServiceLocator Pattern

```csharp
// ✅ GOOD: ServiceLocator provides abstraction layer
public static class ServiceLocator
{
    // Store interfaces, not concrete types
    private static IGamePresenter _game;
    private static IGridPresenter _grid;
    private static IInputService _input;
    private static ISelectionService _selection;
    private static IMoveService _move;
    private static IFeedbackService _feedback;
    
    // Expose via interface types
    public static IGamePresenter Game => _game;
    public static IGridPresenter Grid => _grid;
    public static IMoveService Move => _move;
    public static ISelectionService Selection => _selection;
    public static IFeedbackService Feedback => _feedback;
    
    // Registration methods accept interfaces
    public static void RegisterGame(IGamePresenter game) => _game = game;
    public static void RegisterGrid(IGridPresenter grid) => _grid = grid;
    public static void RegisterMove(IMoveService move) => _move = move;
    
    public static void Clear()
    {
        _game = null;
        _grid = null;
        _move = null;
        // ...
    }
}
```

#### Usage Example

```csharp
public class MoveService : MonoBehaviour, IMoveService
{
    // ✅ Depends on abstraction (IGridPresenter), not concrete (GridPresenter)
    private IGridPresenter Grid => ServiceLocator.Grid;
    
    public IBlobPresenter FindTargetInDirection(IBlobPresenter source, Vector2Int direction)
    {
        Vector2Int currentPos = source.Model.GridPosition;
        Vector2Int checkPos = currentPos + direction;
        
        while (Grid.IsValidPosition(checkPos))  // Uses interface method
        {
            IBlobPresenter target = Grid.GetBlobAt(checkPos);  // Uses interface method
            if (target != null) return target;
            checkPos += direction;
        }
        return null;
    }
}
```

#### Dependency Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                     HIGH-LEVEL MODULES                           │
│                                                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │GamePresenter│  │ MoveService │  │    SelectionService     │  │
│  └──────┬──────┘  └──────┬──────┘  └────────────┬────────────┘  │
│         │                │                      │               │
│         │                │                      │               │
│         ▼                ▼                      ▼               │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                    ABSTRACTIONS (Interfaces)                 ││
│  │                                                              ││
│  │  IGamePresenter  IGridPresenter  IMoveService  ISelection   ││
│  │  IBlobPresenter  IBlobModel      IBlobView     IFeedback    ││
│  │                                                              ││
│  └─────────────────────────────────────────────────────────────┘│
│         ▲                ▲                      ▲               │
│         │                │                      │               │
│  ┌──────┴──────┐  ┌──────┴──────┐  ┌────────────┴────────────┐  │
│  │GridPresenter│  │ BlobModel   │  │       BlobView          │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
│                                                                  │
│                     LOW-LEVEL MODULES                            │
└─────────────────────────────────────────────────────────────────┘

Both HIGH-LEVEL and LOW-LEVEL depend on ABSTRACTIONS,
not on each other directly!
```

---

## 🎨 Design Patterns Used

### Strategy Pattern (Merge Behaviors)
Different blob types have unique merge behaviors encapsulated in separate classes:

| Blob Type | Behavior | Can Initiate Merge |
|-----------|----------|-------------------|
| **Normal** | Standard merge - removes both blobs | ✅ Yes |
| **Trail** | Leaves trail of blobs along path | ✅ Yes |
| **Ghost** | Respawns at source position after merge | ❌ No |
| **Flag** | Goal blob - same color clears both when alone | ❌ No |
| **Rock** | Immovable obstacle | ❌ No |
| **Switch** | Toggles laser obstacles | ❌ No |

### Command Pattern (Undo System)
All merge operations are wrapped in commands for undo/redo:

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class MergeCommand : ICommand
{
    private IBlobPresenter _source;
    private IBlobPresenter _target;
    private Vector2Int _originalSourcePos;
    private Vector2Int _originalTargetPos;
    
    public void Execute()
    {
        // Store original state for undo
        _originalSourcePos = _source.Model.GridPosition;
        _originalTargetPos = _target.Model.GridPosition;
        
        // Perform merge
        _source.ExecuteMerge(_target);
    }
    
    public void Undo()
    {
        // Restore original state
        // Respawn blobs at original positions
    }
}

public class CommandManager
{
    private Stack<ICommand> commandHistory = new Stack<ICommand>();
    private Stack<ICommand> redoStack = new Stack<ICommand>();
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        commandHistory.Push(command);
        redoStack.Clear();
    }
    
    public void Undo()
    {
        if (commandHistory.Count == 0) return;
        
        ICommand command = commandHistory.Pop();
        command.Undo();
        redoStack.Push(command);
    }
}
```

### Service Locator Pattern
Centralized service access without tight coupling:

```csharp
// Usage in any class
IGridPresenter grid = ServiceLocator.Grid;
IMoveService moveService = ServiceLocator.Move;
```

---

## 🎵 Audio System

```csharp
AudioManager.Instance.PlayBGM("menu");     // Menu music
AudioManager.Instance.PlayBGM("gameplay"); // Gameplay music
AudioManager.Instance.PlaySFX("correct");  // Merge success
AudioManager.Instance.PlaySFX("miss");     // Invalid move
AudioManager.Instance.PlaySFX("undo");     // Undo action
AudioManager.Instance.PlaySFX("win");      // Level complete
```

---

## 🎬 Animation System (DOTween)

All animations are handled by `BlobAnimator` using DOTween:

- **Idle**: Subtle breathing animation
- **Select**: Scale up with bounce
- **Deselect**: Return to normal scale
- **Move**: Smooth position tween
- **Merge**: Scale and particle effects
- **Spawn/Despawn**: Pop in/out animations
- **Shake**: Invalid move feedback

---

## ⭐ Scoring System

| Undo Count | Stars Awarded |
|------------|---------------|
| 0 | ⭐⭐⭐ (3 stars) |
| 1-2 | ⭐⭐ (2 stars) |
| 3-4 | ⭐ (1 star) |
| 5+ | No stars |

---

## 📁 Scenes

| Scene | Description |
|-------|-------------|
| `Menu` | Main menu with level selection |
| `MVPGameplay` | Core gameplay scene |

---

## 🚀 Getting Started

1. Open the project in Unity
2. Open `Menu` scene
3. Press Play
4. Select a level to begin

---

## 📋 Future Enhancements

- [ ] Tutorial system with forced moves
- [ ] Sigil tile mechanic
- [ ] Laser obstacle + Switch interaction
- [ ] Colorblind mode (pattern overlays)
- [ ] Level editor

---

## 📚 References

- Game Design Document: `Resources/gdd_content.txt`
- Animation Library: [DOTween](http://dotween.demigiant.com/)
