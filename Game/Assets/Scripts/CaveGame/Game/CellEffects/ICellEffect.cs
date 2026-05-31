using System.Collections;
using CaveTogether.Game.Entities;

namespace CaveTogether.Game.CellEffects
{
    public interface ICellEffect
    {
        IEnumerator Apply(Character character);
    }
}
