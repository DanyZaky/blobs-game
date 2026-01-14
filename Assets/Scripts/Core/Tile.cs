using UnityEngine;
using Blobs.Blobs;

namespace Blobs.Core
{
    public class Tile : MonoBehaviour
    {
        [Header("Position")]
        [SerializeField] private Vector2Int gridPosition;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = new Color(0.3f, 0.25f, 0.4f, 1f);
        [SerializeField] private Color highlightColor = new Color(0.4f, 0.35f, 0.5f, 1f);

        private Blob currentBlob;

        public Vector2Int GridPosition => gridPosition;
        public Blob CurrentBlob => currentBlob;
        public bool IsOccupied => currentBlob != null;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Vector2Int position)
        {
            gridPosition = position;
            gameObject.name = $"Tile_{position.x}_{position.y}";
            SetNormalColor();
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
            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;
        }

        public void SetColor(Color color)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = color;
        }
    }
}
