using UnityEngine;
using Blobs.Core;

namespace Blobs.Blobs
{
    /// <summary>
    /// Flag Blob: Goal point. A blob of the SAME COLOR can merge with it
    /// to clear BOTH blobs. Only works when no other playable blobs are on the board.
    /// (Rock obstacles don't count). Cannot initiate merge.
    /// </summary>
    public class FlagMergeBehavior : IMergeBehavior
    {
        public void OnMerge(Blob source, Blob target, GridManager grid)
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
            if (source.BlobColorType != target.BlobColorType)
            {
                Debug.Log("[FlagMergeBehavior] Cannot merge - flag requires same color!");
                source.PlayInvalidMoveEffect();
                return;
            }

            Debug.Log("[FlagMergeBehavior] 🚩 Flag cleared successfully!");

            // Animate both blobs disappearing together
            BlobAnimator sourceAnimator = source.GetComponent<BlobAnimator>();
            BlobAnimator targetAnimator = target.GetComponent<BlobAnimator>();

            source.Deselect();

            // Move source to flag position first
            Tile flagTile = target.CurrentTile;
            source.MoveTo(flagTile, () =>
            {
                // Then both disappear
                if (sourceAnimator != null)
                {
                    sourceAnimator.PlayDespawnAnimation(() =>
                    {
                        grid.RemoveBlob(source);
                        GameManager.Instance?.CheckWinCondition();
                    });
                }
                else
                {
                    grid.RemoveBlob(source);
                    GameManager.Instance?.CheckWinCondition();
                }
            });

            // Flag disappears immediately
            if (targetAnimator != null)
            {
                targetAnimator.PlayDespawnAnimation(() =>
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
