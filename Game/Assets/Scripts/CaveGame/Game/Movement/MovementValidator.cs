using System;
using System.Collections.Generic;
using CaveTogether.Common.Enums;
using CaveTogether.Generation;
using UnityEngine;

namespace CaveTogether.Game.Movement
{
    public static class MovementValidator
    {
        public static bool CanMove(MapData map, Vector2Int from, Vector2Int to, Func<Vector2Int, bool> isPassable = null) => CanMove(map, from, to, out _, isPassable);
        public static bool CanMove(MapData map, Vector2Int from, Vector2Int to, out int distance, Func<Vector2Int, bool> isPassable = null)
        {
            distance = -1;
            if (!map.IsInsideBounds(from) || map.GetCellRef(from).IsEmpty) return false;
            if (!map.IsInsideBounds(to) || map.GetCellRef(to).IsEmpty) return false;

            var path = MapPathFinder.GetPath(map, from, to, isPassable);
            if (path == null || path.Length == 0) return false;
            else distance = path.Length - 1;

            return true;
        }

        public static List<Vector2Int> GetAllWalkableCells(MapData map)
        {
            var cells = new List<Vector2Int>();

            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    if (!map.GetCellRef(x, y).IsEmpty)
                        cells.Add(new Vector2Int(x, y));

            return cells;
        }

        public static List<Vector2Int> GetWalkableNeighbours(MapData map, Vector2Int pos)
        {
            var walkableNeighbours = new List<Vector2Int>();
            if (!map.IsInsideBounds(pos) || map.GetCellRef(pos).IsEmpty) return walkableNeighbours;
            var centerCellRef = map.GetCellRef(pos);

            var possibleCellPositions = new (Vector2Int Position, Direction Direction)[] {
                (pos + Vector2Int.up, Direction.North),
                (pos + Vector2Int.down, Direction.South),
                (pos + Vector2Int.left, Direction.West),
                (pos + Vector2Int.right, Direction.East),
            };

            foreach (var possiblePos in possibleCellPositions)
            {
                if (!map.IsInsideBounds(possiblePos.Position)) continue;

                var effectiveDirections = DirectionHelpers.RotateDirectionsClockwise(
                    centerCellRef.Data.OpenDirections, centerCellRef.Orientation
                );

                if ((effectiveDirections & possiblePos.Direction) == Direction.None) continue;

                var neighbourCellRef = map.GetCellRef(possiblePos.Position);
                if (neighbourCellRef.IsEmpty) continue;

                var neighbourEffective = DirectionHelpers.RotateDirectionsClockwise(
                    neighbourCellRef.Data.OpenDirections, neighbourCellRef.Orientation);

                var requiredNeighbourOpening = DirectionHelpers.GetOppositeDirection(possiblePos.Direction);

                if ((neighbourEffective & requiredNeighbourOpening) == Direction.None) continue;

                walkableNeighbours.Add(possiblePos.Position);
            }

            return walkableNeighbours;
        }

        public static int GetDistance(MapData map, Vector2Int from, Vector2Int to, Func<Vector2Int, bool> isPassable = null)
        {
            if (CanMove(map, from, to, out int distance, isPassable))
                return distance;
            else
                return -1;
        }
    }
}