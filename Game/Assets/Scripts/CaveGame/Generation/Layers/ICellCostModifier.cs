using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public interface ICellCostModifier
    {
        int GetEntryCostModifier(Vector2Int cellPos);
    }
}
