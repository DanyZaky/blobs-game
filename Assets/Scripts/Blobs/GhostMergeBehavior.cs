using UnityEngine;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Blobs
{
    /// <summary>
    /// Ghost Blob: Cannot initiate merge, but when merged INTO, it "haunts" 
    /// the tile - spawning a ghost at the source blob's original position.
    /// </summary>
    public class GhostMergeBehavior : IMergeBehavior
    {
        public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[GhostMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log($"[GhostMergeBehavior] Ghost absorbed, haunting source position");

            Vector2Int sourceOriginalPos = source.Model.GridPosition;
            Vector2Int targetPos = target.Model.GridPosition;
            BlobColor ghostColor = target.Model.Color;

            // Animate ghost being absorbed
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

            // Move source to ghost's position
            source.Deselect();
            source.MoveTo(targetPos, () =>
            {
                // Spawn new ghost at source's original position (haunting!)
                grid.SpawnBlob(sourceOriginalPos, BlobType.Ghost, ghostColor);
                Debug.Log($"[GhostMergeBehavior] New ghost spawned at {sourceOriginalPos}");

                // Check win condition
                ServiceLocator.Game?.CheckWinCondition();
            });
        }
    }
}
