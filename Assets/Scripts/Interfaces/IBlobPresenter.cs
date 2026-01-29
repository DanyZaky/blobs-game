using UnityEngine;
using Blobs.Blobs;

namespace Blobs.Interfaces
{
    /// <summary>
    /// Interface for blob presenter (business logic)
    /// </summary>
    public interface IBlobPresenter
    {
        IBlobModel Model { get; }
        IBlobView View { get; }
        
        // State
        bool IsSelected { get; }
        bool IsAnimating { get; }
        
        // Actions
        void Select();
        void Deselect();
        void MoveTo(Vector2Int targetPosition, System.Action onComplete = null);
        void ExecuteMerge(IBlobPresenter target);
        void PlayInvalidMoveEffect();
        
        // Lifecycle
        void Initialize(BlobType type, BlobColor color, Vector2Int gridPosition);
        void Dispose();
    }
}
