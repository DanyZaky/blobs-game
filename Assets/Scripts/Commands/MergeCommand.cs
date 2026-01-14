using UnityEngine;
using Blobs.Core;
using Blobs.Blobs;

namespace Blobs.Commands
{
    /// <summary>
    /// Command for executing and undoing blob merges.
    /// Stores state before merge for undo functionality.
    /// </summary>
    public class MergeCommand : ICommand
    {
        private readonly Blob sourceBlob;
        private readonly Blob targetBlob;
        private readonly GridManager gridManager;

        // State before merge (for undo)
        private Vector2Int sourceOriginalPos;
        private Vector2Int targetOriginalPos;
        private BlobColor sourceColor;
        private BlobColor targetColor;
        private bool isExecuted;

        public MergeCommand(Blob source, Blob target, GridManager grid)
        {
            sourceBlob = source;
            targetBlob = target;
            gridManager = grid;

            // Store original state
            if (source?.CurrentTile != null)
            {
                sourceOriginalPos = source.CurrentTile.GridPosition;
                sourceColor = source.BlobColorType;
            }

            if (target?.CurrentTile != null)
            {
                targetOriginalPos = target.CurrentTile.GridPosition;
                targetColor = target.BlobColorType;
            }
        }

        public void Execute()
        {
            if (isExecuted)
            {
                Debug.LogWarning("[MergeCommand] Already executed");
                return;
            }

            if (sourceBlob == null || targetBlob == null)
            {
                Debug.LogWarning("[MergeCommand] Invalid blobs");
                return;
            }

            Debug.Log($"[MergeCommand] Executing merge: {sourceColor} -> {targetColor}");
            
            sourceBlob.ExecuteMerge(targetBlob, gridManager);
            isExecuted = true;
        }

        public void Undo()
        {
            if (!isExecuted)
            {
                Debug.LogWarning("[MergeCommand] Cannot undo - not executed");
                return;
            }

            Debug.Log($"[MergeCommand] Undoing merge");

            // Restore source blob to original position
            Tile sourceOriginalTile = gridManager.GetTile(sourceOriginalPos);
            if (sourceOriginalTile != null && sourceBlob != null)
            {
                sourceBlob.MoveTo(sourceOriginalTile);
            }

            // Recreate target blob at original position
            Tile targetOriginalTile = gridManager.GetTile(targetOriginalPos);
            if (targetOriginalTile != null)
            {
                gridManager.SpawnBlob(targetOriginalPos, targetColor);
            }

            isExecuted = false;
        }
    }
}
