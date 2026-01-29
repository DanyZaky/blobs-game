using UnityEngine;
using Blobs.Blobs;

namespace Blobs.Interfaces
{
    /// <summary>
    /// Interface for blob data model (pure data, no Unity dependencies)
    /// </summary>
    public interface IBlobModel
    {
        BlobType Type { get; }
        BlobColor Color { get; }
        Vector2Int GridPosition { get; set; }
        int Size { get; set; }
        bool CanInitiateMerge { get; }
        bool CanMergeWith(IBlobModel other);
    }
}
