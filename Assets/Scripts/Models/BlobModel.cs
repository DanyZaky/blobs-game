using UnityEngine;
using Blobs.Blobs;
using Blobs.Interfaces;

namespace Blobs.Models
{
    /// <summary>
    /// Pure data model for a blob. No Unity dependencies except Vector2Int.
    /// </summary>
    [System.Serializable]
    public class BlobModel : IBlobModel
    {
        private BlobType _type;
        private BlobColor _color;
        private Vector2Int _gridPosition;
        private int _size;

        public BlobType Type => _type;
        public BlobColor Color => _color;
        public Vector2Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }
        public int Size
        {
            get => _size;
            set => _size = value;
        }

        public bool CanInitiateMerge => _type == BlobType.Normal || _type == BlobType.Trail;

        public BlobModel(BlobType type, BlobColor color, Vector2Int position, int size = 1)
        {
            _type = type;
            _color = color;
            _gridPosition = position;
            _size = size;
        }

        public bool CanMergeWith(IBlobModel other)
        {
            if (other == null) return false;
            if (other == this) return false;

            // Rock cannot be merged with
            if (other.Type == BlobType.Rock) return false;

            // Flag requires same color
            if (other.Type == BlobType.Flag)
            {
                return other.Color == this.Color;
            }

            // Normal rule: different colors can merge
            if (other.Color == this.Color) return false;

            return true;
        }

        public void SetType(BlobType type)
        {
            _type = type;
        }

        public void SetColor(BlobColor color)
        {
            _color = color;
        }
    }
}
