using UnityEngine;
using Blobs.Blobs;

namespace Blobs.Core
{
    public class Tile : MonoBehaviour
    {
        [Header("Position")]
        [SerializeField] private Vector2Int gridPosition;
        
        [Header("Type")]
        [SerializeField] private TileType tileType = TileType.Normal;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = new Color(0.3f, 0.25f, 0.4f, 1f);
        [SerializeField] private Color highlightColor = new Color(0.4f, 0.35f, 0.5f, 1f);
        [SerializeField] private Color goalColor = new Color(0.4f, 0.5f, 0.3f, 1f);
        [SerializeField] private Color blockedColor = new Color(0.2f, 0.15f, 0.25f, 1f);

        private Blob currentBlob;

        public Vector2Int GridPosition => gridPosition;
        public Blob CurrentBlob => currentBlob;
        public bool IsOccupied => currentBlob != null;
        public TileType Type => tileType;
        public bool IsBlocked => tileType == TileType.Blocked;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Vector2Int position)
        {
            gridPosition = position;
            gameObject.name = $"Tile_{position.x}_{position.y}";
            UpdateVisual();
        }

        public void SetTileType(TileType type)
        {
            tileType = type;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.color = tileType switch
            {
                TileType.Normal => normalColor,
                TileType.Blocked => blockedColor,
                TileType.Goal => goalColor,
                TileType.Ice => new Color(0.6f, 0.8f, 0.9f, 1f),
                TileType.Sticky => new Color(0.6f, 0.4f, 0.3f, 1f),
                _ => normalColor
            };
        }

        public void SetBlob(Blob blob)
        {
            currentBlob = blob;
            if (blob != null)
            {
                blob.SetCurrentTile(this);
            }
        }

        public void ClearBlob()
        {
            currentBlob = null;
        }

        public void Highlight()
        {
            if (spriteRenderer != null)
                spriteRenderer.color = highlightColor;
        }

        public void SetNormalColor()
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
