using CaveGame.Common.Enums;
using CaveGame.Game.Entities;
using CaveGame.Game.Movement;
using CaveGame.Generation;
using UnityEngine;

namespace CaveGame.Game.Actions
{
    public class WalkAction : GameActionBase
    {
        public override ActionType Type => ActionType.Walk;
        public override ActionUIType UIType => ActionUIType.ContextualCell;

        public override string DisplayName => "Walk here";

        public override void Execute(MapData map, Character character, ActionRequest request)
        {
            character.SetPosition(request.TargetCell);
        }

        public override int GetEnergyCost(MapData map, Character character, Vector2Int targetCell) => MovementValidator.GetDistance(map, character.GridPosition, targetCell);

        public override bool IsValid(MapData map, Character character, Vector2Int targetCell)
        {
            if (targetCell.x == character.GridPosition.x && targetCell.y == character.GridPosition.y) return false;
            if (!MovementValidator.CanMove(map, character.GridPosition, targetCell, out int distance)) return false;

            return true;
        }
    }
}