using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Movement;
using CaveTogether.Generation;

namespace CaveTogether.Game.Actions
{
    public class WaitAction : GameActionBase
    {
        public override ActionType Type => ActionType.Walk;
        public override ActionUIType UIType => ActionUIType.ContextualCell;

        public override string DisplayName => "Wait";

        public override void Execute(MapData map, Character character, ActionRequest request)
        {
            character.SetPosition(request.TargetCell);
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => character.Energy;

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            return request.TargetCell.x == character.GridPosition.x && request.TargetCell.y == character.GridPosition.y;
        }
    }
}