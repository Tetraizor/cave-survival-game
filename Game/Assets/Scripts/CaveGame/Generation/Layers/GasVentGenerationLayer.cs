using System.Collections.Generic;
using CaveTogether.Game.RoundEvents;
using CaveTogether.Generation.Features;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public class GasVentGenerationLayer : MapGenerationLayerBase, IRoundEventProvider
    {
        private const int VentCount = 6;
        private readonly HashSet<Vector2Int> _ventCells = new();
        private MapData _mapData;
        private GasSpreadEvent _event;

        public IReadOnlyCollection<Vector2Int> VentCells => _ventCells;

        public IEnumerable<RoundEventBase> GetRoundEvents()
        {
            _event ??= new GasSpreadEvent(_ventCells, _mapData);
            yield return _event;
        }

        public bool IsGasCell(Vector2Int pos) => _event?.IsGasCell(pos) ?? false;

        public GasSpreadEvent GetEvent()
        {
            _event ??= new GasSpreadEvent(_ventCells, _mapData);
            return _event;
        }

        public override void Process(MapData mapData, System.Random random)
        {
            _mapData = mapData;
            var spawnPos = mapData.GetFeature<SpawnFeature>()?.PlayerSpawnPosition;
            var exitPos = mapData.GetFeature<ExitFeature>()?.ExitPosition;

            var candidates = new List<Vector2Int>();
            for (int y = 0; y < mapData.Height; y++)
                for (int x = 0; x < mapData.Width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (mapData.GetCellRef(x, y).IsEmpty) continue;
                    if (pos == spawnPos || pos == exitPos) continue;
                    candidates.Add(pos);
                }

            for (int i = 0; i < VentCount && candidates.Count > 0; i++)
            {
                int index = random.Next(candidates.Count);
                _ventCells.Add(candidates[index]);
                candidates.RemoveAt(index);
            }
        }
    }
}
