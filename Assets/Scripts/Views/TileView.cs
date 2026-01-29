using UnityEngine;
using Blobs.Core;

namespace Blobs.Views
{
    /// <summary>
    /// View component for tile visual representation.
    /// Handles only visual concerns: colors, highlights.
    /// </summary>
    public class TileView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.25f, 0.4f, 1f);
        [SerializeField] private Color highlightColor = new Color(0.4f, 0.35f, 0.5f, 1f);
        [SerializeField] private Color goalColor = new Color(0.4f, 0.5f, 0.3f, 1f);
        [SerializeField] private Color blockedColor = new Color(0.2f, 0.15f, 0.25f, 1f);
        [SerializeField] private Color iceColor = new Color(0.6f, 0.8f, 0.9f, 1f);
        [SerializeField] private Color stickyColor = new Color(0.6f, 0.4f, 0.3f, 1f);

        private TileType currentType = TileType.Normal;

        public Vector2Int GridPosition { get; private set; }

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Vector2Int position)
        {
            GridPosition = position;
            gameObject.name = $"Tile_{position.x}_{position.y}";
            UpdateVisual();
        }

        public void SetTileType(TileType type)
        {
            currentType = type;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.color = currentType switch
            {
                TileType.Normal => normalColor,
                TileType.Blocked => blockedColor,
                TileType.Goal => goalColor,
                TileType.Ice => iceColor,
                TileType.Sticky => stickyColor,
                _ => normalColor
            };
        }

        public void Highlight()
        {
            if (spriteRenderer != null)
                spriteRenderer.color = highlightColor;
        }

        public void ResetColor()
        {
            UpdateVisual();
        }

        public void SetColor(Color color)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = color;
        }
    }
}
