using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Game.Entities;

namespace CaveTogether.Items
{
    public class Antidote : ItemBase
    {
        public override ItemType Id => ItemType.Antidote;
        public override string DisplayName => "Antidote";
        public override bool IsConsumable => false;

        public override IEnumerable<ItemUseOption> GetUseOptions(Character user, CharacterManager cm) => Enumerable.Empty<ItemUseOption>();
        public override IEnumerator Use(int optionIndex, Character user, CharacterManager cm, ulong targetCharacterId) { yield break; }
    }
}