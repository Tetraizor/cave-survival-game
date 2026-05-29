using System;
using System.Collections;
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

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            character.LookAt(request.TargetCell);
            var explorationManager = UnityEngine.Object.FindAnyObjectByType<ExplorationManager>();

            yield return character.StartCoroutine(
                character.InspectCell(() => explorationManager.RevealFromPosition(request.TargetCell))
            );
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => 1;

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            bool isExplored = UnityEngine.Object.FindAnyObjectByType<ExplorationManager>().IsExplored(request.TargetCell);
            return !isExplored && MovementValidator.CanMove(map, character.GridPosition, request.TargetCell, out int distance) && distance == 1;
        }
    }
}