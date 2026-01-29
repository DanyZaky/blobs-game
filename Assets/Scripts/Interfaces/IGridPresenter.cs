using UnityEngine;
using System.Collections.Generic;
using Blobs.Blobs;
using Blobs.Core;

namespace Blobs.Interfaces
{
    /// <summary>
    /// Interface for grid presenter (grid management logic)
    /// </summary>
    public interface IGridPresenter
    {
        int Width { get; }
        int Height { get; }
        LevelData CurrentLevel { get; }
        
        // Initialization
        void LoadLevel(LevelData levelData);
        void ClearGrid();
        
        // Blob management
        IBlobPresenter SpawnBlob(Vector2Int position, BlobType type, BlobColor color);
        IBlobPresenter GetBlobAt(Vector2Int position);
        void RemoveBlob(IBlobPresenter blob);
        List<IBlobPresenter> GetAllBlobs();
        
        // Grid queries
        bool IsValidPosition(Vector2Int position);
        bool IsPositionOccupied(Vector2Int position);
        Vector3 GridToWorldPosition(Vector2Int gridPosition);
        
        // Win condition
        int GetPlayableBlobCount();
    }
}
