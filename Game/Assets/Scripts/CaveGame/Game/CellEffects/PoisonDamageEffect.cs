using System.Collections;
using CaveTogether.Game.Entities;
using CaveTogether.Items;

namespace CaveTogether.Game.CellEffects
{
    public class PoisonDamageEffect : ICellEffect
    {
        private readonly int _amount;
        public PoisonDamageEffect(int amount) => _amount = amount;

        public IEnumerator Apply(Character character)
        {
            if (!character.Inventory.HasItemOfType(ItemType.Antidote))
                yield return character.TakeDamageSequence(_amount);
        }
    }
}
