using System.Runtime.Serialization.Json;
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

                            Instantiate(prefab, realPosition, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }
}