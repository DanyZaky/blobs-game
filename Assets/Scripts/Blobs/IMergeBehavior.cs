using Blobs.Core;

namespace Blobs.Blobs
{
    /// <summary>
    /// Strategy interface for different blob merge behaviors.
    /// Implement this to create custom merge logic for special blob types.
    /// </summary>
    public interface IMergeBehavior
    {
        /// <summary>
        /// Execute the merge behavior.
        /// </summary>
        /// <param name="source">The blob initiating the merge</param>
        /// <param name="target">The blob being merged into</param>
        /// <param name="grid">Reference to the grid manager</param>
        void OnMerge(Blob source, Blob target, GridManager grid);
    }
}
