using UnityEngine;
using Blobs.Core;
using System.Collections.Generic;

namespace Blobs.Blobs
{
    /// <summary>
    /// Trail Blob: Leaves a trail of blobs behind it after merging.
    /// The trail consists of new blobs spawned along the path taken.
    /// </summary>
    public class TrailMergeBehavior : IMergeBehavior
    {
        public void OnMerge(Blob source, Blob target, GridManager grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[TrailMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log($"[TrailMergeBehavior] Trail blob merging, leaving trail behind");

            Vector2Int sourcePos = source.CurrentTile.GridPosition;
            Vector2Int targetPos = target.CurrentTile.GridPosition;
            BlobColor trailColor = source.BlobColorType;

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
            Tile targetTile = target.CurrentTile;
            BlobAnimator targetAnimator = target.GetComponent<BlobAnimator>();
            
            if (targetAnimator != null)
            {
                targetAnimator.PlayMergeAnimation(source.transform.position, () =>
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
            source.MoveTo(targetTile, () =>
            {
                // Spawn trail blobs after movement completes
                foreach (var pos in trailPositions)
                {
                    grid.SpawnBlob(pos, trailColor, BlobType.Normal);
                    Debug.Log($"[TrailMergeBehavior] Spawned trail blob at {pos}");
                }

                // Check win condition
                GameManager.Instance?.CheckWinCondition();
            });
        }
    }
}
