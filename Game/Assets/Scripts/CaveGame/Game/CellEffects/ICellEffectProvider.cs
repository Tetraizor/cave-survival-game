using System.Collections.Generic;
using UnityEngine;

namespace CaveTogether.Game.CellEffects
{
    public interface ICellEffectProvider
    {
        IEnumerable<CellEffect> GetEffectsForCell(Vector2Int pos);
    }
}
