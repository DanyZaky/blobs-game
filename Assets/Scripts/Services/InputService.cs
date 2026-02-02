using UnityEngine;
using System;
using System.Collections;
using Blobs.Core;
using Blobs.Interfaces;
using Blobs.Presenters;

namespace Blobs.Services
{
    /// <summary>
    /// Input polling service - handles raw input detection only.
    /// SRP: Only polls input and emits events.
    /// Does NOT handle selection, validation, or game logic.
    /// </summary>
    public class InputService : MonoBehaviour, IInputService
    {
        [Header("Settings")]
        [SerializeField] private KeyCode undoKey = KeyCode.Z;
        [SerializeField] private KeyCode redoKey = KeyCode.Y;
        
        private Camera mainCamera;
        private bool isInputBlocked;
        private bool isInputEnabled = true;
        
        // Service reference
        private IGamePresenter Game => ServiceLocator.Game;
        
        public bool IsInputBlocked => isInputBlocked || !isInputEnabled;
        
        // Events
        public event Action<Vector2> OnClickAtPosition;
        public event Action<Vector2Int> OnDirectionInput;
        public event Action OnUndoPressed;
        public event Action OnRedoPressed;
        
        private void Awake()
        {
            mainCamera = Camera.main;
            
            // Self-register to ServiceLocator
            ServiceLocator.RegisterInput(this);
        }
        
        private void Update()
        {
            // Check game state - only process input when playing
            if (Game?.CurrentState != GameState.Playing)
                return;
            
            if (IsInputBlocked)
                return;
            
            PollMouseInput();
            PollKeyboardInput();
            PollUndoRedo();
        }
        
        private void PollMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                OnClickAtPosition?.Invoke(worldPos);
            }
        }
        
        private void PollKeyboardInput()
        {
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
                OnDirectionInput?.Invoke(direction);
            }
        }
        
        private void PollUndoRedo()
        {
            if (UnityEngine.Input.GetKeyDown(undoKey))
            {
                OnUndoPressed?.Invoke();
            }
            else if (UnityEngine.Input.GetKeyDown(redoKey))
            {
                OnRedoPressed?.Invoke();
            }
        }
        
        public void BlockInput(float duration)
        {
            StartCoroutine(BlockInputCoroutine(duration));
        }
        
        private IEnumerator BlockInputCoroutine(float duration)
        {
            isInputBlocked = true;
            yield return new WaitForSeconds(duration);
            isInputBlocked = false;
        }
        
        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;
        }
    }
}
