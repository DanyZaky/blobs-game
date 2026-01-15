using UnityEngine;
using Blobs.Core;
using Blobs.Commands;
using Blobs.Input;

namespace Blobs
{
    /// <summary>
    /// Automatically sets up the Gameplay scene with all required managers and wires references.
    /// Just attach this to any GameObject and press Play - everything sets up automatically!
    /// </summary>
    [DefaultExecutionOrder(-100)] // Run before other scripts
    public class GameplaySceneSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnAwake = true;
        [SerializeField] private bool createUICanvas = true;

        [Header("Grid Settings (Optional Override)")]
        [SerializeField] private int gridWidth = 3;
        [SerializeField] private int gridHeight = 3;

        // Created managers
        private GameManager gameManager;
        private GridManager gridManager;
        private CommandManager commandManager;
        private InputManager inputManager;

        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                SetupScene();
            }
        }

        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("[GameplaySceneSetup] === STARTING AUTO SETUP ===");

            // 1. Setup Camera
            SetupCamera();

            // 2. Create all managers
            CreateManagers();

            // 3. Wire all references
            WireReferences();

            // 4. Create UI if needed
            if (createUICanvas)
            {
                CreateBasicUI();
            }

            Debug.Log("[GameplaySceneSetup] === AUTO SETUP COMPLETE ===");
            Debug.Log("[GameplaySceneSetup] Controls: Click blob to select, Arrow keys/WASD to merge, Z to undo");
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.36f, 0.31f, 0.48f); // Purple background
            cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
            
            Debug.Log("[GameplaySceneSetup] Camera configured");
        }

        private void CreateManagers()
        {
            // GameManager
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                GameObject obj = new GameObject("GameManager");
                gameManager = obj.AddComponent<GameManager>();
            }

            // GridManager
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                GameObject obj = new GameObject("GridManager");
                gridManager = obj.AddComponent<GridManager>();
            }

            // CommandManager
            commandManager = FindObjectOfType<CommandManager>();
            if (commandManager == null)
            {
                GameObject obj = new GameObject("CommandManager");
                commandManager = obj.AddComponent<CommandManager>();
            }

            // InputManager
            inputManager = FindObjectOfType<InputManager>();
            if (inputManager == null)
            {
                GameObject obj = new GameObject("InputManager");
                inputManager = obj.AddComponent<InputManager>();
            }

            // UIManager (for feedback text)
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                GameObject obj = new GameObject("UIManager");
                obj.AddComponent<UIManager>();
            }

            // BlobTooltipManager (for hover tooltips)
            BlobTooltipManager tooltipManager = FindObjectOfType<BlobTooltipManager>();
            if (tooltipManager == null)
            {
                GameObject obj = new GameObject("BlobTooltipManager");
                obj.AddComponent<BlobTooltipManager>();
            }

            Debug.Log("[GameplaySceneSetup] All managers created");
        }

        private void WireReferences()
        {
            // Wire GameManager -> GridManager
            var gmGridField = typeof(GameManager).GetField("gridManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gmGridField != null)
            {
                gmGridField.SetValue(gameManager, gridManager);
            }

            // Wire InputManager -> GridManager, CommandManager
            var imGridField = typeof(InputManager).GetField("gridManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var imCmdField = typeof(InputManager).GetField("commandManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (imGridField != null)
                imGridField.SetValue(inputManager, gridManager);
            if (imCmdField != null)
                imCmdField.SetValue(inputManager, commandManager);

            Debug.Log("[GameplaySceneSetup] All references wired");
        }

        private void CreateBasicUI()
        {
            // Check if canvas already exists
            Canvas existingCanvas = FindObjectOfType<Canvas>();
            if (existingCanvas != null) return;

            // Create Canvas
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create instruction text
            GameObject textObj = new GameObject("Instructions");
            textObj.transform.SetParent(canvasObj.transform, false);
            
            UnityEngine.UI.Text instructionText = textObj.AddComponent<UnityEngine.UI.Text>();
            instructionText.text = "Click blob → Arrow keys to merge → Z to undo";
            instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            instructionText.fontSize = 24;
            instructionText.color = Color.white;
            instructionText.alignment = TextAnchor.UpperCenter;

            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(0, 40);

            // Create Undo button
            CreateButton(canvasObj.transform, "Undo Button", "UNDO (Z)", 
                new Vector2(100, 50), new Vector2(-100, 50), OnUndoClicked);

            Debug.Log("[GameplaySceneSetup] Basic UI created");
        }

        private void CreateButton(Transform parent, string name, string text, Vector2 size, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            UnityEngine.UI.Image btnImage = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);

            UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(onClick);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            UnityEngine.UI.Text btnText = textObj.AddComponent<UnityEngine.UI.Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 18;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
        }

        private void OnUndoClicked()
        {
            commandManager?.Undo();
        }

        [ContextMenu("Clear Scene")]
        public void ClearScene()
        {
            // Remove all created objects (useful for reset)
            if (gameManager != null) DestroyImmediate(gameManager.gameObject);
            if (gridManager != null) DestroyImmediate(gridManager.gameObject);
            if (commandManager != null) DestroyImmediate(commandManager.gameObject);
            if (inputManager != null) DestroyImmediate(inputManager.gameObject);
            
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) DestroyImmediate(canvas.gameObject);

            Debug.Log("[GameplaySceneSetup] Scene cleared");
        }
    }
}
