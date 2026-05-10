using CaveTogether.Common.Enums;
using UnityEditor;
using UnityEngine;

namespace CaveTogether.Generation
{
    public class MapRenderManager : MonoBehaviour
    {
        public const int CELL_SIZE = 2;

        [SerializeField] private Transform _caveCellContainer;

        private MapManager _manager;
        private MapGenerator _generator;

        private bool _isInitialized;

        public void Initialize(MapManager manager)
        {
            _manager = manager;

            _generator = manager.Generator;
            _generator.GenerationLayerFinished += OnGenerationLayerFinished;

            _isInitialized = true;
        }

        private void OnGenerationLayerFinished(MapGenerationLayerBase layerBase)
        {
            // TODO: Hacky solution, do real OOP
            if (layerBase.GetType() == typeof(BaseGenerationLayer))
            {
                for (int y = 0; y < _manager.Map.Height; y++)
                {
                    for (int x = 0; x < _manager.Map.Width; x++)
                    {
                        ref var cellRef = ref _manager.Map.GetCellRef(new Vector2Int(x, y));

                        if (!cellRef.IsEmpty)
                        {
                            var prefab = cellRef.Data.Prefab;
                            Vector3 realPosition = new Vector3(cellRef.X * CELL_SIZE, 0, cellRef.Y * CELL_SIZE);

                            var cell = Instantiate(prefab, realPosition, Quaternion.identity, _caveCellContainer);
                            cell.transform.RotateAround(realPosition + new Vector3(CELL_SIZE / 2, 0, CELL_SIZE / 2), Vector3.up, cellRef.Orientation * 90);
                        }
                    }
                }
            }
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