using UnityEngine;
using Blobs.Commands;
using Blobs.Interfaces;
using Blobs.Presenters;
using Blobs.Services;

namespace Blobs.Input
{
    /// <summary>
    /// MVP Input Presenter - Thin coordinator using SRP-compliant services.
    /// 
    /// REFACTORED for SRP compliance:
    /// - Input polling → IInputService
    /// - Hit testing & selection → ISelectionService  
    /// - Move finding & validation → IMoveService
    /// - Feedback display → IFeedbackService
    /// - Game progression → GamePresenter (via events)
    /// 
    /// This class now only coordinates between services.
    /// </summary>
    public class InputPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CommandManager commandManager;

        // Service references via ServiceLocator
        private IInputService InputService => ServiceLocator.Input;
        private ISelectionService SelectionService => ServiceLocator.Selection;
        private IMoveService MoveService => ServiceLocator.Move;
        private IFeedbackService FeedbackService => ServiceLocator.Feedback;

        private void Start()
        {
            if (commandManager == null)
                commandManager = FindObjectOfType<CommandManager>();
            
            // Subscribe to service events
            StartCoroutine(WaitAndSubscribeToServices());
        }
        
        private System.Collections.IEnumerator WaitAndSubscribeToServices()
        {
            // Wait until all services are registered
            while (!ServiceLocator.AreServicesInitialized)
            {
                yield return null;
            }
            
            SubscribeToEvents();
            Debug.Log("[InputPresenter] Subscribed to all services");
        }
        
        private void SubscribeToEvents()
        {
            // Input events
            InputService.OnClickAtPosition += HandleClick;
            InputService.OnDirectionInput += HandleDirection;
            InputService.OnUndoPressed += HandleUndo;
            InputService.OnRedoPressed += HandleRedo;
            
            // Selection events
            SelectionService.OnInvalidSelection += HandleInvalidSelection;
            SelectionService.OnSelectionChanged += HandleSelectionChanged;
            
            // Move events
            MoveService.OnMergeAttemptFailed += HandleMergeAttemptFailed;
            MoveService.OnMergeExecuted += HandleMergeExecuted;
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void UnsubscribeFromEvents()
        {
            if (InputService != null)
            {
                InputService.OnClickAtPosition -= HandleClick;
                InputService.OnDirectionInput -= HandleDirection;
                InputService.OnUndoPressed -= HandleUndo;
                InputService.OnRedoPressed -= HandleRedo;
            }
            
            if (SelectionService != null)
            {
                SelectionService.OnInvalidSelection -= HandleInvalidSelection;
                SelectionService.OnSelectionChanged -= HandleSelectionChanged;
            }
            
            if (MoveService != null)
            {
                MoveService.OnMergeAttemptFailed -= HandleMergeAttemptFailed;
                MoveService.OnMergeExecuted -= HandleMergeExecuted;
            }
        }
        
        #region Input Event Handlers
        
        private void HandleClick(Vector2 worldPosition)
        {
            // Check if we should block input during animations
            if (SelectionService?.SelectedBlob != null && SelectionService.SelectedBlob.IsAnimating)
                return;
            
            SelectionService?.HandleClickAt(worldPosition);
        }
        
        private void HandleDirection(Vector2Int direction)
        {
            if (SelectionService?.SelectedBlob == null)
                return;
            
            if (SelectionService.SelectedBlob.IsAnimating)
                return;
            
            MoveService?.TryMergeInDirection(SelectionService.SelectedBlob, direction);
        }
        
        private void HandleUndo()
        {
            commandManager?.Undo();
        }
        
        private void HandleRedo()
        {
            commandManager?.Redo();
        }
        
        #endregion
        
        #region Selection Event Handlers
        
        private void HandleInvalidSelection(IBlobPresenter blob)
        {
            FeedbackService?.ShowCannotSelectFeedback(blob);
        }
        
        private void HandleSelectionChanged(IBlobPresenter blob)
        {
            // If we have a selection and clicked another blob, try to merge
            if (SelectionService?.SelectedBlob != null && 
                blob != null && 
                blob != SelectionService.SelectedBlob)
            {
                MoveService?.TryMerge(SelectionService.SelectedBlob, blob);
            }
        }
        
        #endregion
        
        #region Move Event Handlers
        
        private void HandleMergeAttemptFailed(IBlobPresenter blob, MoveValidationResult result)
        {
            FeedbackService?.ShowMoveValidationFeedback(blob, result);
        }
        
        private void HandleMergeExecuted(IBlobPresenter source, IBlobPresenter target)
        {
            // Deselect after successful merge
            SelectionService?.Deselect();
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Temporarily block input (used during animations).
        /// </summary>
        public void BlockInput(float duration)
        {
            InputService?.BlockInput(duration);
        }
        
        /// <summary>
        /// Get currently selected blob.
        /// </summary>
        public IBlobPresenter GetSelectedBlob()
        {
            return SelectionService?.SelectedBlob;
        }
        
        #endregion
    }
}
