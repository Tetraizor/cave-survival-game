using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class EndTurnAction : GameActionBase
    {
        public override ActionType Type => ActionType.EndTurn;
        public override ActionUIType UIType => ActionUIType.ActionBarGlobal;
        public override string DisplayName => "End Turn";

        public override int GetEnergyCost(MapData map, Character character, Vector2Int targetCell) => 0;
        public override bool IsValid(MapData map, Character character, Vector2Int targetCell) => true;

        public override void Execute(MapData map, Character character, ActionRequest request)
        {
            // TODO: Set energy to 0!
        }
    }
}