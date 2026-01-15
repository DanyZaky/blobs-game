using System.Collections.Generic;
using UnityEngine;
using Blobs.Blobs;

namespace Blobs.Core
{
    public class GridManager : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private LevelData currentLevel;

        [Header("Grid Settings (Fallback if no LevelData)")]
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int gridHeight = 5;
        [SerializeField] private float tileSize = 1.2f;
        [SerializeField] private float tileSpacing = 0.1f;

        [Header("Prefabs")]
        [SerializeField] private GameObject tilePrefab;
        
        [Header("Blob Prefabs")]
        [SerializeField] private GameObject normalBlobPrefab;
        [SerializeField] private GameObject trailBlobPrefab;
        [SerializeField] private GameObject ghostBlobPrefab;
        [SerializeField] private GameObject flagBlobPrefab;
        [SerializeField] private GameObject rockBlobPrefab;
        [SerializeField] private GameObject switchBlobPrefab;

        private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        private List<Blob> allBlobs = new List<Blob>();

        public LevelData CurrentLevel => currentLevel;

        public void InitializeGrid()
        {
            ClearGrid();
            
            if (currentLevel != null)
            {
                LoadFromLevelData(currentLevel);
            }
            else
            {
                Debug.LogWarning("[GridManager] No LevelData assigned! Creating empty grid.");
                CreateTiles();
            }
        }

        /// <summary>
        /// Load level from LevelData ScriptableObject
        /// </summary>
        public void LoadFromLevelData(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[GridManager] LevelData is null!");
                return;
            }

            currentLevel = levelData;
            gridWidth = levelData.width;
            gridHeight = levelData.height;

            Debug.Log($"[GridManager] Loading level: {levelData.levelName} ({gridWidth}x{gridHeight})");

            // Create tiles
            CreateTiles();

            // Apply special tile types if defined
            foreach (var tileData in levelData.tiles)
            {
                if (tiles.TryGetValue(tileData.position, out Tile tile))
                {
                    tile.SetTileType(tileData.type);
                }
            }

            // Spawn blobs
            foreach (var blobData in levelData.blobs)
            {
                SpawnBlob(blobData.position, blobData.color, blobData.type);
            }

