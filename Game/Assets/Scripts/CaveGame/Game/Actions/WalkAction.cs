using System.Collections;
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

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            var path = MapPathFinder.GetPath(map, character.GridPosition, request.TargetCell);
            if (path == null) { character.SetPosition(request.TargetCell); yield break; }

            for (int i = 1; i < path.Length; i++)
                yield return character.StartCoroutine(character.MoveToCell(path[i]));
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