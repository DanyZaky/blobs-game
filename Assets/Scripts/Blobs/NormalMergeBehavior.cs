using UnityEngine;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Blobs
{
    /// <summary>
    /// Default merge behavior for normal blobs.
    /// Source blob moves to target's position, target is absorbed.
    /// </summary>
    public class NormalMergeBehavior : IMergeBehavior
    {
        public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[NormalMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log($"[NormalMergeBehavior] Merging {source.Model.Color} into {target.Model.Color}");

            // Store target position
            Vector2Int targetPos = target.Model.GridPosition;

            // Animate target being absorbed (via view)
            if (target.View != null)
            {
                Color blobColor = GetBlobColor(target);
                target.View.PlayMergeAnimation(blobColor, () =>
                {
                    // Remove target after animation
                    grid.RemoveBlob(target);
                });
            }
            else
            {
                grid.RemoveBlob(target);
            }

            // Move source to target's position
            source.Deselect();
            source.MoveTo(targetPos, () =>
            {
                // Check win condition after animation completes
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
