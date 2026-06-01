using System.Collections.Generic;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public interface ICellWalkOptionsProvider
    {
        IEnumerable<WalkCellOption> GetWalkOptions(Vector2Int pos);
    }
}
