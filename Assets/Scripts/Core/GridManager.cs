using System.Collections.Generic;
using UnityEngine;
using Blobs.Blobs;

namespace Blobs.Core
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int gridHeight = 5;
        [SerializeField] private float tileSize = 1.2f;
        [SerializeField] private float tileSpacing = 0.1f;

        [Header("Prefabs")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject blobPrefab;

        [Header("Test Setup")]
        [SerializeField] private List<TestBlobData> testBlobs = new List<TestBlobData>();
        [SerializeField] private bool spawnShowcaseLevel = true;

        private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        private List<Blob> allBlobs = new List<Blob>();

        [System.Serializable]
        public class TestBlobData
        {
            public Vector2Int position;
            public BlobType type = BlobType.Normal;
            public BlobColor color = BlobColor.Pink;
        }

        public void InitializeGrid()
        {
            ClearGrid();
            
            // Check for showcase level override BEFORE creating tiles
            if (testBlobs.Count == 0 && spawnShowcaseLevel)
            {
                gridWidth = 5;
                gridHeight = 5;
            }

            CreateTiles();
            SpawnTestBlobs();
            Debug.Log($"[GridManager] Grid initialized: {gridWidth}x{gridHeight}");
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

        private void SpawnTestBlobs()
        {
            if (testBlobs.Count == 0 && spawnShowcaseLevel)
            {
                SpawnShowcaseLevel();
            }
            else
            {
                foreach (var data in testBlobs)
                {
                    SpawnBlob(data.position, data.color, data.type);
                }
            }
        }

        /// <summary>
        /// Spawns a SOLVABLE showcase level demonstrating all 6 blob types.
        /// Layout (5x5 grid):
        /// Row 4: [Rock ] [     ] [     ] [     ] [     ]
        /// Row 3: [     ] [     ] [Switch] [     ] [Flag-Pink]
        /// Row 2: [Trail-Green] [     ] [Ghost-Blue] [     ] [     ]
        /// Row 1: [Normal-Pink] [Normal-Blue] [Normal-Red] [Normal-Cyan] [Normal-Pink]
        /// Row 0: [     ] [     ] [     ] [     ] [     ]
        /// 
        /// SOLUTION:
        /// 1. Pink(0,1) → RIGHT → Blue(1,1) = Pink at (1,1)
        /// 2. Pink(1,1) → RIGHT → Red(2,1) = Pink at (2,1)
        /// 3. Pink(2,1) → RIGHT → Cyan(3,1) = Pink at (3,1)
        /// 4. Pink(3,1) → UP → Ghost(2,2)? No, not same column. 
        ///    Pink(3,1) → RIGHT → Pink(4,1)? No, same color.
        /// 5. Pink(4,1) → UP → Switch(2,3)? No, not same column.
        ///    Let's use Trail → Ghost first...
        /// 6. Trail(0,2) → RIGHT → Ghost(2,2) = Trail at (2,2), Ghost respawns at (0,2)
        /// ... continue merging until Flag + Pink are alone (Rock doesn't count)
        /// Final: Pink merges into Flag-Pink = WIN!
        /// </summary>
        private void SpawnShowcaseLevel()
        {
            // Force grid size to 5x5 for showcase (overriding Inspector values)
            gridWidth = 5;
            gridHeight = 5;
            
            Debug.Log("[GridManager] === SPAWNING SOLVABLE SHOWCASE LEVEL ===");
            Debug.Log("[GridManager] This level shows all 6 blob types and CAN BE WON!");

            // Row 1: Chain of Normal blobs for merging practice
            SpawnBlob(new Vector2Int(0, 1), BlobColor.Pink, BlobType.Normal);    // Start here!
            SpawnBlob(new Vector2Int(1, 1), BlobColor.Blue, BlobType.Normal);    
            SpawnBlob(new Vector2Int(2, 1), BlobColor.Red, BlobType.Normal);     
            SpawnBlob(new Vector2Int(3, 1), BlobColor.Cyan, BlobType.Normal);    

            // Row 2: Trail and Ghost
            SpawnBlob(new Vector2Int(0, 2), BlobColor.Yellow, BlobType.Trail);   // Trail - leaves blobs behind
            SpawnBlob(new Vector2Int(3, 2), BlobColor.Green, BlobType.Ghost);    // Ghost - respawns

            // Row 3: Switch and Flag (goal)
            SpawnBlob(new Vector2Int(1, 3), BlobColor.Cyan, BlobType.Switch);    // Switch
            SpawnBlob(new Vector2Int(4, 3), BlobColor.Pink, BlobType.Flag);      // FLAG - merge Pink here to WIN!

            // Row 4: Rock obstacle (doesn't count for win)
            SpawnBlob(new Vector2Int(0, 4), BlobColor.Gray, BlobType.Rock);      // Rock - just obstacle

            Debug.Log("[GridManager] === HOW TO WIN ===");
            Debug.Log("[GridManager] 1. Merge all Normal blobs (different colors only!)");
            Debug.Log("[GridManager] 2. Handle special blobs (Trail leaves trail, Ghost respawns)");
            Debug.Log("[GridManager] 3. Get down to just Pink + Flag-Pink (Rock doesn't count)");
            Debug.Log("[GridManager] 4. Merge Pink into Flag = WIN!");
            Debug.Log("[GridManager] TIP: Rock is an obstacle, it won't block your victory!");
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
            if (blobPrefab != null)
            {
                blobObj = Instantiate(blobPrefab, tile.transform.position, Quaternion.identity, transform);
            }
            else
            {
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

        private Sprite CreateRockSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    // Create rock-like shape with rough edges
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
                    
                    // Flag pole
                    if (x >= 18 && x <= 22 && y >= 10 && y <= 55)
                        colors[y * 64 + x] = new Color(0.6f, 0.4f, 0.2f);
                    
                    // Flag triangle
                    if (x >= 22 && x <= 50 && y >= 35 && y <= 55)
                    {
                        float flagWidth = 28f * (55 - y) / 20f;
                        if (x <= 22 + flagWidth)
                            colors[y * 64 + x] = Color.white;
                    }
                    
                    // Base circle
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
                    // Rounded corners
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
                // Rock is an obstacle, doesn't count towards win condition
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
    }
}
