using UnityEngine;
using Blobs.Blobs;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Commands
{
    /// <summary>
    /// Command for executing and undoing blob merges.
    /// Updated for MVP architecture using interfaces.
    /// </summary>
    public class MergeCommand : ICommand
    {
        private readonly IBlobPresenter sourceBlob;
        private readonly IBlobPresenter targetBlob;

        // State before merge (for undo)
        private Vector2Int sourceOriginalPos;
        private Vector2Int targetOriginalPos;
        private BlobType targetType;
        private BlobColor sourceColor;
        private BlobColor targetColor;
        private bool isExecuted;

        public MergeCommand(IBlobPresenter source, IBlobPresenter target)
        {
            sourceBlob = source;
            targetBlob = target;

            // Store original state
            if (source?.Model != null)
            {
                sourceOriginalPos = source.Model.GridPosition;
                sourceColor = source.Model.Color;
            }

            if (target?.Model != null)
            {
                targetOriginalPos = target.Model.GridPosition;
                targetColor = target.Model.Color;
                targetType = target.Model.Type;
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
            
            sourceBlob.ExecuteMerge(targetBlob);
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

            IGridPresenter grid = ServiceLocator.Grid;
            if (grid == null)
            {
                Debug.LogWarning("[MergeCommand] Grid not found for undo");
                return;
            }

            // Move source blob back to original position
            if (sourceBlob != null)
            {
                sourceBlob.MoveTo(sourceOriginalPos);
            }

            // Recreate target blob at original position
            grid.SpawnBlob(targetOriginalPos, targetType, targetColor);

            isExecuted = false;
        }
    }
}
