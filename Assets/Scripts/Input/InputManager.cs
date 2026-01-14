using UnityEngine;
using Blobs.Core;
using Blobs.Blobs;
using Blobs.Commands;

namespace Blobs.Input
{
    /// <summary>
    /// Handles player input for blob selection and merge commands.
    /// Supports keyboard (arrow keys) and mouse input.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private CommandManager commandManager;

        [Header("Settings")]
        [SerializeField] private KeyCode undoKey = KeyCode.Z;
        [SerializeField] private KeyCode redoKey = KeyCode.Y;

        private Blob selectedBlob;
        private Camera mainCamera;
        private bool isInputBlocked;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            if (gridManager == null)
                gridManager = FindObjectOfType<GridManager>();
            if (commandManager == null)
                commandManager = FindObjectOfType<CommandManager>();
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing)
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
                    Blob clickedBlob = hit.collider.GetComponent<Blob>();
                    if (clickedBlob != null)
                    {
                        HandleBlobClick(clickedBlob);
                    }
                }
                else if (selectedBlob != null)
                {
                    // Clicked empty space - deselect
                    DeselectCurrentBlob();
                }
            }
        }

        private void HandleBlobClick(Blob clickedBlob)
        {
            if (selectedBlob == null)
            {
                // Select blob if it can initiate merge
                if (clickedBlob.CanInitiateMerge)
                {
                    SelectBlob(clickedBlob);
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
            if (selectedBlob?.CurrentTile == null) return;

            Vector2Int currentPos = selectedBlob.CurrentTile.GridPosition;
            
            // Find first blob in that direction
            Vector2Int checkPos = currentPos + direction;
            while (gridManager.IsValidPosition(checkPos))
            {
                Blob targetBlob = gridManager.GetBlobAt(checkPos);
                if (targetBlob != null)
                {
                    TryMerge(selectedBlob, targetBlob);
                    return;
                }
                checkPos += direction;
            }

            Debug.Log("[InputManager] No blob found in direction");
        }

        private void TryMerge(Blob source, Blob target)
        {
            if (!source.CanMergeWith(target))
            {
                Debug.Log($"[InputManager] Cannot merge {source.BlobColorType} with {target.BlobColorType}");
                return;
            }

            // Check if they are on same row or column
            Vector2Int sourcePos = source.CurrentTile.GridPosition;
            Vector2Int targetPos = target.CurrentTile.GridPosition;

            if (sourcePos.x != targetPos.x && sourcePos.y != targetPos.y)
            {
                Debug.Log("[InputManager] Blobs must be on same row or column");
                return;
            }

            // Create and execute merge command
            MergeCommand mergeCommand = new MergeCommand(source, target, gridManager);
            commandManager.ExecuteCommand(mergeCommand);

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

        private void SelectBlob(Blob blob)
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

        public Blob GetSelectedBlob()
        {
            return selectedBlob;
        }
    }
}
