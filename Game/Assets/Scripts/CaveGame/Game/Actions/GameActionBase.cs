using System.Collections;
using System.Collections.Generic;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public abstract class GameActionBase
    {
        public abstract ActionType Type { get; }
        public abstract ActionUIType UIType { get; }
        public abstract string DisplayName { get; }

        public abstract int GetEnergyCost(MapData map, Character character, ActionRequest request);

        public abstract bool IsValid(MapData map, Character character, ActionRequest request);
        public abstract IEnumerator Execute(MapData map, Character character, ActionRequest request);

        public virtual IEnumerable<CellActionEntry> GetEntries(MapData map, Character character, int x, int y)
        {
            if (UIType != ActionUIType.ContextualCell) yield break;
            var pos = new Vector2Int(x, y);
            var request = new ActionRequest { Type = Type, TargetCell = pos };
            int cost = GetEnergyCost(map, character, request);
            if (IsValid(map, character, request) && cost <= character.Energy)
                yield return new CellActionEntry { Type = Type, Title = DisplayName, EnergyCost = cost, Position = pos, Request = request };
        }
    }
}