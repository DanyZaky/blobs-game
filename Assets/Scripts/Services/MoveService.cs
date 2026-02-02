using UnityEngine;
using System;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Services
{
    /// <summary>
    /// Move finding and validation service.
    /// SRP: Only handles move-finding, validation, and execution.
    /// Does NOT handle input, selection, or feedback directly.
    /// </summary>
    public class MoveService : MonoBehaviour, IMoveService
    {
        // Service reference
        private IGridPresenter Grid => ServiceLocator.Grid;

        // Events
        public event Action<IBlobPresenter, IBlobPresenter> OnMergeExecuted;

        private void Awake()
        {
            // Self-register to ServiceLocator
            ServiceLocator.RegisterMove(this);
        }
        public event Action<IBlobPresenter, MoveValidationResult> OnMergeAttemptFailed;

        public IBlobPresenter FindTargetInDirection(IBlobPresenter source, Vector2Int direction)
        {
            if (source?.Model == null || Grid == null) return null;

            Vector2Int currentPos = source.Model.GridPosition;
            Vector2Int checkPos = currentPos + direction;

            // Search in direction until we find a blob or hit grid boundary
            while (Grid.IsValidPosition(checkPos))
            {
                IBlobPresenter targetBlob = Grid.GetBlobAt(checkPos);
                if (targetBlob != null)
                {
                    return targetBlob;
                }
                checkPos += direction;
            }

            return null;
        }

        public MoveValidationResult ValidateMerge(IBlobPresenter source, IBlobPresenter target)
        {
            if (source == null || target == null)
                return MoveValidationResult.NoTarget;

            // Check row/column alignment
            Vector2Int sourcePos = source.Model.GridPosition;
            Vector2Int targetPos = target.Model.GridPosition;

            if (sourcePos.x != targetPos.x && sourcePos.y != targetPos.y)
                return MoveValidationResult.NotAligned;

            // Check if can merge (color compatibility, type rules)
            if (!source.Model.CanMergeWith(target.Model))
            {
                // Determine why it can't merge
                if (target.Model.Color == source.Model.Color)
                    return MoveValidationResult.SameColor;

                return MoveValidationResult.IncompatibleType;
            }

            return MoveValidationResult.Valid;
        }

        public void TryMergeInDirection(IBlobPresenter source, Vector2Int direction)
        {
            if (source == null)
            {
                Debug.LogWarning("[MoveService] TryMergeInDirection called with null source");
                return;
            }

            IBlobPresenter target = FindTargetInDirection(source, direction);

            if (target == null)
            {
                Debug.Log("[MoveService] No blob found in direction");
                OnMergeAttemptFailed?.Invoke(source, MoveValidationResult.NoTarget);
                return;
            }

            TryMerge(source, target);
        }

        public void TryMerge(IBlobPresenter source, IBlobPresenter target)
        {
            MoveValidationResult validation = ValidateMerge(source, target);

            if (validation != MoveValidationResult.Valid)
            {
                Debug.Log($"[MoveService] Merge validation failed: {validation}");
                OnMergeAttemptFailed?.Invoke(source, validation);
                return;
            }

            // Execute the merge
            Debug.Log($"[MoveService] Executing merge: {source.Model.Color} -> {target.Model.Color}");
            source.ExecuteMerge(target);

            // Notify listeners
            OnMergeExecuted?.Invoke(source, target);
        }
    }
}
