using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Movement;
using CaveTogether.Generation;

namespace CaveTogether.Game.Actions
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

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => MovementValidator.GetDistance(map, character.GridPosition, request.TargetCell);

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            if (request.TargetCell.x == character.GridPosition.x && request.TargetCell.y == character.GridPosition.y) return false;
            if (!MovementValidator.CanMove(map, character.GridPosition, request.TargetCell, out int _)) return false;

            return true;
        }
    }
}