            Debug.Log($"[GridManager] Level loaded: {allBlobs.Count} blobs spawned");
        }

        /// <summary>
        /// Load level by assigning new LevelData
        /// </summary>
        public void LoadLevel(LevelData levelData)
        {
            ClearGrid();
            LoadFromLevelData(levelData);
        }

        private void ClearGrid()
        {
            foreach (var tile in tiles.Values)
            {
                if (tile != null)
                    Destroy(tile.gameObject);
            }
            tiles.Clear();

            foreach (var blob in allBlobs)
            {
                if (blob != null)
                    Destroy(blob.gameObject);
            }
            allBlobs.Clear();
        }

        private void CreateTiles()
        {
            float totalWidth = gridWidth * (tileSize + tileSpacing) - tileSpacing;
            float totalHeight = gridHeight * (tileSize + tileSpacing) - tileSpacing;
            Vector3 startPos = new Vector3(-totalWidth / 2 + tileSize / 2, -totalHeight / 2 + tileSize / 2, 0);

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
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

                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile == null)
                        tile = tileObj.AddComponent<Tile>();

                    Vector2Int gridPos = new Vector2Int(x, y);
                    tile.Initialize(gridPos);
                    tiles[gridPos] = tile;
                }
            }
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

        public Blob SpawnBlob(Vector2Int gridPos, BlobColor color, BlobType type = BlobType.Normal)
        {
            if (!tiles.TryGetValue(gridPos, out Tile tile))
            {
                Debug.LogWarning($"[GridManager] No tile at position {gridPos}");
                return null;
            }

            if (tile.IsOccupied)
            {
                Debug.LogWarning($"[GridManager] Tile at {gridPos} is already occupied");
                return null;
            }

            GameObject blobObj;
            GameObject prefab = GetPrefabForType(type);
            
            if (prefab != null)
            {
                blobObj = Instantiate(prefab, tile.transform.position, Quaternion.identity, transform);
                blobObj.name = $"{type}Blob_{gridPos.x}_{gridPos.y}";
            }
            else
            {
                // Fallback to programmatic creation if no prefab assigned
                blobObj = CreateDefaultBlob(tile.transform.position, type);
            }

            Blob blob = blobObj.GetComponent<Blob>();
            if (blob == null)
                blob = blobObj.AddComponent<Blob>();

            blob.Initialize(type, color, tile);
            tile.SetBlob(blob);
            allBlobs.Add(blob);

            return blob;
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

        private GameObject CreateDefaultBlob(Vector3 position, BlobType type = BlobType.Normal)
        {
            GameObject blobObj = new GameObject("Blob");
            blobObj.transform.position = position;
            blobObj.transform.parent = transform;

            SpriteRenderer sr = blobObj.AddComponent<SpriteRenderer>();
            
            // Use different sprites based on type
            switch (type)
            {
                case BlobType.Rock:
                    sr.sprite = CreateRockSprite();
                    break;
                case BlobType.Flag:
                    sr.sprite = CreateFlagSprite();
                    break;
                default:
                    sr.sprite = CreateCircleSprite();
                    break;
            }
            
            sr.sortingOrder = 1;
            blobObj.transform.localScale = Vector3.one * (tileSize * 0.7f);

            // Add collider for click detection
            CircleCollider2D collider = blobObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            return blobObj;
        }

        #region Sprite Generation

        private Sprite CreateRockSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float nx = (x - 32f) / 28f;
                    float ny = (y - 32f) / 28f;
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);
                    float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.3f;
                    
                    if (dist + noise < 1.0f)
                        colors[y * 64 + x] = Color.white;
                    else
                        colors[y * 64 + x] = Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        private Sprite CreateFlagSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    colors[y * 64 + x] = Color.clear;
                    
                    if (x >= 18 && x <= 22 && y >= 10 && y <= 55)
                        colors[y * 64 + x] = new Color(0.6f, 0.4f, 0.2f);
                    
                    if (x >= 22 && x <= 50 && y >= 35 && y <= 55)
                    {
                        float flagWidth = 28f * (55 - y) / 20f;
                        if (x <= 22 + flagWidth)
                            colors[y * 64 + x] = Color.white;
                    }
                    
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(20, 10));
                    if (dist <= 8)
                        colors[y * 64 + x] = new Color(0.4f, 0.3f, 0.2f);
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
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

        #endregion

        #region Public API

        public Tile GetTile(Vector2Int position)
        {
            tiles.TryGetValue(position, out Tile tile);
            return tile;
        }

        public Tile GetTileInDirection(Vector2Int from, Vector2Int direction)
        {
            Vector2Int targetPos = from + direction;
            return GetTile(targetPos);
        }

        public Blob GetBlobAt(Vector2Int position)
        {
            Tile tile = GetTile(position);
            return tile?.CurrentBlob;
        }

        public int GetBlobCount()
        {
            allBlobs.RemoveAll(b => b == null);
            return allBlobs.Count;
        }

        /// <summary>
        /// Returns count of playable blobs (excludes obstacles like Rock).
        /// Used for win condition checking.
        /// </summary>
        public int GetPlayableBlobCount()
        {
            allBlobs.RemoveAll(b => b == null);
            int count = 0;
            foreach (var blob in allBlobs)
            {
                if (blob.Type != BlobType.Rock)
                {
                    count++;
                }
            }
            return count;
        }

        public void RemoveBlob(Blob blob)
        {
            allBlobs.Remove(blob);
            if (blob.CurrentTile != null)
            {
                blob.CurrentTile.ClearBlob();
            }
            Destroy(blob.gameObject);
        }

        public List<Blob> GetAllBlobs()
        {
            allBlobs.RemoveAll(b => b == null);
            return new List<Blob>(allBlobs);
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return tiles.ContainsKey(position);
        }

        #endregion
    }
}
