using UnityEngine;

namespace CaveTogether.Generation.Decorations
{
    public interface ICellDecorator
    {
        public void DecorateCell(Vector2Int pos, MapData map, Transform parent);
    }
}