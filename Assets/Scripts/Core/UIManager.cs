using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Blobs.Commands;
using Blobs.Services;

namespace Blobs.Core
{
    /// <summary>
    /// Simple UI Manager for gameplay feedback.
    /// Shows animated text feedback for invalid actions.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Feedback Text")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI gameplayScoreText;
        
        [Header("Animation Settings")]
        [SerializeField] private float feedbackDuration = 1.5f;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float moveUpAmount = 30f;
        [SerializeField] private Ease fadeInEase = Ease.OutBack;
        [SerializeField] private Ease fadeOutEase = Ease.InQuad;

        [Header("Input UI")]
        [SerializeField] private UnityEngine.UI.Button undoButton;

        [Header("Win Panel")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private UnityEngine.UI.Image[] winStarImages;
        [SerializeField] private UnityEngine.Sprite starFilledSprite;
        [SerializeField] private UnityEngine.Sprite starEmptySprite;
        [SerializeField] private TextMeshProUGUI winScoreText;
        [SerializeField] private UnityEngine.UI.Button nextLevelButton;
        [SerializeField] private UnityEngine.UI.Button retryButton;
        [SerializeField] private UnityEngine.UI.Button menuButton;

        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button retryPauseButton;
        [SerializeField] private Button menuPauseButton;

        private Sequence currentFeedbackSequence;
        private Vector3 feedbackOriginalPosition;

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Store original position
            if (feedbackText != null)
            {
                feedbackOriginalPosition = feedbackText.rectTransform.anchoredPosition;
                feedbackText.alpha = 0f;
            }
        }

        private void Start()
        {
            SetupButtonListeners();
            UpdateUndoButtonState();
        }

        private void Update()
        {
            // Keep undo button state in sync
            UpdateUndoButtonState();
        }

        private void UpdateUndoButtonState()
        {
            if (undoButton != null)
            {
                bool canUndo = CommandManager.Instance != null && CommandManager.Instance.CanUndo;
                undoButton.interactable = canUndo;
            }
        }

        private void SetupButtonListeners()
        {
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
            if (undoButton != null) undoButton.onClick.AddListener(OnUndoClicked);

            // Pause buttons
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (retryPauseButton != null) retryPauseButton.onClick.AddListener(OnRetryClicked);
            if (menuPauseButton != null) menuPauseButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            
            // Ensure time scale is reset just in case
            Time.timeScale = 1f;

            currentFeedbackSequence?.Kill();
        }

        #region Button Handlers

        private void OnPauseClicked()
        {
            AudioManager.Instance?.PlaySFX("ui button");
            ShowPausePanel();
        }

        private void OnResumeClicked()
        {
            AudioManager.Instance?.PlaySFX("ui button");
            HidePausePanel();
        }

        private void OnNextLevelClicked()
        {
            Time.timeScale = 1f; // Ensure time scale is normal
            // Get current level index from PlayerPrefs
            int currentIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
            int nextIndex = currentIndex + 1;

            // Check if there's a next level
            if (nextIndex >= MainMenuController.TotalLevelCount)
            {
                Debug.Log("[UIManager] No more levels! Returning to menu.");
                SceneManager.LoadScene("Menu");
                return;
            }

            // Set next level data
            LevelData nextLevel = MainMenuController.SetSelectedLevel(nextIndex);
            if (nextLevel != null)
            {
                Debug.Log($"[UIManager] Loading next level: {nextLevel.levelName}");
                SceneManager.LoadScene("MVPGameplay");
            }
            else
            {
                Debug.LogWarning("[UIManager] Failed to set next level, returning to menu.");
                SceneManager.LoadScene("Menu");
            }
        }

