using System;
using System.Collections.Generic;
using CaveTogether.Generation.Features;
using CaveTogether.Items;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public class LootGenerationLayer : MapGenerationLayerBase
    {
        private const string LootTablePath = "Data/LootTable";
        private const double SpawnChance = 0.15;

        public event Action<Vector2Int> LootPickedUp;

        private readonly Dictionary<Vector2Int, ItemType> _lootCells = new();
        private readonly LootTable _lootTable;

        public IReadOnlyDictionary<Vector2Int, ItemType> LootCells => _lootCells;

        public LootGenerationLayer()
        {
            _lootTable = Resources.Load<LootTable>(LootTablePath);
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
                    if (random.NextDouble() >= SpawnChance) continue;

                    var item = _lootTable.Roll(random);
                    if (item != ItemType.None)
                        _lootCells[pos] = item;
                }
            }
        }

        public void RemoveLoot(Vector2Int pos)
        {
            if (_lootCells.Remove(pos))
                LootPickedUp?.Invoke(pos);
        }
    }
}
