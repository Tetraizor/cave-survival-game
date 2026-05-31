using System.Collections;
using System.Collections.Generic;
using CaveTogether.Game.Entities;
using UnityEngine;

namespace CaveTogether.Items
{
    public class MedKit : ItemBase
    {
        private const string HealAnimation = "Patch";
        private const float HealAnimationDuration = 2f;
        private const float ReturnDelay = 0.5f;

        public override ItemType Id => ItemType.MedKit;
        public override string DisplayName => "Med Kit";
        public override bool IsConsumable => false;

        public override IEnumerable<ItemUseOption> GetUseOptions(Character user, CharacterManager cm)
        {
            yield return new ItemUseOption
            {
                DisplayName = "Heal Self",
                IsAvailable = !user.IsDown && user.Health < user.MaxHealth,
                EnergyCost = 2
            };
            yield return new ItemUseOption
            {
                DisplayName = "Heal Friend",
                IsAvailable = cm.Characters.Exists(c => c != user && !c.IsDown && c.Health < c.MaxHealth && c.GridPosition == user.GridPosition),
                EnergyCost = 1
            };
        }

        public override IEnumerator Use(int optionIndex, Character user, CharacterManager cm, ulong targetCharacterId)
        {
            Character target;

            if (optionIndex == 0)
            {
                target = user;
            }
            else
            {
                target = cm.GetCharacter(targetCharacterId);
                if (target == null || target == user || target.IsDown || target.GridPosition != user.GridPosition)
                    target = cm.Characters.Find(c => c != user && !c.IsDown && c.GridPosition == user.GridPosition);
                if (target == null) yield break;
            }

            yield return user.StartCoroutine(HealSequence(user, target));
        }

        private IEnumerator HealSequence(Character user, Character target)
        {
            bool isSelf = target == user;

            if (!isSelf)
                yield return user.StartCoroutine(user.WalkTo(target.transform.position));

            user.PlayAnimationTrigger(HealAnimation);
            yield return new WaitForSeconds(HealAnimationDuration);

            target.Heal(1);

            yield return new WaitForSeconds(ReturnDelay);

            if (!isSelf)
                yield return user.StartCoroutine(user.WalkToGridPosition());
        }
    }
}