        private void OnRetryClicked()
        {
            AudioManager.Instance?.PlaySFX("ui button");
            Debug.Log("[UIManager] Retrying level");
            Time.timeScale = 1f; // Reset time scale
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnMenuClicked()
        {
            AudioManager.Instance?.PlaySFX("ui button");
            Debug.Log("[UIManager] Returning to menu");
            Time.timeScale = 1f; // Reset time scale
            SceneManager.LoadScene("Menu");
        }

        private void OnUndoClicked()
        {
            Debug.Log("[UIManager] Undo button clicked!");
            
            if (CommandManager.Instance == null)
            {
                Debug.LogWarning("[UIManager] CommandManager.Instance is null!");
                return;
            }

            if (!CommandManager.Instance.CanUndo)
            {
                Debug.Log("[UIManager] Nothing to undo (CanUndo = false)");
                return;
            }

            // Play undo SFX
            AudioManager.Instance?.PlaySFX("undo");

            CommandManager.Instance.Undo();
            Debug.Log("[UIManager] Undo executed successfully");
        }

        #endregion

        #region Pause Panel

        private void ShowPausePanel()
        {
            if (pausePanel == null) return;

            pausePanel.SetActive(true);
            Time.timeScale = 0f;

            // Animate
            if (pausePanel.GetComponent<CanvasGroup>() == null)
                pausePanel.AddComponent<CanvasGroup>();
            
            CanvasGroup cg = pausePanel.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f).SetUpdate(true); // SetUpdate(true) ignores timeScale

            RectTransform rt = pausePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one * 0.9f;
                rt.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        private void HidePausePanel()
        {
            if (pausePanel == null) return;

            Time.timeScale = 1f;

            // Animate
            CanvasGroup cg = pausePanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
                {
                    pausePanel.SetActive(false);
                });
            }
            else
            {
                pausePanel.SetActive(false);
            }
        }

        #endregion

        #region Gameplay Score

        /// <summary>
        /// Update the gameplay score display with animation.
        /// </summary>
        public void UpdateScore(int newScore)
        {
            if (gameplayScoreText == null) return;

            // Simple punch animation
            gameplayScoreText.text = $"Score: {newScore}";
            
            // Kill existing tween on the transform to avoid conflicts
            gameplayScoreText.transform.DOKill(true);
            
            // Punch scale effect
            gameplayScoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f);
        }

        #endregion

        /// <summary>
        /// Show animated feedback text
        /// </summary>
        public void ShowFeedback(string message)
        {
            if (feedbackText == null)
            {
                Debug.LogWarning("[UIManager] Feedback text not assigned!");
                return;
            }

            // Kill any existing animation
            currentFeedbackSequence?.Kill();

            // Reset position and set text
            feedbackText.rectTransform.anchoredPosition = feedbackOriginalPosition;
            feedbackText.text = message;
            feedbackText.alpha = 0f;

            // Create animation sequence
            currentFeedbackSequence = DOTween.Sequence();

            // Fade in + scale pop
            currentFeedbackSequence.Append(
                feedbackText.DOFade(1f, fadeInDuration)
                    .SetEase(fadeInEase)
            );
            currentFeedbackSequence.Join(
                feedbackText.rectTransform.DOScale(1.1f, fadeInDuration * 0.5f)
                    .SetEase(Ease.OutBack)
            );
            currentFeedbackSequence.Append(
                feedbackText.rectTransform.DOScale(1f, fadeInDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
            );

            // Hold for duration
            currentFeedbackSequence.AppendInterval(feedbackDuration);

            // Fade out + move up
            currentFeedbackSequence.Append(
                feedbackText.DOFade(0f, fadeOutDuration)
                    .SetEase(fadeOutEase)
            );
            currentFeedbackSequence.Join(
                feedbackText.rectTransform.DOAnchorPosY(
                    feedbackOriginalPosition.y + moveUpAmount, 
                    fadeOutDuration
                ).SetEase(fadeOutEase)
            );

            // Reset position after complete
            currentFeedbackSequence.OnComplete(() =>
            {
                feedbackText.rectTransform.anchoredPosition = feedbackOriginalPosition;
            });
        }

        #region Predefined Messages

        public void ShowCannotSelectFeedback()
        {
            ShowFeedback("Can't select this blob!");
        }

        public void ShowSameColorFeedback()
        {
            ShowFeedback("Can't merge same colors!");
        }

        public void ShowCannotMergeFeedback()
        {
            ShowFeedback("Can't merge with that!");
        }

        public void ShowNoMoveFeedback()
        {
            ShowFeedback("No blob there!");
        }

        public void ShowBlockedFeedback()
        {
            ShowFeedback("Path is blocked!");
        }

        #endregion

        #region Win Panel

        /// <summary>
        /// Show win panel with star animation.
        /// </summary>
        public void ShowWinPanel(int stars, int score)
        {
            if (winPanel == null)
            {
                Debug.LogWarning("[UIManager] Win panel not assigned!");
                return;
            }

            // Play win SFX
            AudioManager.Instance?.PlaySFX("win");
            AudioManager.Instance?.PlaySFX("win2");

            // Update score text
            if (winScoreText != null)
            {
                winScoreText.text = $"Score: {score}";
            }

            // Update star display
            UpdateWinStars(stars);

            // Show panel with animation
            winPanel.SetActive(true);
            var panelRect = winPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.localScale = Vector3.zero;
                panelRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }

            // Animate stars sequentially
            AnimateStars(stars);
        }

        private void UpdateWinStars(int stars)
        {
            if (winStarImages == null) return;

            for (int i = 0; i < winStarImages.Length; i++)
            {
                if (winStarImages[i] != null)
                {
                    winStarImages[i].sprite = (i < stars) ? starFilledSprite : starEmptySprite;
                    winStarImages[i].transform.localScale = Vector3.zero;
                }
            }
        }

        private void AnimateStars(int stars)
        {
            if (winStarImages == null) return;

            for (int i = 0; i < winStarImages.Length && i < stars; i++)
            {
                if (winStarImages[i] != null)
                {
                    float delay = 0.5f + (i * 0.2f);
                    winStarImages[i].transform
                        .DOScale(1f, 0.3f)
                        .SetEase(Ease.OutBack)
                        .SetDelay(delay);
                }
            }

            // Show empty stars immediately (no animation)
            for (int i = stars; i < winStarImages.Length; i++)
            {
                if (winStarImages[i] != null)
                {
                    winStarImages[i].transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Hide win panel.
        /// </summary>
        public void HideWinPanel()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }
        }

        #endregion
    }
}
