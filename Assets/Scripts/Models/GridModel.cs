using UnityEngine;
using System.Collections.Generic;
using Blobs.Blobs;
using Blobs.Interfaces;

namespace Blobs.Models
{
    /// <summary>
    /// Pure data model for the grid state. No Unity dependencies except Vector2Int.
    /// </summary>
    [System.Serializable]
    public class GridModel
    {
        private int _width;
        private int _height;
        private Dictionary<Vector2Int, IBlobPresenter> _blobs;

        public int Width => _width;
        public int Height => _height;
        public int BlobCount => _blobs.Count;

        public GridModel(int width, int height)
        {
            _width = width;
            _height = height;
            _blobs = new Dictionary<Vector2Int, IBlobPresenter>();
        }

        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Clear()
        {
            _blobs.Clear();
        }

        public void AddBlob(Vector2Int position, IBlobPresenter blob)
        {
            _blobs[position] = blob;
        }

        public void RemoveBlob(Vector2Int position)
        {
            _blobs.Remove(position);
        }

        public void MoveBlob(Vector2Int from, Vector2Int to)
        {
            if (_blobs.TryGetValue(from, out var blob))
            {
                _blobs.Remove(from);
                _blobs[to] = blob;
            }
        }

        public IBlobPresenter GetBlobAt(Vector2Int position)
        {
            _blobs.TryGetValue(position, out var blob);
            return blob;
        }

        public bool IsPositionOccupied(Vector2Int position)
        {
            return _blobs.ContainsKey(position);
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < _width &&
                   position.y >= 0 && position.y < _height;
        }

        public List<IBlobPresenter> GetAllBlobs()
        {
            return new List<IBlobPresenter>(_blobs.Values);
        }

        public int GetPlayableBlobCount()
        {
            int count = 0;
            foreach (var blob in _blobs.Values)
            {
                if (blob.Model.Type != BlobType.Rock)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
