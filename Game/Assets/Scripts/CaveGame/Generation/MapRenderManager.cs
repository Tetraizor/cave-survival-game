using System.Runtime.Serialization.Json;
using CaveGame.CommonEnums;
using UnityEngine;

namespace CaveGame.Generation
{
    public class MapRenderManager : MonoBehaviour
    {
        private MapManager _manager;
        private MapGenerator _generator;

        public void Initialize(MapManager manager)
        {
            _manager = manager;

            _generator = manager.Generator;
            _generator.GenerationLayerFinished += OnGenerationLayerFinished;
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

                        if (!cellRef.IsEmpty())
                        {
                            var prefab = cellRef.Data.Prefab;
                            Vector3 realPosition = new Vector3(cellRef.X * MapManager.CELL_SIZE, 0, cellRef.Y * MapManager.CELL_SIZE);

                            var cell = Instantiate(prefab, realPosition, Quaternion.identity);
                            cell.transform.RotateAround(realPosition + new Vector3(MapManager.CELL_SIZE / 2, 0, MapManager.CELL_SIZE / 2), Vector3.up, cellRef.Orientation * 90);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            for (int y = 0; y < MapManager.MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MapManager.MAP_WIDTH; x++)
                {
                    var cell = _manager.Map.Cells[y, x];

                    if (cell.Data != null)
                    {
                        Vector2Int cellCenter = new Vector2Int(
                            MapManager.CELL_SIZE * cell.X + MapManager.CELL_SIZE / 2,
                            MapManager.CELL_SIZE * cell.Y + MapManager.CELL_SIZE / 2
                        );

                        foreach (var dir in DirectionHelpers.DirectionFlagToList(DirectionHelpers.RotateDirectionsClockwise(cell.Data.OpenDirections, cell.Orientation)))
                        {
                            var directionEnd = cellCenter + dir.ToVector() * MapManager.CELL_SIZE / 2;

                            Debug.DrawLine(
                                new Vector3(cellCenter.x, 1, cellCenter.y),
                                new Vector3(directionEnd.x, 1, directionEnd.y),
                                Color.red, 0
                            );
                        }
                    }
                }
            }
        }
    }
}