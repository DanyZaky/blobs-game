using UnityEngine;
using Blobs.Core;

namespace Blobs.Blobs
{
    /// <summary>
    /// Switch Blob: Turns corresponding laser obstacle off when merged.
    /// Cannot initiate merge.
    /// </summary>
    public class SwitchMergeBehavior : IMergeBehavior
    {
        public void OnMerge(Blob source, Blob target, GridManager grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[SwitchMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log("[SwitchMergeBehavior] ⚡ Switch activated!");

            // Get switch color for laser matching
            BlobColor switchColor = target.BlobColorType;
            Tile targetTile = target.CurrentTile;

            // Animate switch being absorbed
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

            // Move source to switch position
            source.Deselect();
            source.MoveTo(targetTile, () =>
            {
                Debug.Log($"[SwitchMergeBehavior] Laser of color {switchColor} should be disabled");
                GameManager.Instance?.CheckWinCondition();
            });
        }
    }
}
