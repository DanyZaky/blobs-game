using UnityEngine;
using Blobs.Core;
using Blobs.Interfaces;
using Blobs.Models;
using Blobs.Services;

namespace Blobs.Presenters
{
    /// <summary>
    /// Presenter for game state logic.
    /// Manages game state, scoring, and win conditions.
    /// Subscribes to MoveService events for game progression (SRP compliance).
    /// </summary>
    public class GamePresenter : MonoBehaviour, IGamePresenter
    {
        [Header("References")]
        [SerializeField] private GridPresenter gridPresenter;
        [SerializeField] private LevelData startingLevel;

        private GameStateModel _model;

        // Service reference for event subscription
        private IMoveService MoveService => ServiceLocator.Move;

        public GameState CurrentState => _model?.CurrentState ?? GameState.Playing;
        public int MoveCount => _model?.MoveCount ?? 0;
        public int Score => _model?.Score ?? 0;

        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<int> OnMoveCountChanged;
        public event System.Action<int> OnScoreChanged;

        private void Awake()
        {
            _model = new GameStateModel();

            // Subscribe to model events
            _model.OnStateChanged += state => OnGameStateChanged?.Invoke(state);
            _model.OnMoveCountChanged += count => OnMoveCountChanged?.Invoke(count);
            _model.OnScoreChanged += score => OnScoreChanged?.Invoke(score);

            // Register with ServiceLocator
            ServiceLocator.RegisterGame(this);
        }

        private void Start()
        {
            InitializeGame();
            SubscribeToMoveService();
        }

        /// <summary>
        /// Subscribe to MoveService events for game progression.
        /// This decouples game logic from input handling (SRP).
        /// </summary>
        private void SubscribeToMoveService()
        {
            // Wait for services to be registered
            StartCoroutine(WaitAndSubscribeToMoveService());
        }

        private System.Collections.IEnumerator WaitAndSubscribeToMoveService()
        {
            // Wait until MoveService is available
            while (MoveService == null)
            {
                yield return null;
            }

            MoveService.OnMergeExecuted += HandleMergeExecuted;
            Debug.Log("[GamePresenter] Subscribed to MoveService events");
        }

        private void HandleMergeExecuted(IBlobPresenter source, IBlobPresenter target)
        {
            // Increment move count
            IncrementMoveCount();

            // Check win condition
            CheckWinCondition();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (MoveService != null)
            {
                MoveService.OnMergeExecuted -= HandleMergeExecuted;
            }
        }

        public void InitializeGame()
        {
            _model.Reset();

            if (gridPresenter == null)
                gridPresenter = FindObjectOfType<GridPresenter>();

            if (startingLevel != null && gridPresenter != null)
            {
                gridPresenter.LoadLevel(startingLevel);
                _model.SetLevelNumber(startingLevel.levelNumber);
            }

            Debug.Log("[GamePresenter] Game initialized");
        }

        public void StartGame()
        {
            SetGameState(GameState.Playing);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            InitializeGame();
        }

        public void SetGameState(GameState state)
        {
            _model.SetState(state);
            Debug.Log($"[GamePresenter] State changed to: {state}");
        }

        public void CheckWinCondition()
        {
            if (gridPresenter == null) return;

            int playableCount = gridPresenter.GetPlayableBlobCount();

            if (playableCount <= 0)
            {
                SetGameState(GameState.Win);
                CalculateFinalScore();
                Debug.Log("[GamePresenter] 🎉 YOU WIN!");
            }
            else if (playableCount == 1)
            {
                Debug.Log($"[GamePresenter] Almost there! {playableCount} blob remaining.");
            }
        }

        public void IncrementMoveCount()
        {
            _model.IncrementMoveCount();
        }

        public void AddScore(int points)
        {
            _model.AddScore(points);
        }

        private void CalculateFinalScore()
        {
            if (startingLevel == null) return;

            int baseScore = startingLevel.baseScore;
            int movePenalty = startingLevel.movePenalty * MoveCount;
            int finalScore = Mathf.Max(0, baseScore - movePenalty);

            _model.AddScore(finalScore);
            Debug.Log($"[GamePresenter] Final Score: {finalScore} (Base: {baseScore}, Penalty: {movePenalty})");
        }

        public void LoadLevel(LevelData level)
        {
            startingLevel = level;
            RestartGame();
        }
    }
}
