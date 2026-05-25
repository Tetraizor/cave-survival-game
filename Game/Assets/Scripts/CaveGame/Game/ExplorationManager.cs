using System;
using CaveTogether.Common.Enums;
using CaveTogether.Generation;
using UnityEngine;

namespace CaveTogether.Game
{
    public class ExplorationManager : MonoBehaviour
    {
        public event Action<Vector2Int> CellRevealed;

        private bool[,] _explored;
        private MapData _map;
        private MapRenderManager _renderManager;

        public void Initialize(MapData map)
        {
            _map = map;
            _renderManager = FindAnyObjectByType<MapRenderManager>();
            _explored = new bool[map.Height, map.Width];
            CellRevealed += _renderManager.OnCellRevealed;
        }

        public bool IsExplored(Vector2Int pos) =>
            _map.IsInsideBounds(pos) && _explored[pos.y, pos.x];

        public void RevealFromPosition(Vector2Int pos) => RevealCell(pos);

        public bool IsFrontier(Vector2Int pos)
        {
            if (IsExplored(pos) || !_map.IsInsideBounds(pos)) return false;
            if (_map.GetCellRef(pos).IsEmpty) return false;

            Direction[] dirs = { Direction.North, Direction.East, Direction.South, Direction.West };
            foreach (var dir in dirs)
            {
                var neighborPos = pos - dir.ToVector();
                if (!_map.IsInsideBounds(neighborPos) || !IsExplored(neighborPos)) continue;

                ref var neighbor = ref _map.GetCellRef(neighborPos);
                if (neighbor.IsEmpty) continue;

                var openDirs = DirectionHelpers.RotateDirectionsClockwise(neighbor.Data.OpenDirections, neighbor.Orientation);
                if ((openDirs & dir) != 0) return true;
            }
            return false;
        }

        private void RevealCell(Vector2Int pos)
        {
            if (_explored[pos.y, pos.x]) return;
            _explored[pos.y, pos.x] = true;
            _renderManager.SpawnCell(pos);
            CellRevealed?.Invoke(pos);
        }
    }
}
