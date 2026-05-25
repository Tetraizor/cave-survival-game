using System.Collections;
using System.Linq;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class ReviveAction : GameActionBase
    {
        public override ActionType Type => ActionType.Revive;
        public override ActionUIType UIType => ActionUIType.ContextualCell;
        public override string DisplayName => "Revive";

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            var characterManager = Object.FindAnyObjectByType<CharacterManager>();
            var target = characterManager.Characters.FirstOrDefault(
                c => c != character && c.IsDown && c.GridPosition == request.TargetCell);
            if (target != null) target.Heal(1);

            yield break;
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => 2;

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            if (character.IsDown) return false;
            if (request.TargetCell != character.GridPosition) return false;

            var characterManager = Object.FindAnyObjectByType<CharacterManager>();

            return characterManager.Characters.Any(
                c => c != character && c.IsDown && c.GridPosition == request.TargetCell);
        }
    }
}
