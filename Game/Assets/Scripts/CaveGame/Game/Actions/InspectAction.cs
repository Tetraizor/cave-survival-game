using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Movement;
using CaveTogether.Generation;

namespace CaveTogether.Game.Actions
{
    public class InspectAction : GameActionBase
    {
        public override ActionType Type => ActionType.Inspect;
        public override ActionUIType UIType => ActionUIType.ContextualCell;

        public override string DisplayName => "Inspect here";

        public override void Execute(MapData map, Character character, ActionRequest request)
        {
            character.SetPosition(request.TargetCell);
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => 1;

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            return MovementValidator.CanMove(map, character.GridPosition, request.TargetCell, out int distance) && distance == 1;
        }
    }
}