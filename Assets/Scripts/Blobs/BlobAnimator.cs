using System.Collections;
using UnityEngine;

namespace Blobs.Blobs
{
    /// <summary>
    /// Handles all blob animations using coroutines.
    /// Provides smooth movement, selection effects, spawn/despawn animations.
    /// </summary>
    public class BlobAnimator : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Selection")]
        [SerializeField] private float selectScale = 1.2f;
        [SerializeField] private float selectDuration = 0.15f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.05f;

        [Header("Spawn/Despawn")]
        [SerializeField] private float spawnDuration = 0.25f;
        [SerializeField] private float despawnDuration = 0.2f;

        private Vector3 originalScale;
        private Coroutine currentMoveCoroutine;
        private Coroutine currentPulseCoroutine;
        private bool isSelected;
        private bool isAnimating;

        public bool IsAnimating => isAnimating;

        public System.Action OnMoveComplete;
        public System.Action OnDespawnComplete;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        /// <summary>
        /// Smoothly move to target position.
        /// </summary>
        public void AnimateMoveTo(Vector3 targetPosition, System.Action onComplete = null)
        {
            if (currentMoveCoroutine != null)
                StopCoroutine(currentMoveCoroutine);

            currentMoveCoroutine = StartCoroutine(MoveCoroutine(targetPosition, onComplete));
        }

        private IEnumerator MoveCoroutine(Vector3 targetPosition, System.Action onComplete)
        {
            isAnimating = true;
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            // Add a slight arc for more dynamic movement
            Vector3 midPoint = (startPosition + targetPosition) / 2f;
            midPoint.y += 0.3f; // Slight hop

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                float curveT = moveCurve.Evaluate(t);

                // Bezier curve for smooth arc movement
                Vector3 a = Vector3.Lerp(startPosition, midPoint, curveT);
                Vector3 b = Vector3.Lerp(midPoint, targetPosition, curveT);
                transform.position = Vector3.Lerp(a, b, curveT);

                yield return null;
            }

            transform.position = targetPosition;
            isAnimating = false;
            onComplete?.Invoke();
            OnMoveComplete?.Invoke();
        }

        /// <summary>
        /// Play selection animation (scale up + pulse).
        /// </summary>
        public void PlaySelectAnimation()
        {
            if (isSelected) return;
            isSelected = true;

            StopPulse();
            StartCoroutine(ScaleCoroutine(originalScale * selectScale, selectDuration, () =>
            {
                currentPulseCoroutine = StartCoroutine(PulseCoroutine());
            }));
        }

        /// <summary>
        /// Play deselection animation (scale back to normal).
        /// </summary>
        public void PlayDeselectAnimation()
        {
            if (!isSelected) return;
            isSelected = false;

            StopPulse();
            StartCoroutine(ScaleCoroutine(originalScale, selectDuration, null));
        }

        private void StopPulse()
        {
            if (currentPulseCoroutine != null)
            {
                StopCoroutine(currentPulseCoroutine);
                currentPulseCoroutine = null;
            }
        }

        private IEnumerator PulseCoroutine()
        {
            float baseScale = selectScale;
            while (isSelected)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                transform.localScale = originalScale * (baseScale + pulse);
                yield return null;
            }
        }

        private IEnumerator ScaleCoroutine(Vector3 targetScale, float duration, System.Action onComplete)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Play spawn animation (pop in from small).
        /// </summary>
        public void PlaySpawnAnimation()
        {
            StartCoroutine(SpawnCoroutine());
        }

        private IEnumerator SpawnCoroutine()
        {
            transform.localScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < spawnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / spawnDuration;
                
                // Overshoot for bouncy effect
                float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
                float scale = Mathf.Lerp(0, 1, t) * overshoot;
                
                transform.localScale = originalScale * Mathf.Min(scale, 1.1f);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Play despawn animation (shrink + fade out).
        /// </summary>
        public void PlayDespawnAnimation(System.Action onComplete = null)
        {
            StartCoroutine(DespawnCoroutine(onComplete));
        }

        private IEnumerator DespawnCoroutine(System.Action onComplete)
        {
            isAnimating = true;
            Vector3 startScale = transform.localScale;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color startColor = sr != null ? sr.color : Color.white;
            float elapsed = 0f;

            while (elapsed < despawnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / despawnDuration;

                // Shrink with spin
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                transform.rotation = Quaternion.Euler(0, 0, t * 180f);

                // Fade out
                if (sr != null)
                {
                    Color c = startColor;
                    c.a = 1f - t;
                    sr.color = c;
                }

                yield return null;
            }

            transform.localScale = Vector3.zero;
            isAnimating = false;
            onComplete?.Invoke();
            OnDespawnComplete?.Invoke();
        }

        /// <summary>
        /// Play merge animation (quick squish towards target then disappear).
        /// </summary>
        public void PlayMergeAnimation(Vector3 targetPosition, System.Action onComplete)
        {
            StartCoroutine(MergeCoroutine(targetPosition, onComplete));
        }

        private IEnumerator MergeCoroutine(Vector3 targetPosition, System.Action onComplete)
        {
            isAnimating = true;
            Vector3 startPosition = transform.position;
            Vector3 startScale = transform.localScale;
            float duration = moveDuration * 0.8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease out for snappy feel
                float easeT = 1f - Mathf.Pow(1f - t, 3f);

                transform.position = Vector3.Lerp(startPosition, targetPosition, easeT);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t * t);

                yield return null;
            }

            isAnimating = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Quick shake effect (for invalid moves).
        /// </summary>
        public void PlayShakeAnimation()
        {
            StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            Vector3 originalPos = transform.position;
            float duration = 0.3f;
            float elapsed = 0f;
            float intensity = 0.1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (elapsed / duration);
                float offsetX = Mathf.Sin(elapsed * 50f) * intensity * t;
                transform.position = originalPos + new Vector3(offsetX, 0, 0);
                yield return null;
            }

            transform.position = originalPos;
        }

        public void SetOriginalScale(Vector3 scale)
        {
            originalScale = scale;
        }
    }
}
