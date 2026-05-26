using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common.Enums;
using CaveTogether.Generation.Decorations;
using CaveTogether.Generation.Layers;
using DG.Tweening;
using UnityEngine;

namespace CaveTogether.Generation
{
    public class MapRenderManager : MonoBehaviour
    {
        public const int CELL_SIZE = 2;

        [SerializeField] private Transform _caveCellContainer;
        [SerializeField] private GameObject _fogPrefab;

        private MapManager _manager;
        private MapGenerator _generator;

        private readonly HashSet<Vector2Int> _spawnedCells = new();
        private readonly Dictionary<Vector2Int, GameObject> _fogInstances = new();

        private List<ICellDecorator> _cellDecorators;

        private bool _isInitialized;

        public void Initialize(MapManager manager)
        {
            _manager = manager;

            _generator = manager.Generator;
            _generator.GenerationLayerFinished += OnGenerationLayerFinished;

            _isInitialized = true;

            _cellDecorators = FindObjectsByType<MonoBehaviour>().OfType<ICellDecorator>().ToList();
        }

        private void OnGenerationLayerFinished(MapGenerationLayerBase layerBase) { }

        public void SpawnCell(Vector2Int pos)
        {
            ref var cellRef = ref _manager.Map.GetCellRef(pos);
            if (cellRef.IsEmpty) return;

            Vector3 realPosition = new Vector3(cellRef.X * CELL_SIZE, 0, cellRef.Y * CELL_SIZE);

            var cell = Instantiate(cellRef.Data.Prefab, realPosition, Quaternion.identity, _caveCellContainer);
            cell.transform.RotateAround(realPosition + new Vector3(CELL_SIZE / 2, 0, CELL_SIZE / 2), Vector3.up, cellRef.Orientation * 90);

            Vector3 finalPos = cell.transform.position;
            cell.transform.position = finalPos + Vector3.down * 2f;
            cell.transform.localScale = Vector3.zero;

            DOTween.Sequence()
                .Join(cell.transform.DOMove(finalPos, 0.3f).SetEase(Ease.OutCubic))
                .Join(cell.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            _spawnedCells.Add(pos);
        }

        public void OnCellRevealed(Vector2Int pos)
        {
            if (_fogInstances.TryGetValue(pos, out var existing))
            {
                _fogInstances.Remove(pos);
                var ps = existing.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    StartCoroutine(DestroyWhenFinished(ps, existing));
                }
                else
                {
                    Destroy(existing);
                }
            }

            if (_fogPrefab == null) return;

            ref var cell = ref _manager.Map.GetCellRef(pos);
            if (cell.IsEmpty) return;

            var openDirs = DirectionHelpers.DirectionFlagToList(
                DirectionHelpers.RotateDirectionsClockwise(cell.Data.OpenDirections, cell.Orientation));

            foreach (var dir in openDirs)
            {
                var neighborPos = DirectionHelpers.AddDirectionToPosition(pos, dir);
                if (!_manager.Map.IsInsideBounds(neighborPos)) continue;
                if (_manager.Map.GetCellRef(neighborPos).IsEmpty) continue;
                if (_spawnedCells.Contains(neighborPos) || _fogInstances.ContainsKey(neighborPos)) continue;

                _fogInstances[neighborPos] = Instantiate(_fogPrefab, GridToWorldPosition(neighborPos) + Vector3.one, Quaternion.identity, _caveCellContainer);
            }

            foreach (var decorator in _cellDecorators)
                decorator.DecorateCell(pos, _manager.Map, _caveCellContainer);
        }

        private IEnumerator DestroyWhenFinished(ParticleSystem ps, GameObject go)
        {
            while (ps.IsAlive(true))
                yield return null;
            Destroy(go);
        }

        private void OnDrawGizmos()
        {
            if (!_isInitialized || _manager == null || _manager.Map == null) return;

            Gizmos.color = Color.red;

            Gizmos.DrawCube(
                new Vector3(_manager.Map.Width / 2f, 0, _manager.Map.Height / 2f) * CELL_SIZE + new Vector3(CELL_SIZE / 2, 0, CELL_SIZE / 2),
                Vector3.one * CELL_SIZE
            );

            for (int y = 0; y < _manager.Map.Height; y++)
            {
                for (int x = 0; x < _manager.Map.Width; x++)
                {
                    var cell = _manager.Map.Cells[y, x];

                    if (cell.Data != null)
                    {
                        Vector2Int cellCenter = new Vector2Int(
                            CELL_SIZE * cell.X + CELL_SIZE / 2,
                            CELL_SIZE * cell.Y + CELL_SIZE / 2
                        );

                        foreach (var dir in DirectionHelpers.DirectionFlagToList(DirectionHelpers.RotateDirectionsClockwise(cell.Data.OpenDirections, cell.Orientation)))
                        {
                            var directionEnd = (Vector2)cellCenter + ((Vector2)dir.ToVector() * (CELL_SIZE / 2f));

                            Gizmos.DrawLine(
                                new Vector3(cellCenter.x, 1, cellCenter.y),
                                new Vector3(directionEnd.x, 1, directionEnd.y)
                            );
                        }
                    }
                }
            }
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x * CELL_SIZE, 0, gridPosition.y * CELL_SIZE);
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            return new Vector2Int((int)worldPosition.x / CELL_SIZE, (int)worldPosition.z / CELL_SIZE);
        }
    }
}