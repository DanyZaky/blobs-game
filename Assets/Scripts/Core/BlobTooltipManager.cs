using UnityEngine;
using Blobs.Interfaces;
using Blobs.Presenters;
using Blobs.Views;

namespace Blobs.Core
{
    /// <summary>
    /// Manages blob tooltips using Simple Tooltip plugin.
    /// Updated for MVP architecture.
    /// </summary>
    public class BlobTooltipManager : MonoBehaviour
    {
        public static BlobTooltipManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float hoverDelay = 0.3f;
        [SerializeField] private LayerMask blobLayer = -1;

        private STController tooltipController;
        private SimpleTooltipStyle tooltipStyle;
        private Camera mainCamera;
        
        private BlobView currentHoveredBlob;
        private float hoverTimer;
        private bool isShowingTooltip;

        // MVP reference
        private IGamePresenter Game => ServiceLocator.Game;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            mainCamera = Camera.main;

            // Auto-instantiate tooltip prefab if not exists
            tooltipController = FindObjectOfType<STController>();
            if (tooltipController == null)
            {
                tooltipController = SimpleTooltip.AddTooltipPrefabToScene();
            }

            // Load default style
            tooltipStyle = Resources.Load<SimpleTooltipStyle>("STDefault");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            // Use ServiceLocator instead of GameManager.Instance
            if (Game?.CurrentState != GameState.Playing)
            {
                HideTooltip();
                return;
            }

            DetectBlobUnderMouse();
        }

        private void DetectBlobUnderMouse()
        {
            if (mainCamera == null) return;

            Vector2 mousePos = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, blobLayer);

            BlobView hoveredBlob = null;
            if (hit.collider != null)
            {
                hoveredBlob = hit.collider.GetComponent<BlobView>();
            }

            // Check if hover changed
            if (hoveredBlob != currentHoveredBlob)
            {
                currentHoveredBlob = hoveredBlob;
                hoverTimer = 0f;
                
                if (hoveredBlob == null)
                {
                    HideTooltip();
                }
                else
                {
                    isShowingTooltip = false;
                }
            }

            // Update hover timer and show tooltip after delay
            if (currentHoveredBlob != null && !isShowingTooltip)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= hoverDelay)
                {
                    ShowTooltipForBlob(currentHoveredBlob);
                }
            }

            // Keep tooltip visible while hovering
            if (isShowingTooltip && tooltipController != null)
            {
                tooltipController.ShowTooltip();
            }
        }

        private void ShowTooltipForBlob(BlobView blob)
        {
            if (tooltipController == null || tooltipStyle == null)
                return;

            var (title, description, hint) = blob.GetTooltipInfo();

            // Format tooltip text with styling
            string tooltipText = $"<b><size=120%>{title}</size></b>\n{description}";
            
            // Add hint in smaller text if available
            string hintText = "";
            if (!string.IsNullOrEmpty(hint))
            {
                hintText = $"<i><color=#888888>{hint}</color></i>";
            }

            // Set text and show
            tooltipController.SetCustomStyledText(tooltipText, tooltipStyle, STController.TextAlign.Left);
            tooltipController.SetCustomStyledText(hintText, tooltipStyle, STController.TextAlign.Right);
            tooltipController.ShowTooltip();

            isShowingTooltip = true;
        }

        private void HideTooltip()
        {
            if (tooltipController != null && isShowingTooltip)
            {
                tooltipController.HideTooltip();
            }
            isShowingTooltip = false;
            hoverTimer = 0f;
        }

        /// <summary>
        /// Force show tooltip for a specific blob view
        /// </summary>
        public void ForceShowTooltip(BlobView blob)
        {
            if (blob == null) return;
            currentHoveredBlob = blob;
            ShowTooltipForBlob(blob);
        }

        /// <summary>
        /// Force hide tooltip
        /// </summary>
        public void ForceHideTooltip()
        {
            HideTooltip();
            currentHoveredBlob = null;
        }
    }
}
