using UnityEngine;
using Blobs.Core;

namespace Blobs.Blobs
{
    /// <summary>
    /// Default merge behavior for normal blobs.
    /// Source blob moves to target's position with smooth animation, target is absorbed.
    /// </summary>
    public class NormalMergeBehavior : IMergeBehavior
    {
        public void OnMerge(Blob source, Blob target, GridManager grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[NormalMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log($"[NormalMergeBehavior] Merging {source.BlobColorType} into {target.BlobColorType}");

            // Store target tile before animations
            Tile targetTile = target.CurrentTile;
            Vector3 targetPosition = target.transform.position;

            // Get animator from target for despawn
            BlobAnimator targetAnimator = target.GetComponent<BlobAnimator>();
            
            // Animate target being absorbed (shrink)
            if (targetAnimator != null)
            {
                targetAnimator.PlayMergeAnimation(source.transform.position, () =>
                {
                    // Remove target after animation
                    grid.RemoveBlob(target);
                });
            }
            else
            {
                grid.RemoveBlob(target);
            }

            // Move source to target's position with animation
            source.Deselect();
            source.MoveTo(targetTile, () =>
            {
                // Check win condition after animation completes
                GameManager.Instance?.CheckWinCondition();
            });
        }
    }
}
