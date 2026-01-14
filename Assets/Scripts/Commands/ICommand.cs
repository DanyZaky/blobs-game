namespace Blobs.Commands
{
    /// <summary>
    /// Command pattern interface for undoable actions.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Execute the command.
        /// </summary>
        void Execute();

        /// <summary>
        /// Undo the command, reverting to previous state.
        /// </summary>
        void Undo();
    }
}
