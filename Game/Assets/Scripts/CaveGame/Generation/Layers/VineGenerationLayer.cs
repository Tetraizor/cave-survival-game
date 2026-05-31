using System.Collections.Generic;
using CaveTogether.Game.CellEffects;
using CaveTogether.Generation.Features;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public class VineGenerationLayer : MapGenerationLayerBase, ICellEffectProvider
    {
        private readonly HashSet<Vector2Int> _vineCells = new();
        private readonly double _chance;

        public IReadOnlyCollection<Vector2Int> VineCells => _vineCells;

        public VineGenerationLayer(double chance = 0.2) => _chance = chance;

        public IEnumerable<CellEffect> GetEffectsForCell(Vector2Int pos)
        {
            if (_vineCells.Contains(pos))
                yield return new CellEffect(CellEffectTrigger.OnEnter, new DamageEffect(1));
        }

        public override void Process(MapData mapData, System.Random random)
        {
            var spawnPos = mapData.GetFeature<SpawnFeature>()?.PlayerSpawnPosition;
            var exitPos = mapData.GetFeature<ExitFeature>()?.ExitPosition;

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (mapData.GetCellRef(x, y).IsEmpty) continue;
                    if (pos == spawnPos || pos == exitPos) continue;
                    if (random.NextDouble() < _chance)
                        _vineCells.Add(pos);
                }
            }
        }
    }
}