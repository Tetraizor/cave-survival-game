using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using CaveTogether.Items;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class UseItemAction : GameActionBase
    {
        public override ActionType Type => ActionType.UseItem;
        public override ActionUIType UIType => ActionUIType.ContextualCell;
        public override string DisplayName => "Use Item";

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request)
        {
            var inventory = character.Inventory;
            if (inventory == null || !inventory.HasItem(request.ItemSlot)) return 1;

            var item = ServiceLocator.Get<ItemDatabaseService>().CreateItem(inventory.GetItemType(request.ItemSlot));
            if (item == null) return 1;

            var cm = Object.FindAnyObjectByType<CharacterManager>();
            var options = item.GetUseOptions(character, cm).ToList();

            return request.ItemActionIndex < options.Count ? options[request.ItemActionIndex].EnergyCost : 1;
        }

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            var inventory = character.Inventory;
            if (inventory == null || !inventory.HasItem(request.ItemSlot)) return false;

            var item = ServiceLocator.Get<ItemDatabaseService>().CreateItem(inventory.GetItemType(request.ItemSlot));
            if (item == null) return false;

            var cm = Object.FindAnyObjectByType<CharacterManager>();
            var options = item.GetUseOptions(character, cm).ToList();
            return request.ItemActionIndex < options.Count && options[request.ItemActionIndex].IsAvailable;
        }

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            var inventory = character.Inventory;
            var item = ServiceLocator.Get<ItemDatabaseService>().CreateItem(inventory.GetItemType(request.ItemSlot));
            var cm = Object.FindAnyObjectByType<CharacterManager>();

            yield return character.StartCoroutine(
                item.Use(request.ItemActionIndex, character, cm, request.TargetCharacterId));

            if (item.IsConsumable)
                inventory.RemoveItem(request.ItemSlot);
        }

        public override IEnumerable<CellActionEntry> GetEntries(MapData map, Character character, int x, int y)
        {
            var cell = new Vector2Int(x, y);
            if (cell != character.GridPosition) yield break;

            var db = ServiceLocator.Get<ItemDatabaseService>();
            var cm = Object.FindAnyObjectByType<CharacterManager>();

            for (byte slot = 0; slot < Inventory.Size; slot++)
            {
                if (!character.Inventory.HasItem(slot)) continue;
                var item = db.CreateItem(character.Inventory.GetItemType(slot));
                if (item == null) continue;

                byte optionIdx = 0;
                foreach (var option in item.GetUseOptions(character, cm))
                {
                    if (option.IsAvailable && option.EnergyCost <= character.Energy)
                    {
                        var request = new ActionRequest
                        {
                            Type = ActionType.UseItem,
                            TargetCell = cell,
                            ItemSlot = slot,
                            ItemActionIndex = optionIdx
                        };
                        yield return new CellActionEntry
                        {
                            Type = ActionType.UseItem,
                            Title = option.DisplayName,
                            EnergyCost = option.EnergyCost,
                            Position = cell,
                            Request = request
                        };
                    }
                    optionIdx++;
                }
            }
        }
    }
}
