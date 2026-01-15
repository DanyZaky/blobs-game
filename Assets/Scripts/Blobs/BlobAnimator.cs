using UnityEngine;
using DG.Tweening;

namespace Blobs.Blobs
{
    /// <summary>
    /// Handles all blob animations using DOTween.
    /// Provides smooth state-based animations: idle, selected, moving, merging.
    /// </summary>
    public class BlobAnimator : MonoBehaviour
    {
        public enum BlobState
        {
            Idle,
            Selected,
            Moving,
            Merging
        }

        [Header("Idle Animation")]
        [SerializeField] private float idleScaleAmount = 0.02f;
        [SerializeField] private float idleScaleDuration = 1.5f;
        [SerializeField] private float idleFloatAmount = 0.03f;
        [SerializeField] private float idleFloatDuration = 2f;

        [Header("Selected Animation")]
        [SerializeField] private float selectedScaleAmount = 0.12f;
        [SerializeField] private float selectedScaleDuration = 0.5f;
        [SerializeField] private float selectedRotationAmount = 3f;
        [SerializeField] private float selectedRotationDuration = 0.3f;

        [Header("Movement")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private float moveArcHeight = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        [Header("Spawn/Despawn")]
        [SerializeField] private float spawnDuration = 0.25f;
        [SerializeField] private float despawnDuration = 0.2f;

        [Header("Merge")]
        [SerializeField] private float mergeDuration = 0.25f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem mergeParticlePrefab;

        private Vector3 originalScale;
        private Vector3 originalPosition;
        private BlobState currentState = BlobState.Idle;
        private bool isAnimating;

        // DOTween sequences
        private Sequence idleSequence;
        private Sequence selectedSequence;
        private Tween currentMoveTween;

        public bool IsAnimating => isAnimating;
        public BlobState CurrentState => currentState;

        public System.Action OnMoveComplete;
        public System.Action OnDespawnComplete;

        private void Awake()
        {
            originalScale = transform.localScale;
            originalPosition = transform.localPosition;
        }

        private void Start()
        {
            // Start idle animation by default
            StartIdleAnimation();
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        private void KillAllTweens()
        {
            idleSequence?.Kill();
            selectedSequence?.Kill();
            currentMoveTween?.Kill();
            transform.DOKill();
        }

        #region State Animations

        /// <summary>
        /// Start idle breathing/floating animation
        /// </summary>
        public void StartIdleAnimation()
        {
            if (currentState == BlobState.Idle) return;

            KillAllTweens();
            currentState = BlobState.Idle;

            // Reset to original state
            transform.localScale = originalScale;
            transform.localRotation = Quaternion.identity;

            // Create idle sequence - gentle breathing + floating
            idleSequence = DOTween.Sequence();

            // Breathing: scale pulse
            idleSequence.Append(
                transform.DOScale(originalScale * (1f + idleScaleAmount), idleScaleDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );
            idleSequence.Append(
                transform.DOScale(originalScale * (1f - idleScaleAmount * 0.5f), idleScaleDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );

            // Also add subtle float movement
            transform.DOLocalMoveY(originalPosition.y + idleFloatAmount, idleFloatDuration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            idleSequence.SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Start selected pulsing animation
        /// </summary>
        public void StartSelectedAnimation()
        {
            if (currentState == BlobState.Selected) return;

            KillAllTweens();
            currentState = BlobState.Selected;

            // Scale up quickly first
            transform.DOScale(originalScale * (1f + selectedScaleAmount), 0.1f)
                .SetEase(Ease.OutBack);

            // Create selected sequence - bigger pulse + rotation wobble
            selectedSequence = DOTween.Sequence();

            // Pulsing
            selectedSequence.Append(
                transform.DOScale(originalScale * (1f + selectedScaleAmount), selectedScaleDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );
            selectedSequence.Append(
                transform.DOScale(originalScale * (1f + selectedScaleAmount * 0.5f), selectedScaleDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );

            // Rotation wobble
            transform.DORotate(new Vector3(0, 0, selectedRotationAmount), selectedRotationDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            selectedSequence.SetLoops(-1, LoopType.Restart);
        }

        #endregion

        #region Selection

        public void PlaySelectAnimation()
        {
            StartSelectedAnimation();
        }

        public void PlayDeselectAnimation()
        {
            // Reset rotation
            transform.DORotate(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
            StartIdleAnimation();
        }

        #endregion

        #region Movement

        /// <summary>
        /// Smoothly move to target position with arc
        /// </summary>
        public void AnimateMoveTo(Vector3 targetPosition, System.Action onComplete = null)
        {
            KillAllTweens();
            currentState = BlobState.Moving;
            isAnimating = true;

            Vector3 startPosition = transform.position;

            // Create arc path
            Vector3[] path = new Vector3[3];
            path[0] = startPosition;
            path[1] = (startPosition + targetPosition) / 2f + Vector3.up * moveArcHeight;
            path[2] = targetPosition;

            currentMoveTween = transform.DOPath(path, moveDuration, PathType.CatmullRom)
                .SetEase(moveEase)
                .OnComplete(() =>
                {
                    isAnimating = false;
                    originalPosition = transform.localPosition;
                    StartIdleAnimation();
                    onComplete?.Invoke();
                    OnMoveComplete?.Invoke();
                });
        }

        #endregion

        #region Spawn/Despawn

        public void PlaySpawnAnimation()
        {
            KillAllTweens();

            transform.localScale = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            Sequence spawnSeq = DOTween.Sequence();

            // Pop in with overshoot
            spawnSeq.Append(
                transform.DOScale(originalScale * 1.15f, spawnDuration * 0.6f)
                    .SetEase(Ease.OutBack)
            );
            spawnSeq.Append(
                transform.DOScale(originalScale, spawnDuration * 0.4f)
                    .SetEase(Ease.OutBounce)
            );

            spawnSeq.OnComplete(() =>
            {
                StartIdleAnimation();
            });
        }

        public void PlayDespawnAnimation(System.Action onComplete = null)
        {
            KillAllTweens();
            currentState = BlobState.Merging;
            isAnimating = true;

            Sequence despawnSeq = DOTween.Sequence();

            // Shrink + spin + fade
            despawnSeq.Append(
                transform.DOScale(Vector3.zero, despawnDuration)
                    .SetEase(Ease.InBack)
            );
            despawnSeq.Join(
                transform.DORotate(new Vector3(0, 0, 180f), despawnDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.InQuad)
            );

            // Fade out sprite
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                despawnSeq.Join(
                    sr.DOFade(0f, despawnDuration)
                        .SetEase(Ease.InQuad)
                );
            }

            despawnSeq.OnComplete(() =>
            {
                isAnimating = false;
                onComplete?.Invoke();
                OnDespawnComplete?.Invoke();
            });
        }

        #endregion

        #region Merge Animation

        /// <summary>
        /// Play merge animation - squish towards target then spawn particles
        /// </summary>
        public void PlayMergeAnimation(Vector3 targetPosition, System.Action onComplete, Color? blobColor = null)
        {
            KillAllTweens();
            currentState = BlobState.Merging;
            isAnimating = true;

            Vector3 startPosition = transform.position;

            Sequence mergeSeq = DOTween.Sequence();

            // Squish towards target (stretch in direction of movement)
            Vector3 direction = (targetPosition - startPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Stretch effect
            mergeSeq.Append(
                transform.DOScale(new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z), mergeDuration * 0.3f)
                    .SetEase(Ease.OutQuad)
            );

            // Move to target while shrinking
            mergeSeq.Append(
                transform.DOMove(targetPosition, mergeDuration * 0.7f)
                    .SetEase(Ease.InQuad)
            );
            mergeSeq.Join(
                transform.DOScale(Vector3.zero, mergeDuration * 0.7f)
                    .SetEase(Ease.InQuad)
            );

            mergeSeq.OnComplete(() =>
            {
                // Spawn merge particles
                SpawnMergeParticles(targetPosition, blobColor ?? Color.white);

                isAnimating = false;
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Legacy merge animation without color parameter
        /// </summary>
        public void PlayMergeAnimation(Vector3 targetPosition, System.Action onComplete)
        {
            PlayMergeAnimation(targetPosition, onComplete, null);
        }

        private void SpawnMergeParticles(Vector3 position, Color color)
        {
            if (mergeParticlePrefab == null) return;

            ParticleSystem particles = Instantiate(mergeParticlePrefab, position, Quaternion.identity);

            // Set particle color to match blob
            var main = particles.main;
            main.startColor = color;

            particles.Play();

            // Auto-destroy after particles finish
            Destroy(particles.gameObject, main.duration + main.startLifetime.constantMax);
        }

        #endregion

        #region Effects

        /// <summary>
        /// Quick shake effect for invalid moves
        /// </summary>
        public void PlayShakeAnimation()
        {
            // Don't interrupt important animations
            if (currentState == BlobState.Moving || currentState == BlobState.Merging) return;

            transform.DOShakePosition(0.3f, 0.1f, 20, 90, false, true)
                .OnComplete(() =>
                {
                    // Return to current state animation
                    if (currentState == BlobState.Selected)
                        StartSelectedAnimation();
                    else
                        StartIdleAnimation();
                });
        }

        #endregion

        #region Utility

        public void SetOriginalScale(Vector3 scale)
        {
            originalScale = scale;
        }

        public void SetMergeParticlePrefab(ParticleSystem prefab)
        {
            mergeParticlePrefab = prefab;
        }

        /// <summary>
        /// Force reset to idle state
        /// </summary>
        public void ResetToIdle()
        {
            KillAllTweens();
            transform.localScale = originalScale;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = originalPosition;
            isAnimating = false;
            currentState = BlobState.Idle;
            StartIdleAnimation();
        }

        #endregion
    }
}
