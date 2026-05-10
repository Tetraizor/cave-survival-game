using System.Collections.Generic;
using CaveTogether.Generation;
using UnityEngine;

namespace CaveTogether.Game.Movement
{
    public static class MapPathFinder
    {
        /// <summary>
        /// Returns [from, ..., to] inclusive, or null if no path exists.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        // Returns [from] if from == to.
        public static Vector2Int[] GetPath(MapData map, Vector2Int from, Vector2Int to)
        {
            if (from == to) return new[] { from };

            var open = new MinHeap();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [from] = 0f };
            var closed = new HashSet<Vector2Int>();

            open.Push(from, Heuristic(from, to));

            while (!open.IsEmpty)
            {
                var current = open.Pop();

                if (current == to)
                    return BuildPath(cameFrom, to);

                if (!closed.Add(current))
                    continue;

                foreach (var neighbour in MovementValidator.GetWalkableNeighbours(map, current))
                {
                    if (closed.Contains(neighbour))
                        continue;

                    float tentativeG = gScore[current] + 1f;

                    if (!gScore.TryGetValue(neighbour, out float existingG) || tentativeG < existingG)
                    {
                        cameFrom[neighbour] = current;
                        gScore[neighbour] = tentativeG;
                        open.Push(neighbour, tentativeG + Heuristic(neighbour, to));
                    }
                }
            }

            return null;
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        private static Vector2Int[] BuildPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end)
        {
            var path = new List<Vector2Int>();
            var node = end;
            while (cameFrom.TryGetValue(node, out var prev))
            {
                path.Add(node);
                node = prev;
            }
            path.Add(node);
            path.Reverse();
            return path.ToArray();
        }

        private sealed class MinHeap
        {
            private readonly List<(Vector2Int pos, float priority)> _data = new();

            public bool IsEmpty => _data.Count == 0;

            public void Push(Vector2Int pos, float priority)
            {
                _data.Add((pos, priority));
                BubbleUp(_data.Count - 1);
            }

            public Vector2Int Pop()
            {
                var top = _data[0].pos;
                int last = _data.Count - 1;
                _data[0] = _data[last];
                _data.RemoveAt(last);
                if (_data.Count > 0) SiftDown(0);
                return top;
            }

            private void BubbleUp(int i)
            {
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (_data[parent].priority <= _data[i].priority) break;
                    Swap(i, parent);
                    i = parent;
                }
            }

            private void SiftDown(int i)
            {
                while (true)
                {
                    int left = 2 * i + 1, right = 2 * i + 2, smallest = i;
                    if (left < _data.Count && _data[left].priority < _data[smallest].priority) smallest = left;
                    if (right < _data.Count && _data[right].priority < _data[smallest].priority) smallest = right;
                    if (smallest == i) break;
                    Swap(i, smallest);
                    i = smallest;
                }
            }

            private void Swap(int a, int b) => (_data[a], _data[b]) = (_data[b], _data[a]);
        }
    }
}