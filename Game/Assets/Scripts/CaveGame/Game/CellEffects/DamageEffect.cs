using System.Collections;
using CaveTogether.Game.Entities;

namespace CaveTogether.Game.CellEffects
{
    public class DamageEffect : ICellEffect
    {
        private readonly int _amount;
        public DamageEffect(int amount) => _amount = amount;

        public IEnumerator Apply(Character character) =>
            character.TakeDamageSequence(_amount);
    }
}
