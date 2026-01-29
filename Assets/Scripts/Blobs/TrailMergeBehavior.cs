using UnityEngine;
using System.Collections.Generic;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Blobs
{
    /// <summary>
    /// Trail Blob: Leaves a trail of blobs behind it after merging.
    /// The trail consists of new blobs spawned along the path taken.
    /// </summary>
    public class TrailMergeBehavior : IMergeBehavior
    {
        public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[TrailMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log($"[TrailMergeBehavior] Trail blob merging, leaving trail behind");

            Vector2Int sourcePos = source.Model.GridPosition;
            Vector2Int targetPos = target.Model.GridPosition;
            BlobColor trailColor = source.Model.Color;

            // Calculate direction
            Vector2Int direction = Vector2Int.zero;
            if (targetPos.x > sourcePos.x) direction = Vector2Int.right;
            else if (targetPos.x < sourcePos.x) direction = Vector2Int.left;
            else if (targetPos.y > sourcePos.y) direction = Vector2Int.up;
            else if (targetPos.y < sourcePos.y) direction = Vector2Int.down;

            // Store positions to spawn trail (excluding source and target positions)
            List<Vector2Int> trailPositions = new List<Vector2Int>();
            Vector2Int currentPos = sourcePos + direction;

            while (currentPos != targetPos)
            {
                if (grid.IsValidPosition(currentPos) && grid.GetBlobAt(currentPos) == null)
                {
                    trailPositions.Add(currentPos);
                }
                currentPos += direction;
            }

            // Animate target being absorbed
            if (target.View != null)
            {
                Color blobColor = GetBlobColor(target);
                target.View.PlayMergeAnimation(blobColor, () =>
                {
                    grid.RemoveBlob(target);
                });
            }
            else
            {
                grid.RemoveBlob(target);
            }

            // Move source to target position
            source.Deselect();
            source.MoveTo(targetPos, () =>
            {
                // Spawn trail blobs after movement completes
                foreach (var pos in trailPositions)
                {
                    grid.SpawnBlob(pos, BlobType.Normal, trailColor);
                    Debug.Log($"[TrailMergeBehavior] Spawned trail blob at {pos}");
                }

                // Check win condition
                ServiceLocator.Game?.CheckWinCondition();
            });
        }

        private Color GetBlobColor(IBlobPresenter blob)
        {
            if (blob is BlobPresenter presenter)
            {
                return presenter.GetColor();
            }
            return Color.white;
        }
    }
}
