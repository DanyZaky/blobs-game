using Blobs.Interfaces;

namespace Blobs.Blobs
{
    /// <summary>
    /// Strategy interface for different blob merge behaviors.
    /// Implement this to create custom merge logic for special blob types.
    /// SOLID: Open/Closed Principle - extend via new implementations.
    /// </summary>
    public interface IMergeBehavior
    {
        /// <summary>
        /// Execute the merge behavior.
        /// </summary>
        /// <param name="source">The blob presenter initiating the merge</param>
        /// <param name="target">The blob presenter being merged into</param>
        /// <param name="grid">Reference to the grid presenter</param>
        void OnMerge(IBlobPresenter source, IBlobPresenter target, IGridPresenter grid);
    }
}
