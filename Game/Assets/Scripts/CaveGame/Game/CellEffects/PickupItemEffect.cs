using System.Collections;
using CaveTogether.Game.Entities;
using CaveTogether.Generation.Layers;
using UnityEngine;

namespace CaveTogether.Game.CellEffects
{
    public class PickupItemEffect : ICellEffect
    {
        private readonly LootGenerationLayer _layer;
        private readonly Vector2Int _pos;

        public PickupItemEffect(LootGenerationLayer layer, Vector2Int pos)
        {
            _layer = layer;
            _pos = pos;
        }

        public IEnumerator Apply(Character character)
        {
            if (!_layer.LootCells.ContainsKey(_pos)) yield break;

            character.Inventory.AddItem(_layer.LootCells[_pos]);
            _layer.RemoveLoot(_pos);
        }
    }
}
