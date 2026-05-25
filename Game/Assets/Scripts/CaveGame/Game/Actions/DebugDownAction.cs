using System.Collections;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;

namespace CaveTogether.Game.Actions
{
    public class DebugDownAction : GameActionBase
    {
        public override ActionType Type => ActionType.DebugDown;
        public override ActionUIType UIType => ActionUIType.ContextualCell;
        public override string DisplayName => "[Debug] Down Self";

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            character.TakeDamage(character.Health);
            yield break;
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => 0;

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            return !character.IsDown && request.TargetCell == character.GridPosition;
        }
    }
}
