using UnityEngine;

namespace Blobs.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        Win,
        Lose
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GridManager gridManager;

        public GameState CurrentState { get; private set; } = GameState.Playing;

        public event System.Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            SetGameState(GameState.Playing);
            gridManager?.InitializeGrid();
            Debug.Log("[GameManager] Game initialized");
        }

        public void SetGameState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] State changed to: {newState}");
        }

        public void CheckWinCondition()
        {
            if (gridManager == null) return;

            // Use playable blob count (excludes obstacles like Rock)
            int playableCount = gridManager.GetPlayableBlobCount();
            
            if (playableCount <= 0)
            {
                SetGameState(GameState.Win);
                Debug.Log("[GameManager] 🎉 YOU WIN! All playable blobs cleared!");
            }
            else if (playableCount == 1)
            {
                Debug.Log($"[GameManager] Almost there! {playableCount} blob remaining.");
            }
        }

        public void RestartGame()
        {
            InitializeGame();
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
    }
}
