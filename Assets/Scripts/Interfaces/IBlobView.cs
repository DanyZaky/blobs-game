using UnityEngine;
using Blobs.Blobs;

namespace Blobs.Interfaces
{
    /// <summary>
    /// Interface for blob visual representation (MonoBehaviour)
    /// </summary>
    public interface IBlobView
    {
        Transform Transform { get; }
        
        void Initialize(BlobType type, BlobColor color);
        void UpdateVisual(BlobType type, BlobColor color);
        void SetPosition(Vector3 position);
        
        // Animations
        void PlaySpawnAnimation();
        void PlaySelectAnimation();
        void PlayDeselectAnimation();
        void PlayIdleAnimation();
        void PlayMoveAnimation(Vector3 target, System.Action onComplete);
        void PlayMergeAnimation(Color color, System.Action onComplete);
        void PlayDespawnAnimation(System.Action onComplete);
        void PlayShakeAnimation();
        
        // State
        bool IsAnimating { get; }
        void SetActive(bool active);
        void Destroy();
    }
}
