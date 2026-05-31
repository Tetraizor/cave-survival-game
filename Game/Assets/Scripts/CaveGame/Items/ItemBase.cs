using System.Collections;
using System.Collections.Generic;
using CaveTogether.Game.Entities;

namespace CaveTogether.Items
{
    public abstract class ItemBase
    {
        public abstract ItemType Id { get; }
        public abstract string DisplayName { get; }
        public virtual bool IsConsumable => true;

        public abstract IEnumerable<ItemUseOption> GetUseOptions(Character user, CharacterManager cm);

        public abstract IEnumerator Use(int optionIndex, Character user, CharacterManager cm, ulong targetCharacterId);
    }
}