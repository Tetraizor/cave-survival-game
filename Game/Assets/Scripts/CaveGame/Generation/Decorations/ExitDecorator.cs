using CaveTogether.Generation.Features;
using UnityEngine;

namespace CaveTogether.Generation.Decorations
{
    public class ExitDecorator : MonoBehaviour, ICellDecorator
    {
        [SerializeField] private GameObject _exitDecorationPrefab;

        public void DecorateCell(Vector2Int pos, MapData map, Transform parent)
        {
            if (pos != map.GetFeature<ExitFeature>().ExitPosition) return;

            var exitDecoration = Instantiate(_exitDecorationPrefab, parent);
            exitDecoration.transform.position = FindAnyObjectByType<MapRenderManager>().GridToWorldPosition(pos);
        }
    }
}