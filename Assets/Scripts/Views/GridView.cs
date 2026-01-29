using UnityEngine;
using System.Collections.Generic;
using Blobs.Blobs;
using Blobs.Core;

namespace Blobs.Views
{
    /// <summary>
    /// View component for grid visual representation.
    /// Handles tile creation and blob view spawning.
    /// </summary>
    public class GridView : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float tileSize = 1.2f;
        [SerializeField] private float tileSpacing = 0.1f;

        [Header("Prefabs")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject normalBlobPrefab;
        [SerializeField] private GameObject trailBlobPrefab;
        [SerializeField] private GameObject ghostBlobPrefab;
        [SerializeField] private GameObject flagBlobPrefab;
        [SerializeField] private GameObject rockBlobPrefab;
        [SerializeField] private GameObject switchBlobPrefab;

        private Dictionary<Vector2Int, TileView> tileViews = new Dictionary<Vector2Int, TileView>();
        private int gridWidth;
        private int gridHeight;

        public float TileSize => tileSize;
        public float TileSpacing => tileSpacing;

        public void CreateTileViews(int width, int height)
        {
            ClearTileViews();
            gridWidth = width;
            gridHeight = height;

            float totalWidth = width * (tileSize + tileSpacing) - tileSpacing;
            float totalHeight = height * (tileSize + tileSpacing) - tileSpacing;
            Vector3 startPos = new Vector3(-totalWidth / 2 + tileSize / 2, -totalHeight / 2 + tileSize / 2, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 worldPos = startPos + new Vector3(
                        x * (tileSize + tileSpacing),
                        y * (tileSize + tileSpacing),
                        0
                    );

                    GameObject tileObj;
                    if (tilePrefab != null)
                    {
                        tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                    }
                    else
                    {
                        tileObj = CreateDefaultTile(worldPos);
                    }

                    TileView tileView = tileObj.GetComponent<TileView>();
                    if (tileView == null)
                        tileView = tileObj.AddComponent<TileView>();

                    Vector2Int gridPos = new Vector2Int(x, y);
                    tileView.Initialize(gridPos);
                    tileViews[gridPos] = tileView;
                }
            }
        }

        public void ClearTileViews()
        {
            foreach (var tile in tileViews.Values)
            {
                if (tile != null)
                    Destroy(tile.gameObject);
            }
            tileViews.Clear();
        }

        public TileView GetTileView(Vector2Int position)
        {
            tileViews.TryGetValue(position, out var view);
            return view;
        }

        public void SetTileType(Vector2Int position, TileType type)
        {
            if (tileViews.TryGetValue(position, out var view))
            {
                view.SetTileType(type);
            }
        }

        public BlobView SpawnBlobView(Vector2Int gridPosition, BlobType type, BlobColor color)
        {
            Vector3 worldPos = GridToWorldPosition(gridPosition);
            GameObject prefab = GetPrefabForType(type);

            GameObject blobObj;
            if (prefab != null)
            {
                blobObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            }
            else
            {
                blobObj = CreateDefaultBlobView(worldPos);
            }

            blobObj.name = $"Blob_{type}_{color}_{gridPosition.x}_{gridPosition.y}";

            BlobView view = blobObj.GetComponent<BlobView>();
            if (view == null)
                view = blobObj.AddComponent<BlobView>();

            view.Initialize(type, color);
            view.PlaySpawnAnimation();

            return view;
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            float totalWidth = gridWidth * (tileSize + tileSpacing) - tileSpacing;
            float totalHeight = gridHeight * (tileSize + tileSpacing) - tileSpacing;
            Vector3 startPos = new Vector3(-totalWidth / 2 + tileSize / 2, -totalHeight / 2 + tileSize / 2, 0);

            return startPos + new Vector3(
                gridPosition.x * (tileSize + tileSpacing),
                gridPosition.y * (tileSize + tileSpacing),
                0
            );
        }

        private GameObject GetPrefabForType(BlobType type)
        {
            return type switch
            {
                BlobType.Normal => normalBlobPrefab,
                BlobType.Trail => trailBlobPrefab,
                BlobType.Ghost => ghostBlobPrefab,
                BlobType.Flag => flagBlobPrefab,
                BlobType.Rock => rockBlobPrefab,
                BlobType.Switch => switchBlobPrefab,
                _ => normalBlobPrefab
            };
        }

        private GameObject CreateDefaultTile(Vector3 position)
        {
            GameObject tileObj = new GameObject("Tile");
            tileObj.transform.position = position;
            tileObj.transform.parent = transform;

            SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.3f, 0.25f, 0.4f, 1f);
            tileObj.transform.localScale = Vector3.one * tileSize;

            return tileObj;
        }

        private GameObject CreateDefaultBlobView(Vector3 position)
        {
            GameObject blobObj = new GameObject("Blob");
            blobObj.transform.position = position;
            blobObj.transform.parent = transform;

            SpriteRenderer sr = blobObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.sortingOrder = 1;
            blobObj.transform.localScale = Vector3.one * (tileSize * 0.7f);

            CircleCollider2D collider = blobObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            return blobObj;
        }

        private Sprite CreateSquareSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float cornerRadius = 6f;
                    bool inCorner = false;

                    if (x < cornerRadius && y < cornerRadius)
                        inCorner = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius)) > cornerRadius;
                    else if (x >= 32 - cornerRadius && y < cornerRadius)
                        inCorner = Vector2.Distance(new Vector2(x, y), new Vector2(32 - cornerRadius, cornerRadius)) > cornerRadius;
                    else if (x < cornerRadius && y >= 32 - cornerRadius)
                        inCorner = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, 32 - cornerRadius)) > cornerRadius;
                    else if (x >= 32 - cornerRadius && y >= 32 - cornerRadius)
                        inCorner = Vector2.Distance(new Vector2(x, y), new Vector2(32 - cornerRadius, 32 - cornerRadius)) > cornerRadius;

                    colors[y * 32 + x] = inCorner ? Color.clear : Color.white;
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            Vector2 center = new Vector2(32, 32);

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * 64 + x] = dist <= 30 ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }
    }
}
