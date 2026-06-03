using System.Collections;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using CaveTogether.Generation.Features;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class EscapeAction : GameActionBase
    {
        public override ActionType Type => ActionType.Escape;
        public override ActionUIType UIType => ActionUIType.ContextualCell;
        public override string DisplayName => "Escape";

        private static readonly WaitForSeconds EscapeWait = new(8f);

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            character.PlayAnimationTrigger("Escape");
            yield return EscapeWait;
            character.Escape();
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request)
            => character.Energy;

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            if (character.IsDown || character.IsEscaped) return false;
            var exit = map.GetFeature<ExitFeature>();
            return exit != null
                && character.GridPosition == exit.ExitPosition
                && request.TargetCell == exit.ExitPosition;
        }
    }
}
