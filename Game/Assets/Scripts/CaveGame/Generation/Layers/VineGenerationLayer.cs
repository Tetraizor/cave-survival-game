using System.Collections;
using System.Collections.Generic;
using CaveTogether.Game.Entities;
using CaveTogether.Generation.Features;
using CaveTogether.Items;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public class VineGenerationLayer : MapGenerationLayerBase, ICellWalkOptionsProvider
    {
        private readonly HashSet<Vector2Int> _vineCells = new();
        private readonly double _chance;

        public IReadOnlyCollection<Vector2Int> VineCells => _vineCells;

        public VineGenerationLayer(double chance = 0.15) => _chance = chance;

        public IEnumerable<WalkCellOption> GetWalkOptions(Vector2Int pos)
        {
            if (!_vineCells.Contains(pos)) yield break;

            yield return new WalkCellOption
            {
                Title = "Walk here",
                ExtraEnergyCost = 0,
                OnEnter = character => RiskyWalk(character, pos)
            };

            yield return new WalkCellOption
            {
                Title = "Walk carefully",
                ExtraEnergyCost = 1,
                OnEnter = _ => CarefulWalk()
            };
        }

        private static IEnumerator RiskyWalk(Character character, Vector2Int pos)
        {
            if (!character.Inventory.HasItemOfType(ItemType.Antidote))
            {
                int seed = (int)character.OwnerClientId * 397 ^ pos.x * 31 ^ pos.y * 97 ^ character.Energy;
                if (new System.Random(seed).Next(0, 2) == 0)
                    yield return character.StartCoroutine(character.TakeDamageSequence(1));
            }
        }

        private static IEnumerator CarefulWalk() { yield break; }

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
