using System.Collections;
using System.Collections.Generic;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using CaveTogether.Generation.Layers;
using CaveTogether.Items;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class PickupAction : GameActionBase
    {
        private static readonly WaitForSeconds PickupWait = new(0.8f);

        public override ActionType Type => ActionType.Pickup;
        public override ActionUIType UIType => ActionUIType.ContextualCell;
        public override string DisplayName => "Pick Up";

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => 0;

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            if (request.TargetCell != character.GridPosition) return false;

            var layer = Object.FindAnyObjectByType<MapManager>()?.Generator.GetLayer<LootGenerationLayer>();
            if (layer == null || !layer.LootCells.TryGetValue(request.TargetCell, out var itemType)) return false;

            var item = ServiceLocator.Get<ItemDatabaseService>().CreateItem(itemType);
            return CanPickup(character, itemType, item);
        }

        private static bool CanPickup(Character character, ItemType itemType, ItemBase item)
        {
            if (character.Inventory.IsFull) return false;
            if (item != null && !item.IsConsumable && character.Inventory.HasItemOfType(itemType)) return false;
            return true;
        }

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            var layer = Object.FindAnyObjectByType<MapManager>()?.Generator.GetLayer<LootGenerationLayer>();
            if (layer == null || !layer.LootCells.ContainsKey(request.TargetCell)) yield break;

            character.PlayAnimationTrigger("Pickup");
            yield return PickupWait;

            character.Inventory.AddItem(layer.LootCells[request.TargetCell]);
            layer.RemoveLoot(request.TargetCell);
        }

        public override IEnumerable<CellActionEntry> GetEntries(MapData map, Character character, int x, int y)
        {
            var pos = new Vector2Int(x, y);
            if (pos != character.GridPosition) yield break;

            var layer = Object.FindAnyObjectByType<MapManager>()?.Generator.GetLayer<LootGenerationLayer>();
            if (layer == null || !layer.LootCells.TryGetValue(pos, out var itemType)) yield break;

            var item = ServiceLocator.Get<ItemDatabaseService>().CreateItem(itemType);
            if (!CanPickup(character, itemType, item)) yield break;
            string title = item != null ? $"Pick Up ({item.DisplayName})" : "Pick Up";

            yield return new CellActionEntry
            {
                Type = ActionType.Pickup,
                Title = title,
                EnergyCost = 0,
                Position = pos,
                Request = new ActionRequest { Type = ActionType.Pickup, TargetCell = pos }
            };
        }
    }
}
