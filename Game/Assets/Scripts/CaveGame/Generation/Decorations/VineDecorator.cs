using System.Linq;
using CaveTogether.Generation.Layers;
using UnityEngine;

namespace CaveTogether.Generation.Decorations
{
    public class VineDecorator : MonoBehaviour, ICellDecorator
    {
        [SerializeField] private GameObject _vinePrefab;

        public void DecorateCell(Vector2Int pos, MapData map, Transform parent)
        {
            var mapManager = FindAnyObjectByType<MapManager>();
            if (mapManager == null) return;

            var vineLayer = mapManager.Generator.GetLayer<VineGenerationLayer>();
            if (vineLayer == null || !vineLayer.VineCells.Contains(pos)) return;

            var vine = Instantiate(_vinePrefab, parent);
            vine.transform.position = FindAnyObjectByType<MapRenderManager>().GridToWorldPosition(pos);
        }
    }
}
