using UnityEngine;
using System.Collections.Generic;
using Blobs.Blobs;
using Blobs.Core;
using Blobs.Interfaces;
using Blobs.Models;
using Blobs.Views;

namespace Blobs.Presenters
{
    /// <summary>
    /// Presenter for grid logic.
    /// Coordinates between GridModel (data) and GridView (visual).
    /// </summary>
    public class GridPresenter : MonoBehaviour, IGridPresenter
    {
        [Header("View Reference")]
        [SerializeField] private GridView gridView;

        private GridModel _model;
        private LevelData _currentLevel;

        public int Width => _model?.Width ?? 0;
        public int Height => _model?.Height ?? 0;
        public LevelData CurrentLevel => _currentLevel;

        private void Awake()
        {
            _model = new GridModel(5, 5);

            if (gridView == null)
                gridView = GetComponent<GridView>();
            if (gridView == null)
                gridView = gameObject.AddComponent<GridView>();

            // Register with ServiceLocator
            ServiceLocator.RegisterGrid(this);
        }

        public void LoadLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[GridPresenter] LevelData is null!");
                return;
            }

            ClearGrid();
            _currentLevel = levelData;
            _model = new GridModel(levelData.width, levelData.height);

            Debug.Log($"[GridPresenter] Loading level: {levelData.levelName} ({levelData.width}x{levelData.height})");

            // Create tile views
            gridView.CreateTileViews(levelData.width, levelData.height);

            // Apply special tile types
            foreach (var tileData in levelData.tiles)
            {
                gridView.SetTileType(tileData.position, tileData.type);
            }

            // Spawn blobs
            foreach (var blobData in levelData.blobs)
            {
                SpawnBlob(blobData.position, blobData.type, blobData.color);
            }

            Debug.Log($"[GridPresenter] Level loaded: {_model.BlobCount} blobs spawned");
        }

        public void ClearGrid()
        {
            // Dispose all blob presenters
            if (_model != null)
            {
                foreach (var blob in _model.GetAllBlobs())
                {
                    blob.Dispose();
                }
                _model.Clear();
            }

            // Clear tile views
            gridView?.ClearTileViews();
        }

        public IBlobPresenter SpawnBlob(Vector2Int position, BlobType type, BlobColor color)
        {
            if (!_model.IsValidPosition(position))
            {
                Debug.LogWarning($"[GridPresenter] Invalid position: {position}");
                return null;
            }

            if (_model.IsPositionOccupied(position))
            {
                Debug.LogWarning($"[GridPresenter] Position occupied: {position}");
                return null;
            }

            // Create view
            BlobView view = gridView.SpawnBlobView(position, type, color);

            // Create model
            BlobModel model = new BlobModel(type, color, position);

            // Create merge behavior
            IMergeBehavior mergeBehavior = CreateMergeBehavior(type);

            // Create presenter
            BlobPresenter presenter = new BlobPresenter(model, view, mergeBehavior);

            // Register in model
            _model.AddBlob(position, presenter);

            return presenter;
        }

        private IMergeBehavior CreateMergeBehavior(BlobType type)
        {
            return type switch
            {
                BlobType.Normal => new NormalMergeBehavior(),
                BlobType.Trail => new TrailMergeBehavior(),
                BlobType.Ghost => new GhostMergeBehavior(),
                BlobType.Flag => new FlagMergeBehavior(),
                BlobType.Rock => new RockMergeBehavior(),
                BlobType.Switch => new SwitchMergeBehavior(),
                _ => new NormalMergeBehavior()
            };
        }

        public IBlobPresenter GetBlobAt(Vector2Int position)
        {
            return _model?.GetBlobAt(position);
        }

        public void RemoveBlob(IBlobPresenter blob)
        {
            if (blob == null) return;

            _model.RemoveBlob(blob.Model.GridPosition);
            blob.Dispose();
        }

        public List<IBlobPresenter> GetAllBlobs()
        {
            return _model?.GetAllBlobs() ?? new List<IBlobPresenter>();
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return _model?.IsValidPosition(position) ?? false;
        }

        public bool IsPositionOccupied(Vector2Int position)
        {
            return _model?.IsPositionOccupied(position) ?? false;
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            return gridView?.GridToWorldPosition(gridPosition) ?? Vector3.zero;
        }

        public int GetPlayableBlobCount()
        {
            return _model?.GetPlayableBlobCount() ?? 0;
        }

        public void MoveBlob(IBlobPresenter blob, Vector2Int from, Vector2Int to)
        {
            _model.MoveBlob(from, to);
        }
    }
}
