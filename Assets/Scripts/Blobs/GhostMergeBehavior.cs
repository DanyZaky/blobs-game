using UnityEngine;
using Blobs.Core;

namespace Blobs.Blobs
{
    /// <summary>
    /// Ghost Blob: Cannot initiate merge, but when merged INTO, it "haunts" 
    /// the tile - spawning a ghost at the source blob's original position.
    /// </summary>
    public class GhostMergeBehavior : IMergeBehavior
    {
        public void OnMerge(Blob source, Blob target, GridManager grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[GhostMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log($"[GhostMergeBehavior] Ghost absorbed, haunting source position");

            Vector2Int sourceOriginalPos = source.CurrentTile.GridPosition;
            Tile targetTile = target.CurrentTile;
            BlobColor ghostColor = target.BlobColorType;

            // Animate ghost being absorbed
            BlobAnimator targetAnimator = target.GetComponent<BlobAnimator>();
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

            // Move source to ghost's position
            source.Deselect();
            source.MoveTo(targetTile, () =>
            {
                // Spawn new ghost at source's original position (haunting!)
                grid.SpawnBlob(sourceOriginalPos, ghostColor, BlobType.Ghost);
                Debug.Log($"[GhostMergeBehavior] New ghost spawned at {sourceOriginalPos}");

                // Check win condition
                GameManager.Instance?.CheckWinCondition();
            });
        }
    }
}
