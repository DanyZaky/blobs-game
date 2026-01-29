using UnityEngine;
using Blobs.Core;
using Blobs.Blobs;
using Blobs.Commands;
using Blobs.Interfaces;
using Blobs.Presenters;
using Blobs.Views;

namespace Blobs.Input
{
    /// <summary>
    /// MVP Input Presenter - handles player input using ServiceLocator.
    /// Replaces legacy InputManager with clean architecture.
    /// </summary>
    public class InputPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CommandManager commandManager;

        [Header("Settings")]
        [SerializeField] private KeyCode undoKey = KeyCode.Z;
        [SerializeField] private KeyCode redoKey = KeyCode.Y;

        private IBlobPresenter selectedBlob;
        private Camera mainCamera;
        private bool isInputBlocked;

        // Service references
        private IGamePresenter Game => ServiceLocator.Game;
        private IGridPresenter Grid => ServiceLocator.Grid;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            if (commandManager == null)
                commandManager = FindObjectOfType<CommandManager>();
        }

        private void Update()
        {
            // Check game state via ServiceLocator
            if (Game?.CurrentState != GameState.Playing)
                return;

            // Block input during animations
            if (isInputBlocked || (selectedBlob != null && selectedBlob.IsAnimating))
                return;

            HandleMouseInput();
            HandleKeyboardInput();
            HandleUndoRedo();
        }

        /// <summary>
        /// Temporarily block input (used during animations).
        /// </summary>
        public void BlockInput(float duration)
        {
            StartCoroutine(BlockInputCoroutine(duration));
        }

        private System.Collections.IEnumerator BlockInputCoroutine(float duration)
        {
            isInputBlocked = true;
            yield return new WaitForSeconds(duration);
            isInputBlocked = false;
        }

        private void HandleMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    // Try to get BlobView from collider
                    BlobView blobView = hit.collider.GetComponent<BlobView>();
                    if (blobView != null)
                    {
                        IBlobPresenter presenter = GetPresenterForView(blobView);
                        if (presenter != null)
                        {
                            HandleBlobClick(presenter);
                        }
                    }
                }
                else if (selectedBlob != null)
                {
                    // Clicked empty space - deselect
                    DeselectCurrentBlob();
                }
            }
        }

        private IBlobPresenter GetPresenterForView(BlobView view)
        {
            // Find presenter that owns this view
            if (Grid == null) return null;

            foreach (var blob in Grid.GetAllBlobs())
            {
                if (blob.View == view as IBlobView)
                {
                    return blob;
                }
            }
            return null;
        }

        private void HandleBlobClick(IBlobPresenter clickedBlob)
        {
            if (selectedBlob == null)
            {
                // Select blob if it can initiate merge
                if (clickedBlob.Model.CanInitiateMerge)
                {
                    SelectBlob(clickedBlob);
                }
                else
                {
                    // Show feedback for non-selectable blobs
                    UIManager.Instance?.ShowCannotSelectFeedback();
                    clickedBlob.PlayInvalidMoveEffect();
                }
            }
            else if (selectedBlob == clickedBlob)
            {
                // Clicked same blob - deselect
                DeselectCurrentBlob();
            }
            else
            {
                // Clicked different blob - try to merge
                TryMerge(selectedBlob, clickedBlob);
            }
        }

        private void HandleKeyboardInput()
        {
            if (selectedBlob == null) return;
            if (Grid == null) return;

            Vector2Int direction = Vector2Int.zero;

            if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W))
                direction = Vector2Int.up;
            else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S))
                direction = Vector2Int.down;
            else if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
                direction = Vector2Int.left;
            else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D))
                direction = Vector2Int.right;

            if (direction != Vector2Int.zero)
            {
                TryMergeInDirection(direction);
            }
        }

        private void TryMergeInDirection(Vector2Int direction)
        {
            if (selectedBlob?.Model == null) return;
            if (Grid == null) return;

            Vector2Int currentPos = selectedBlob.Model.GridPosition;

            // Find first blob in that direction
            Vector2Int checkPos = currentPos + direction;
            while (Grid.IsValidPosition(checkPos))
            {
                IBlobPresenter targetBlob = Grid.GetBlobAt(checkPos);
                if (targetBlob != null)
                {
                    TryMerge(selectedBlob, targetBlob);
                    return;
                }
                checkPos += direction;
            }

            Debug.Log("[InputPresenter] No blob found in direction");
            UIManager.Instance?.ShowNoMoveFeedback();
            selectedBlob?.PlayInvalidMoveEffect();
        }

        private void TryMerge(IBlobPresenter source, IBlobPresenter target)
        {
            if (!source.Model.CanMergeWith(target.Model))
            {
                Debug.Log($"[InputPresenter] Cannot merge {source.Model.Color} with {target.Model.Color}");

                // Show appropriate feedback
                if (target.Model.Color == source.Model.Color)
                {
                    UIManager.Instance?.ShowSameColorFeedback();
                }
                else
                {
                    UIManager.Instance?.ShowCannotMergeFeedback();
                }
                source.PlayInvalidMoveEffect();
                return;
            }

            // Check if they are on same row or column
            Vector2Int sourcePos = source.Model.GridPosition;
            Vector2Int targetPos = target.Model.GridPosition;

            if (sourcePos.x != targetPos.x && sourcePos.y != targetPos.y)
            {
                Debug.Log("[InputPresenter] Blobs must be on same row or column");
                return;
            }

            // Execute merge via presenter
            source.ExecuteMerge(target);

            // Increment move count
            Game?.IncrementMoveCount();

            // Check win condition
            Game?.CheckWinCondition();

            DeselectCurrentBlob();
        }

        private void HandleUndoRedo()
        {
            if (UnityEngine.Input.GetKeyDown(undoKey))
            {
                commandManager?.Undo();
            }
            else if (UnityEngine.Input.GetKeyDown(redoKey))
            {
                commandManager?.Redo();
            }
        }

        private void SelectBlob(IBlobPresenter blob)
        {
            if (selectedBlob != null)
            {
                selectedBlob.Deselect();
            }

            selectedBlob = blob;
            selectedBlob.Select();
        }

        private void DeselectCurrentBlob()
        {
            if (selectedBlob != null)
            {
                selectedBlob.Deselect();
                selectedBlob = null;
            }
        }

        public IBlobPresenter GetSelectedBlob()
        {
            return selectedBlob;
        }
    }
}
