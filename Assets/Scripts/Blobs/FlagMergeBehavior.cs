using UnityEngine;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Blobs
{
    /// <summary>
    /// Flag Blob: Goal point. A blob of the SAME COLOR can merge with it
    /// to clear BOTH blobs. Only works when no other playable blobs are on the board.
    /// (Rock obstacles don't count). Cannot initiate merge.
    /// </summary>
    public class FlagMergeBehavior : IMergeBehavior
    {
        public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[FlagMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log($"[FlagMergeBehavior] Flag merge attempt");

            // Check if only these two playable blobs remain (Rock doesn't count)
            int playableCount = grid.GetPlayableBlobCount();
            if (playableCount > 2)
            {
                Debug.Log($"[FlagMergeBehavior] Cannot merge with flag - {playableCount - 2} other playable blobs remain!");
                source.PlayInvalidMoveEffect();
                return;
            }

            // Check same color (required for flag)
            if (source.Model.Color != target.Model.Color)
            {
                Debug.Log("[FlagMergeBehavior] Cannot merge - flag requires same color!");
                source.PlayInvalidMoveEffect();
                return;
            }

            Debug.Log("[FlagMergeBehavior] 🚩 Flag cleared successfully!");

            Vector2Int targetPos = target.Model.GridPosition;
            source.Deselect();

            // Move source to flag position first
            source.MoveTo(targetPos, () =>
            {
                // Then source disappears
                if (source.View != null)
                {
                    source.View.PlayDespawnAnimation(() =>
                    {
                        grid.RemoveBlob(source);
                        ServiceLocator.Game?.CheckWinCondition();
                    });
                }
                else
                {
                    grid.RemoveBlob(source);
                    ServiceLocator.Game?.CheckWinCondition();
                }
            });

            // Flag disappears immediately
            if (target.View != null)
            {
                target.View.PlayDespawnAnimation(() =>
                {
                    grid.RemoveBlob(target);
                });
            }
            else
            {
                grid.RemoveBlob(target);
            }
        }
    }
}
