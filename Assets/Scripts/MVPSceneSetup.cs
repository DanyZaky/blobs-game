using UnityEngine;
using Blobs.Core;
using Blobs.Commands;
using Blobs.Input;
using Blobs.Presenters;
using Blobs.Services;
using Blobs.Views;

namespace Blobs
{
    /// <summary>
    /// MVP Scene Setup - Clean architecture bootstrapper.
    /// Creates all MVP components and SRP-compliant services.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class MVPSceneSetup : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelData startingLevel;

        [Header("Prefabs (Optional)")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject normalBlobPrefab;
        [SerializeField] private GameObject trailBlobPrefab;
        [SerializeField] private GameObject ghostBlobPrefab;
        [SerializeField] private GameObject flagBlobPrefab;
        [SerializeField] private GameObject rockBlobPrefab;
        [SerializeField] private GameObject switchBlobPrefab;

        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnAwake = true;
        [SerializeField] private bool createUI = true;

        // MVP Components
        private GamePresenter gamePresenter;
        private GridPresenter gridPresenter;
        private GridView gridView;
        private InputPresenter inputPresenter;
        private CommandManager commandManager;

        // SRP Services
        private InputService inputService;
        private SelectionService selectionService;
        private MoveService moveService;
        private FeedbackService feedbackService;

        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                SetupScene();
            }
        }

        [ContextMenu("Setup MVP Scene")]
        public void SetupScene()
        {
            Debug.Log("[MVPSceneSetup] === STARTING CLEAN MVP SETUP ===");

            SetupCamera();
            CreateMVPComponents();
            CreateServices();
            CreateInputAndCommands();

            if (createUI)
            {
                CreateUI();
            }

            WireReferences();

            Debug.Log("[MVPSceneSetup] === MVP SETUP COMPLETE ===");
            Debug.Log("[MVPSceneSetup] Architecture: Model-View-Presenter + SOLID + SRP Services");
            Debug.Log("[MVPSceneSetup] Controls: Click blob to select, Arrow keys/WASD to move, Z=Undo, Y=Redo");
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
            cam.backgroundColor = new Color(0.36f, 0.31f, 0.48f);
            cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private void CreateMVPComponents()
        {
            // GridView + GridPresenter
            gridPresenter = FindObjectOfType<GridPresenter>();
            if (gridPresenter == null)
            {
                GameObject obj = new GameObject("GridPresenter");
                gridView = obj.AddComponent<GridView>();
                gridPresenter = obj.AddComponent<GridPresenter>();

                // Wire prefabs to GridView
                WireGridViewPrefabs(gridView);
            }
            else
            {
                gridView = gridPresenter.GetComponent<GridView>();
            }

            // GamePresenter
            gamePresenter = FindObjectOfType<GamePresenter>();
            if (gamePresenter == null)
            {
                GameObject obj = new GameObject("GamePresenter");
                gamePresenter = obj.AddComponent<GamePresenter>();
            }

            Debug.Log("[MVPSceneSetup] MVP Components created: GamePresenter, GridPresenter, GridView");
        }

        private void CreateServices()
        {
            // Create SRP-compliant services
            GameObject servicesObj = new GameObject("Services");

            // InputService - handles input polling only
            inputService = FindObjectOfType<InputService>();
            if (inputService == null)
            {
                inputService = servicesObj.AddComponent<InputService>();
            }
            ServiceLocator.RegisterInput(inputService);

            // SelectionService - handles selection state and hit testing
            selectionService = FindObjectOfType<SelectionService>();
            if (selectionService == null)
            {
                selectionService = servicesObj.AddComponent<SelectionService>();
            }
            ServiceLocator.RegisterSelection(selectionService);

            // MoveService - handles move finding and validation
            moveService = FindObjectOfType<MoveService>();
            if (moveService == null)
            {
                moveService = servicesObj.AddComponent<MoveService>();
            }
            ServiceLocator.RegisterMove(moveService);

            // FeedbackService - handles UI feedback
            feedbackService = FindObjectOfType<FeedbackService>();
            if (feedbackService == null)
            {
                feedbackService = servicesObj.AddComponent<FeedbackService>();
            }
            ServiceLocator.RegisterFeedback(feedbackService);

            Debug.Log("[MVPSceneSetup] SRP Services created: InputService, SelectionService, MoveService, FeedbackService");
        }

        private void WireGridViewPrefabs(GridView view)
        {
            // Use reflection to set prefab references
            var viewType = typeof(GridView);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            SetField(viewType, view, "tilePrefab", tilePrefab, flags);
            SetField(viewType, view, "normalBlobPrefab", normalBlobPrefab, flags);
            SetField(viewType, view, "trailBlobPrefab", trailBlobPrefab, flags);
            SetField(viewType, view, "ghostBlobPrefab", ghostBlobPrefab, flags);
            SetField(viewType, view, "flagBlobPrefab", flagBlobPrefab, flags);
            SetField(viewType, view, "rockBlobPrefab", rockBlobPrefab, flags);
            SetField(viewType, view, "switchBlobPrefab", switchBlobPrefab, flags);
        }

        private void CreateInputAndCommands()
        {
            // CommandManager
            commandManager = FindObjectOfType<CommandManager>();
            if (commandManager == null)
            {
                GameObject obj = new GameObject("CommandManager");
                commandManager = obj.AddComponent<CommandManager>();
            }

            // InputPresenter (MVP version)
            inputPresenter = FindObjectOfType<InputPresenter>();
            if (inputPresenter == null)
            {
                GameObject obj = new GameObject("InputPresenter");
                inputPresenter = obj.AddComponent<InputPresenter>();
            }

            Debug.Log("[MVPSceneSetup] Input and Command components created");
        }

        private void CreateUI()
        {
            // UIManager
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                GameObject obj = new GameObject("UIManager");
                obj.AddComponent<UIManager>();
            }

            // BlobTooltipManager
            BlobTooltipManager tooltipManager = FindObjectOfType<BlobTooltipManager>();
            if (tooltipManager == null)
            {
                GameObject obj = new GameObject("BlobTooltipManager");
                obj.AddComponent<BlobTooltipManager>();
            }

            Debug.Log("[MVPSceneSetup] UI components created");
        }

        private void WireReferences()
        {
            // Wire starting level to GamePresenter
            if (startingLevel != null && gamePresenter != null)
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                SetField(typeof(GamePresenter), gamePresenter, "startingLevel", startingLevel, flags);
                SetField(typeof(GamePresenter), gamePresenter, "gridPresenter", gridPresenter, flags);
            }

            // Wire CommandManager to InputPresenter
            if (inputPresenter != null && commandManager != null)
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                SetField(typeof(InputPresenter), inputPresenter, "commandManager", commandManager, flags);
            }

            Debug.Log("[MVPSceneSetup] References wired");
        }

        private void SetField(System.Type type, object target, string fieldName, object value, System.Reflection.BindingFlags flags)
        {
            var field = type.GetField(fieldName, flags);
            if (field != null && value != null)
            {
                field.SetValue(target, value);
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Clear();
        }
    }
}
