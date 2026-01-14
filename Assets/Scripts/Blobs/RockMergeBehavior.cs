using UnityEngine;
using Blobs.Core;

namespace Blobs.Blobs
{
    /// <summary>
    /// Rock Blob: Obstacle that blocks direct merges.
    /// Cannot be merged with or moved. Cannot initiate merge.
    /// </summary>
    public class RockMergeBehavior : IMergeBehavior
    {
        public void OnMerge(Blob source, Blob target, GridManager grid)
        {
            // Rock blobs should never be involved in merges
            // This is blocked at the CanMergeWith level
            Debug.Log("[RockMergeBehavior] Rock cannot be merged - this should not be called!");
        }
    }
}
