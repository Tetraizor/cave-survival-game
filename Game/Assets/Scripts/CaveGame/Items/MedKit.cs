using System.Collections;
using System.Collections.Generic;
using CaveTogether.Game.Entities;

namespace CaveTogether.Items
{
    public class MedKit : ItemBase
    {
        public override ItemType Id => ItemType.MedKit;
        public override string DisplayName => "Med Kit";

        public override IEnumerable<ItemUseOption> GetUseOptions(Character user, CharacterManager cm)
        {
            yield return new ItemUseOption
            {
                DisplayName = "Heal Self",
                IsAvailable = !user.IsDown && user.Health < user.MaxHealth
            };
            yield return new ItemUseOption
            {
                DisplayName = "Heal Friend",
                IsAvailable = cm.Characters.Exists(c => c != user && !c.IsDown && c.GridPosition == user.GridPosition)
            };
        }

        public override IEnumerator Use(int optionIndex, Character user, CharacterManager cm, ulong targetCharacterId)
        {
            if (optionIndex == 0)
            {
                user.Heal(1);
            }
            else if (optionIndex == 1)
            {
                var target = cm.GetCharacter(targetCharacterId);

                if (target == null || target == user || target.IsDown || target.GridPosition != user.GridPosition)
                    target = cm.Characters.Find(c => c != user && !c.IsDown && c.GridPosition == user.GridPosition);

                target?.Heal(1);
            }
            yield break;
        }
    }
}
