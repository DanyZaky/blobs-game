using UnityEngine;
using Blobs.Blobs;
using Blobs.Interfaces;

namespace Blobs.Views
{
    /// <summary>
    /// View component for blob visual representation.
    /// Handles only visual concerns: sprites, colors, animations.
    /// </summary>
    [RequireComponent(typeof(BlobAnimator))]
    public class BlobView : MonoBehaviour, IBlobView
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private BlobAnimator animator;
        private MaterialPropertyBlock materialPropertyBlock;
        private Vector3 originalScale;

        // Current state for tooltips
        private BlobType currentType;
        private BlobColor currentColor;

        // Shader property IDs
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ShadowColorProperty = Shader.PropertyToID("_ShadowColor");

        // Color palettes
        private static readonly Color[] BaseColorPalette = new Color[]
        {
            HexToColor("FF80B3"),   // Pink
            HexToColor("47BDFF"),   // Blue
            HexToColor("FF6666"),   // Red
            HexToColor("66E6E6"),   // Cyan
            HexToColor("80E680"),   // Green
            HexToColor("FFE666"),   // Yellow
            HexToColor("FFFFFF"),   // White
            HexToColor("808080")    // Gray
        };

        private static readonly Color[] ShadowColorPalette = new Color[]
        {
            HexToColor("8B0045"),   // Pink shadow
            HexToColor("005057"),   // Blue shadow
            HexToColor("8B0000"),   // Red shadow
            HexToColor("005757"),   // Cyan shadow
            HexToColor("006600"),   // Green shadow
            HexToColor("8B7300"),   // Yellow shadow
            HexToColor("808080"),   // White shadow
            HexToColor("404040")    // Gray shadow
        };

        public Transform Transform => transform;
        public bool IsAnimating => animator != null && animator.IsAnimating;

        private static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            animator = GetComponent<BlobAnimator>();
            if (animator == null)
                animator = gameObject.AddComponent<BlobAnimator>();

            materialPropertyBlock = new MaterialPropertyBlock();
            originalScale = transform.localScale;
        }

        public void Initialize(BlobType type, BlobColor color)
        {
            currentType = type;
            currentColor = color;
            UpdateVisual(type, color);
            animator.SetOriginalScale(originalScale);
        }

        public void UpdateVisual(BlobType type, BlobColor color)
        {
            if (spriteRenderer == null) return;

            currentType = type;
            currentColor = color;

            spriteRenderer.GetPropertyBlock(materialPropertyBlock);

            Color baseColor;
            Color shadowColor;

            switch (type)
            {
                case BlobType.Ghost:
                    baseColor = HexToColor("FFFFFF");
                    shadowColor = HexToColor("808080");
                    spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
                    break;
                case BlobType.Rock:
                    baseColor = HexToColor("665A54");
                    shadowColor = HexToColor("332D2A");
                    spriteRenderer.color = Color.white;
                    break;
                case BlobType.Switch:
                    baseColor = HexToColor("CC9933");
                    shadowColor = HexToColor("664D1A");
                    spriteRenderer.color = Color.white;
                    break;
                default:
                    baseColor = BaseColorPalette[(int)color];
                    shadowColor = ShadowColorPalette[(int)color];
                    spriteRenderer.color = Color.white;
                    break;
            }

            materialPropertyBlock.SetColor(BaseColorProperty, baseColor);
            materialPropertyBlock.SetColor(ShadowColorProperty, shadowColor);
            spriteRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void PlaySpawnAnimation()
        {
            animator?.PlaySpawnAnimation();
        }

        public void PlaySelectAnimation()
        {
            animator?.PlaySelectAnimation();
        }

        public void PlayDeselectAnimation()
        {
            animator?.PlayDeselectAnimation();
        }

        public void PlayIdleAnimation()
        {
            animator?.StartIdleAnimation();
        }

        public void PlayMoveAnimation(Vector3 target, System.Action onComplete)
        {
            if (animator != null)
                animator.AnimateMoveTo(target, onComplete);
            else
            {
                transform.position = target;
                onComplete?.Invoke();
            }
        }

        public void PlayMergeAnimation(Color color, System.Action onComplete)
        {
            // BlobAnimator expects (Vector3, Action, Color) - use current position as target
            animator?.PlayMergeAnimation(transform.position, onComplete, color);
        }

        public void PlayDespawnAnimation(System.Action onComplete)
        {
            animator?.PlayDespawnAnimation(onComplete);
        }

        public void PlayShakeAnimation()
        {
            animator?.PlayShakeAnimation();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void Destroy()
        {
            if (gameObject != null)
                Object.Destroy(gameObject);
        }

        public Color GetBaseColor(BlobColor color)
        {
            return BaseColorPalette[(int)color];
        }

        /// <summary>
        /// Get tooltip information for this blob
        /// </summary>
        public (string title, string description, string hint) GetTooltipInfo()
        {
            string title = $"{currentColor} {currentType}";
            string description = "";
            string hint = "";

            switch (currentType)
            {
                case BlobType.Normal:
                    description = "A normal blob that can merge with different colors.";
                    hint = "Click to select, then use arrow keys to merge.";
                    break;
                case BlobType.Trail:
                    description = "Leaves a trail of blobs behind when merging.";
                    hint = "Trail blobs spawn along the path.";
                    break;
                case BlobType.Ghost:
                    description = "Cannot initiate merge. Haunts the source position.";
                    hint = "A new ghost appears where you started.";
                    break;
                case BlobType.Flag:
                    description = "Goal blob. Merge same color when alone.";
                    hint = "Clear all other blobs first!";
                    break;
                case BlobType.Rock:
                    description = "Immovable obstacle. Cannot be merged.";
                    hint = "Find a way around it.";
                    break;
                case BlobType.Switch:
                    description = "Deactivates lasers when merged.";
                    hint = "Match the color to disable lasers.";
                    break;
            }

            return (title, description, hint);
        }
    }
}

