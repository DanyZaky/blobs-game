using UnityEngine;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Blobs
{
    /// <summary>
    /// Switch Blob: Turns corresponding laser obstacle off when merged.
    /// Cannot initiate merge.
    /// </summary>
    public class SwitchMergeBehavior : IMergeBehavior
    {
        public void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid)
        {
            if (source == null || target == null || grid == null)
            {
                Debug.LogWarning("[SwitchMergeBehavior] Invalid merge parameters");
                return;
            }

            Debug.Log("[SwitchMergeBehavior] ⚡ Switch activated!");

            // Get switch color for laser matching
            BlobColor switchColor = target.Model.Color;
            Vector2Int targetPos = target.Model.GridPosition;

            // Animate switch being absorbed
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

            // Move source to switch position
            source.Deselect();
            source.MoveTo(targetPos, () =>
            {
                Debug.Log($"[SwitchMergeBehavior] Laser of color {switchColor} should be disabled");
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
