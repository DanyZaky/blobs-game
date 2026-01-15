using UnityEngine;
using UnityEditor;
using Blobs.Core;
using Blobs.Blobs;

namespace Blobs.Editor
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        private const float CELL_SIZE = 40f;
        private const float CELL_SPACING = 2f;

        private LevelData levelData;
        private BlobType selectedBlobType = BlobType.Normal;
        private BlobColor selectedBlobColor = BlobColor.Pink;
        private TileType selectedTileType = TileType.Normal;
        private bool isPlacingBlob = true;
        private Vector2 scrollPosition;

        // Foldout states
        private bool showLevelInfo = true;
        private bool showScoring = false;
        private bool showTutorial = false;
        private bool showLevelEditor = true;
        private bool showRawData = false;

        // Color palette for blob types
        private static readonly Color[] BlobTypeColors = new Color[]
        {
            new Color(0.5f, 0.8f, 1f),      // Normal - light blue
            new Color(0.8f, 0.6f, 1f),      // Trail - purple
            new Color(0.9f, 0.9f, 0.9f),    // Ghost - white
            new Color(0.4f, 0.9f, 0.4f),    // Flag - green
            new Color(0.5f, 0.5f, 0.5f),    // Rock - gray
            new Color(1f, 0.8f, 0.3f),      // Switch - gold
        };

        private static readonly Color[] BlobColorPalette = new Color[]
        {
            new Color(1f, 0.5f, 0.7f),      // Pink
            new Color(0.4f, 0.7f, 1f),      // Blue
            new Color(1f, 0.4f, 0.4f),      // Red
            new Color(0.4f, 0.9f, 0.9f),    // Cyan
            new Color(0.5f, 0.9f, 0.5f),    // Green
            new Color(1f, 0.9f, 0.4f),      // Yellow
            new Color(1f, 1f, 1f),          // White
            new Color(0.5f, 0.5f, 0.5f),    // Gray
        };

        private void OnEnable()
        {
            levelData = (LevelData)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with level name
            DrawHeader();

            EditorGUILayout.Space(5);

            // Level Info Section (Foldout)
            showLevelInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showLevelInfo, "📋 Level Info & Grid Size");
            if (showLevelInfo)
            {
                EditorGUI.indentLevel++;
                DrawLevelInfoSection();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(3);

            // Scoring Section (Foldout)
            showScoring = EditorGUILayout.BeginFoldoutHeaderGroup(showScoring, "⭐ Scoring Settings");
            if (showScoring)
            {
                EditorGUI.indentLevel++;
                DrawScoringSection();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(3);

            // Tutorial Section (Foldout)
            showTutorial = EditorGUILayout.BeginFoldoutHeaderGroup(showTutorial, "📖 Tutorial Settings");
            if (showTutorial)
            {
                EditorGUI.indentLevel++;
                DrawTutorialSection();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            // Visual Grid Editor (Foldout - default open)
            showLevelEditor = EditorGUILayout.BeginFoldoutHeaderGroup(showLevelEditor, "🎮 Level Editor");
            if (showLevelEditor)
            {
                DrawGridEditor();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Raw Data (Foldout - collapsed)
            showRawData = EditorGUILayout.BeginFoldoutHeaderGroup(showRawData, "📦 Raw Data (Advanced)");
            if (showRawData)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blobs"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tiles"), true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft
            };
            
            string levelName = string.IsNullOrEmpty(levelData.levelName) ? "Unnamed Level" : levelData.levelName;
            EditorGUILayout.LabelField($"Level {levelData.levelNumber}: {levelName}", headerStyle);
            
            // Stats
            GUIStyle statsStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
            EditorGUILayout.LabelField($"{levelData.width}x{levelData.height} | {levelData.blobs.Count} blobs", statsStyle, GUILayout.Width(120));
            
            EditorGUILayout.EndHorizontal();
            
            // Draw separator
            EditorGUILayout.Space(2);
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private void DrawLevelInfoSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelNumber"), GUILayout.Width(200));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelName"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Grid Size", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("width"), GUILayout.Width(150));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("height"), GUILayout.Width(150));
            
            if (GUILayout.Button("3x3", GUILayout.Width(40))) { SetGridSize(3, 3); }
            if (GUILayout.Button("5x5", GUILayout.Width(40))) { SetGridSize(5, 5); }
            if (GUILayout.Button("7x7", GUILayout.Width(40))) { SetGridSize(7, 7); }
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void SetGridSize(int w, int h)
        {
            Undo.RecordObject(levelData, "Set Grid Size");
            levelData.width = w;
            levelData.height = h;
            EditorUtility.SetDirty(levelData);
        }

        private void DrawScoringSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minMoves"), GUILayout.Width(180));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseScore"), GUILayout.Width(180));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movePenalty"), GUILayout.Width(180));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("starThresholds"));
            
            EditorGUILayout.EndVertical();
        }

        private void DrawTutorialSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isTutorial"));
            
            if (levelData.isTutorial)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tutorialMessages"), true);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawGridEditor()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Tool Selection Row
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Tool:", GUILayout.Width(35));
            
            GUI.backgroundColor = isPlacingBlob ? new Color(0.4f, 0.8f, 0.4f) : Color.white;
            if (GUILayout.Button("🔵 Blob", GUILayout.Width(70), GUILayout.Height(22)))
                isPlacingBlob = true;
            
            GUI.backgroundColor = !isPlacingBlob ? new Color(0.4f, 0.8f, 0.4f) : Color.white;
            if (GUILayout.Button("⬛ Tile", GUILayout.Width(70), GUILayout.Height(22)))
                isPlacingBlob = false;
            
            GUI.backgroundColor = Color.white;
            
            GUILayout.FlexibleSpace();
            
            // Quick actions on same row
            if (GUILayout.Button("Clear Blobs", GUILayout.Width(80)))
            {
                Undo.RecordObject(levelData, "Clear Blobs");
                levelData.blobs.Clear();
                EditorUtility.SetDirty(levelData);
            }
            if (GUILayout.Button("Clear Tiles", GUILayout.Width(80)))
            {
                Undo.RecordObject(levelData, "Clear Tiles");
                levelData.tiles.Clear();
                EditorUtility.SetDirty(levelData);
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Palette row
            if (isPlacingBlob)
            {
                DrawBlobPalette();
            }
            else
            {
                DrawTilePalette();
            }

            EditorGUILayout.Space(8);

            // Grid Display
            DrawGrid();

            // Legend
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("💡 Left-click: Place | Right-click: Remove | Letters: N=Normal T=Trail G=Ghost F=Flag R=Rock S=Switch", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawBlobPalette()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Type selection
            EditorGUILayout.LabelField("Type:", GUILayout.Width(35));
            string[] typeNames = { "Normal", "Trail", "Ghost", "Flag", "Rock", "Switch" };
            for (int i = 0; i < typeNames.Length; i++)
            {
                GUI.backgroundColor = BlobTypeColors[i];
                if (selectedBlobType == (BlobType)i)
                {
                    GUI.backgroundColor = Color.Lerp(BlobTypeColors[i], Color.white, 0.3f);
                }
                
                GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                if (selectedBlobType == (BlobType)i)
                    btnStyle.fontStyle = FontStyle.Bold;
                
                if (GUILayout.Button(typeNames[i].Substring(0, 1), btnStyle, GUILayout.Width(25), GUILayout.Height(20)))
                {
                    selectedBlobType = (BlobType)i;
                }
            }
            
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(15);
            
            // Color selection
            EditorGUILayout.LabelField("Color:", GUILayout.Width(40));
            string[] colorNames = { "Pk", "Bl", "Rd", "Cy", "Gr", "Yl", "Wh", "Gy" };
            for (int i = 0; i < colorNames.Length; i++)
            {
                GUI.backgroundColor = BlobColorPalette[i];
                if (selectedBlobColor == (BlobColor)i)
                {
                    GUI.backgroundColor = Color.Lerp(BlobColorPalette[i], Color.white, 0.3f);
                }
                
                if (GUILayout.Button("", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    selectedBlobColor = (BlobColor)i;
                }
            }
            
            GUI.backgroundColor = Color.white;
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"[{selectedBlobType} - {selectedBlobColor}]", EditorStyles.miniLabel, GUILayout.Width(100));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTilePalette()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Type:", GUILayout.Width(35));
            
            string[] tileNames = { "Normal", "Blocked", "Goal", "Ice", "Sticky" };
            Color[] tileColors = {
                new Color(0.25f, 0.25f, 0.3f),
                new Color(0.15f, 0.15f, 0.2f),
                new Color(0.3f, 0.4f, 0.3f),
                new Color(0.5f, 0.7f, 0.8f),
                new Color(0.5f, 0.35f, 0.25f)
            };
            
            for (int i = 0; i < tileNames.Length; i++)
            {
                bool isSelected = selectedTileType == (TileType)i;
                GUI.backgroundColor = isSelected ? Color.green : tileColors[i];
                
                if (GUILayout.Button(tileNames[i], GUILayout.Height(20)))
                {
                    selectedTileType = (TileType)i;
                }
            }
            
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            float gridWidth = levelData.width * (CELL_SIZE + CELL_SPACING);
            float gridHeight = levelData.height * (CELL_SIZE + CELL_SPACING);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, 
                GUILayout.Height(Mathf.Min(gridHeight + 20, 350)));

            Rect gridRect = GUILayoutUtility.GetRect(gridWidth, gridHeight);

            // Draw grid cells (Y is flipped - 0 is bottom)
            for (int y = levelData.height - 1; y >= 0; y--)
            {
                for (int x = 0; x < levelData.width; x++)
                {
                    float cellX = gridRect.x + x * (CELL_SIZE + CELL_SPACING);
                    float cellY = gridRect.y + (levelData.height - 1 - y) * (CELL_SIZE + CELL_SPACING);
                    Rect cellRect = new Rect(cellX, cellY, CELL_SIZE, CELL_SIZE);

                    Vector2Int pos = new Vector2Int(x, y);

                    // Get tile and blob at this position
                    TileSpawnData tileData = GetTileAt(pos);
                    BlobSpawnData blobData = GetBlobAt(pos);

                    // Draw tile background
                    Color tileColor = GetTileColor(tileData);
                    EditorGUI.DrawRect(cellRect, tileColor);

                    // Draw blob if exists
                    if (blobData != null)
                    {
                        DrawBlobInCell(cellRect, blobData);
                    }

                    // Draw border
                    DrawCellBorder(cellRect);

                    // Handle clicks
                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        HandleCellClick(pos, Event.current.button);
                        Event.current.Use();
                        Repaint();
                    }

                    // Draw coordinates (smaller, bottom-left)
                    GUIStyle coordStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 8,
                        normal = { textColor = new Color(1, 1, 1, 0.5f) }
                    };
                    GUI.Label(new Rect(cellX + 1, cellY + CELL_SIZE - 10, 20, 10), $"{x},{y}", coordStyle);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawBlobInCell(Rect cellRect, BlobSpawnData blob)
        {
            float padding = 5f;
            Rect blobRect = new Rect(
                cellRect.x + padding,
                cellRect.y + padding,
                cellRect.width - padding * 2,
                cellRect.height - padding * 2
            );

            // Get blob color
            Color blobColor = BlobColorPalette[(int)blob.color];
            
            // Modify for special types
            if (blob.type == BlobType.Ghost)
            {
                blobColor.a = 0.6f;
            }
            else if (blob.type == BlobType.Rock)
            {
                blobColor = new Color(0.4f, 0.35f, 0.3f);
            }

            // Draw blob circle-ish shape
            EditorGUI.DrawRect(blobRect, blobColor);

            // Draw type indicator
            string typeLabel = blob.type.ToString().Substring(0, 1);
            GUIStyle centerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                normal = { textColor = Color.white }
            };
            
            // Add shadow for readability
            Rect shadowRect = new Rect(blobRect.x + 1, blobRect.y + 1, blobRect.width, blobRect.height);
            GUIStyle shadowStyle = new GUIStyle(centerStyle) { normal = { textColor = Color.black } };
            GUI.Label(shadowRect, typeLabel, shadowStyle);
            GUI.Label(blobRect, typeLabel, centerStyle);
        }

        private void DrawCellBorder(Rect cellRect)
        {
            Handles.color = new Color(0.4f, 0.4f, 0.4f);
            Handles.DrawLine(new Vector3(cellRect.x, cellRect.y), new Vector3(cellRect.xMax, cellRect.y));
            Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.y), new Vector3(cellRect.xMax, cellRect.yMax));
            Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.yMax), new Vector3(cellRect.x, cellRect.yMax));
            Handles.DrawLine(new Vector3(cellRect.x, cellRect.yMax), new Vector3(cellRect.x, cellRect.y));
        }

        private Color GetTileColor(TileSpawnData tile)
        {
            if (tile == null)
                return new Color(0.25f, 0.25f, 0.3f); // Normal

            return tile.type switch
            {
                TileType.Normal => new Color(0.25f, 0.25f, 0.3f),
                TileType.Blocked => new Color(0.15f, 0.15f, 0.2f),
                TileType.Goal => new Color(0.3f, 0.4f, 0.3f),
                TileType.Ice => new Color(0.5f, 0.7f, 0.8f),
                TileType.Sticky => new Color(0.5f, 0.35f, 0.25f),
                _ => new Color(0.25f, 0.25f, 0.3f)
            };
        }

        private void HandleCellClick(Vector2Int pos, int mouseButton)
        {
            Undo.RecordObject(levelData, "Edit Level");

            if (mouseButton == 0) // Left click - place
            {
                if (isPlacingBlob)
                {
                    levelData.blobs.RemoveAll(b => b.position == pos);
                    levelData.blobs.Add(new BlobSpawnData
                    {
                        position = pos,
                        type = selectedBlobType,
                        color = selectedBlobColor,
                        size = 1
                    });
                }
                else
                {
                    levelData.tiles.RemoveAll(t => t.position == pos);
                    if (selectedTileType != TileType.Normal)
                    {
                        levelData.tiles.Add(new TileSpawnData
                        {
                            position = pos,
                            type = selectedTileType
                        });
                    }
                }
            }
            else if (mouseButton == 1) // Right click - remove
            {
                if (isPlacingBlob)
                {
                    levelData.blobs.RemoveAll(b => b.position == pos);
                }
                else
                {
                    levelData.tiles.RemoveAll(t => t.position == pos);
                }
            }

            EditorUtility.SetDirty(levelData);
        }

        private BlobSpawnData GetBlobAt(Vector2Int pos)
        {
            return levelData.blobs.Find(b => b.position == pos);
        }

        private TileSpawnData GetTileAt(Vector2Int pos)
        {
            return levelData.tiles.Find(t => t.position == pos);
        }
    }
}
