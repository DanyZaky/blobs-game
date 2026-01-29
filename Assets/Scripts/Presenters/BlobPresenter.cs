using UnityEngine;
using Blobs.Blobs;
using Blobs.Interfaces;
using Blobs.Models;
using Blobs.Views;

namespace Blobs.Presenters
{
    /// <summary>
    /// Presenter for individual blob logic.
    /// Coordinates between BlobModel (data) and BlobView (visual).
    /// </summary>
    public class BlobPresenter : IBlobPresenter
    {
        private readonly BlobModel _model;
        private readonly IBlobView _view;
        private readonly IMergeBehavior _mergeBehavior;
        private bool _isSelected;

        public IBlobModel Model => _model;
        public IBlobView View => _view;
        public bool IsSelected => _isSelected;
        public bool IsAnimating => _view?.IsAnimating ?? false;

        public BlobPresenter(BlobModel model, IBlobView view, IMergeBehavior mergeBehavior)
        {
            _model = model;
            _view = view;
            _mergeBehavior = mergeBehavior;
        }

        public void Initialize(BlobType type, BlobColor color, Vector2Int gridPosition)
        {
            _model.SetType(type);
            _model.SetColor(color);
            _model.GridPosition = gridPosition;
            _view?.Initialize(type, color);
        }

        public void Select()
        {
            if (_isSelected || !_model.CanInitiateMerge) return;

            _isSelected = true;
            _view?.PlaySelectAnimation();
        }

        public void Deselect()
        {
            if (!_isSelected) return;

            _isSelected = false;
            _view?.PlayDeselectAnimation();
        }

        public void MoveTo(Vector2Int targetPosition, System.Action onComplete = null)
        {
            _model.GridPosition = targetPosition;

            if (_view != null)
            {
                // Get world position from ServiceLocator
                Vector3 worldPos = ServiceLocator.Grid?.GridToWorldPosition(targetPosition) ?? Vector3.zero;
                _view.PlayMoveAnimation(worldPos, () =>
                {
                    _view.PlayIdleAnimation();
                    onComplete?.Invoke();
                });
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void ExecuteMerge(IBlobPresenter target)
        {
            if (_mergeBehavior == null)
            {
                Debug.LogWarning("[BlobPresenter] No merge behavior assigned!");
                return;
            }

            IGridPresenter grid = ServiceLocator.Grid;
            if (grid == null)
            {
                Debug.LogWarning("[BlobPresenter] Grid not found in ServiceLocator!");
                return;
            }

            Debug.Log($"[BlobPresenter] ExecuteMerge from {_model.Type} to {target.Model.Type}");
            
            // Delegate to the merge behavior (Strategy Pattern - SOLID OCP)
            _mergeBehavior.OnMerge(this, target, grid);
        }

        public void PlayInvalidMoveEffect()
        {
            _view?.PlayShakeAnimation();
        }

        public void Dispose()
        {
            _view?.Destroy();
        }

        // Helper to get color for particles
        public Color GetColor()
        {
            if (_view is BlobView blobView)
            {
                return blobView.GetBaseColor(_model.Color);
            }
            return Color.white;
        }
    }
}
