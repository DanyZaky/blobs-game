using UnityEngine;
using Blobs.Core;
using System.Collections;

namespace Blobs.Blobs
{
    public enum BlobColor
    {
        Pink,
        Blue,
        Red,
        Cyan,
        Green,
        Yellow,
        White,  // For Ghost blob
        Gray    // For Rock blob
    }

    public enum BlobType
    {
        Normal,     // Standard blob - can merge with different colors
        Trail,      // Leaves trail of blobs behind after merging
        Ghost,      // Haunts another blob's space after merging (cannot initiate)
        Flag,       // Goal point - same color blob clears both (cannot initiate)
        Rock,       // Obstacle - blocks direct merges (cannot initiate)
        Switch      // Turns laser off (cannot initiate)
    }

    public class Blob : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private BlobType blobType = BlobType.Normal;
        [SerializeField] private BlobColor blobColor = BlobColor.Pink;
        [SerializeField] private int size = 1;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Tile currentTile;
        private IMergeBehavior mergeBehavior;
        private BlobAnimator animator;
        private bool isSelected;
        private Vector3 originalScale;

        // Properties
        public BlobType Type => blobType;
        public BlobColor BlobColorType => blobColor;
        public int Size => size;
        public Tile CurrentTile => currentTile;
        public bool IsSelected => isSelected;
        public IMergeBehavior MergeBehavior => mergeBehavior;
        public bool IsAnimating => animator != null && animator.IsAnimating;

        // Derived from type
        public bool CanInitiateMerge => blobType == BlobType.Normal || blobType == BlobType.Trail;

        private static readonly Color[] ColorPalette = new Color[]
        {
            new Color(1f, 0.5f, 0.7f),      // Pink
            new Color(0.4f, 0.7f, 1f),      // Blue
            new Color(1f, 0.4f, 0.4f),      // Red
            new Color(0.4f, 0.9f, 0.9f),    // Cyan
            new Color(0.5f, 0.9f, 0.5f),    // Green
            new Color(1f, 0.9f, 0.4f),      // Yellow
            new Color(1f, 1f, 1f),          // White (Ghost)
            new Color(0.5f, 0.5f, 0.5f)     // Gray (Rock)
        };

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Get or add animator
            animator = GetComponent<BlobAnimator>();
            if (animator == null)
                animator = gameObject.AddComponent<BlobAnimator>();
            
            originalScale = transform.localScale;
        }

        public void Initialize(BlobType type, BlobColor color, Tile tile)
        {
            blobType = type;
            blobColor = color;
            currentTile = tile;
            
            // Assign behavior based on type
            AssignBehavior();
            UpdateVisual();
            
            // Setup animator
            if (animator != null)
            {
                animator.SetOriginalScale(originalScale);
                animator.PlaySpawnAnimation();
            }
            
            gameObject.name = $"Blob_{type}_{color}";
        }

        // Simplified initialize for normal blobs
        public void Initialize(BlobColor color, Tile tile)
        {
            Initialize(BlobType.Normal, color, tile);
        }

        private void AssignBehavior()
        {
            mergeBehavior = blobType switch
            {
                BlobType.Normal => new NormalMergeBehavior(),
                BlobType.Trail => new TrailMergeBehavior(),
                BlobType.Ghost => new GhostMergeBehavior(),
                BlobType.Flag => new FlagMergeBehavior(),
                BlobType.Rock => new RockMergeBehavior(),
                BlobType.Switch => new SwitchMergeBehavior(),
                _ => new NormalMergeBehavior()
            };
        }

        public void SetMergeBehavior(IMergeBehavior behavior)
        {
            mergeBehavior = behavior;
        }

        public void SetCurrentTile(Tile tile)
        {
            currentTile = tile;
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            // Set base color
            Color baseColor = ColorPalette[(int)blobColor];
            
            // Modify appearance based on type
            switch (blobType)
            {
                case BlobType.Ghost:
                    baseColor = new Color(1f, 1f, 1f, 0.7f); // Transparent white
                    break;
                case BlobType.Rock:
                    baseColor = new Color(0.4f, 0.35f, 0.3f); // Rocky brown-gray
                    break;
                case BlobType.Switch:
                    baseColor = new Color(0.8f, 0.6f, 0.2f); // Golden/switch color
                    break;
            }
            
            spriteRenderer.color = baseColor;
        }

        public void Select()
        {
            if (isSelected || !CanInitiateMerge) return;
            
            isSelected = true;
            
            if (animator != null)
                animator.PlaySelectAnimation();
            else
                transform.localScale = originalScale * 1.2f;
        }

        public void Deselect()
        {
            if (!isSelected) return;
            
            isSelected = false;
            
            if (animator != null)
                animator.PlayDeselectAnimation();
            else
                transform.localScale = originalScale;
        }

        /// <summary>
        /// Move to target tile with smooth animation.
        /// </summary>
        public void MoveTo(Tile targetTile, System.Action onComplete = null)
        {
            if (currentTile != null)
            {
                currentTile.ClearBlob();
            }

            currentTile = targetTile;
            targetTile.SetBlob(this);
            
            // Animate movement
            if (animator != null)
            {
                animator.AnimateMoveTo(targetTile.transform.position, onComplete);
            }
            else
            {
                transform.position = targetTile.transform.position;
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Legacy MoveTo without callback for backward compatibility.
        /// </summary>
        public void MoveTo(Tile targetTile)
        {
            MoveTo(targetTile, null);
        }

        public void ExecuteMerge(Blob target, GridManager grid)
        {
            mergeBehavior?.OnMerge(this, target, grid);
        }

        public bool CanMergeWith(Blob other)
        {
            if (other == null) return false;
            if (other == this) return false;
            
            // Rock cannot be merged with
            if (other.blobType == BlobType.Rock) return false;
            
            // Flag requires same color
            if (other.blobType == BlobType.Flag)
            {
                return other.blobColor == this.blobColor;
            }
            
            // Normal rule: different colors can merge
            if (other.blobColor == this.blobColor) return false;
            
            return true;
        }

        /// <summary>
        /// Play shake animation for invalid moves.
        /// </summary>
        public void PlayInvalidMoveEffect()
        {
            if (animator != null)
                animator.PlayShakeAnimation();
        }

        public void SetSize(int newSize)
        {
            size = newSize;
        }

        public Color GetColor()
        {
            return ColorPalette[(int)blobColor];
        }

        public void SetType(BlobType type)
        {
            blobType = type;
            AssignBehavior();
            UpdateVisual();
        }
    }
}
