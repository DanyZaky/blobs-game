using System.Collections.Generic;
using UnityEngine;

namespace Blobs.Commands
{
    /// <summary>
    /// Manages command history for undo/redo functionality.
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager Instance { get; private set; }

        private Stack<ICommand> commandHistory = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();

        [Header("Settings")]
        [SerializeField] private int maxHistorySize = 50;

        public int HistoryCount => commandHistory.Count;
        public bool CanUndo => commandHistory.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public event System.Action OnCommandExecuted;
        public event System.Action OnCommandUndone;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Execute a command and add it to history.
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                Debug.LogWarning("[CommandManager] Null command");
                return;
            }

            command.Execute();
            commandHistory.Push(command);

            // Clear redo stack when new command is executed
            redoStack.Clear();

            // Limit history size
            if (commandHistory.Count > maxHistorySize)
            {
                // Convert to array, remove oldest, convert back
                var tempList = new List<ICommand>(commandHistory);
                tempList.RemoveAt(tempList.Count - 1);
                commandHistory = new Stack<ICommand>(tempList);
            }

            OnCommandExecuted?.Invoke();
            Debug.Log($"[CommandManager] Command executed. History: {commandHistory.Count}");
        }

        /// <summary>
        /// Undo the last command.
        /// </summary>
        public void Undo()
        {
            if (commandHistory.Count == 0)
            {
                Debug.Log("[CommandManager] Nothing to undo");
                return;
            }

            ICommand command = commandHistory.Pop();
            command.Undo();
            redoStack.Push(command);

            OnCommandUndone?.Invoke();
            Debug.Log($"[CommandManager] Undo performed. History: {commandHistory.Count}");
        }

        /// <summary>
        /// Redo the last undone command.
        /// </summary>
        public void Redo()
        {
            if (redoStack.Count == 0)
            {
                Debug.Log("[CommandManager] Nothing to redo");
                return;
            }

            ICommand command = redoStack.Pop();
            command.Execute();
            commandHistory.Push(command);

            OnCommandExecuted?.Invoke();
            Debug.Log($"[CommandManager] Redo performed. History: {commandHistory.Count}");
        }

        /// <summary>
        /// Clear all command history.
        /// </summary>
        public void ClearHistory()
        {
            commandHistory.Clear();
            redoStack.Clear();
            Debug.Log("[CommandManager] History cleared");
        }
    }
}
