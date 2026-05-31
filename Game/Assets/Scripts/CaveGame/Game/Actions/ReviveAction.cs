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

        private const float PatchDuration = 4f;
        private const float ReturnDelay = 1f;

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            var characterManager = Object.FindAnyObjectByType<CharacterManager>();
            var target = characterManager.Characters.FirstOrDefault(
                c => c != character && c.IsDown && c.GridPosition == request.TargetCell);

            if (target == null) yield break;

            yield return character.StartCoroutine(character.WalkTo(target.transform.position));
            character.PlayAnimationTrigger("Patch");
            yield return new WaitForSeconds(PatchDuration);
            target.Heal(1);
            yield return new WaitForSeconds(ReturnDelay);
            yield return character.StartCoroutine(character.WalkToGridPosition());
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
