using System.Collections.Generic;
using CaveTogether.Generation.Layers;
using CaveTogether.Items;
using CaveTogether.Services;
using DG.Tweening;
using UnityEngine;

namespace CaveTogether.Generation.Decorations
{
    public class LootDecorator : MonoBehaviour, ICellDecorator
    {
        [SerializeField] private GameObject _lootPrefab;

        private LootGenerationLayer _lootLayer;
        private readonly Dictionary<Vector2Int, GameObject> _lootObjects = new();

        private LootGenerationLayer GetOrInitLayer()
        {
            if (_lootLayer != null) return _lootLayer;
            _lootLayer = FindAnyObjectByType<MapManager>()?.Generator.GetLayer<LootGenerationLayer>();
            if (_lootLayer != null)
                _lootLayer.LootPickedUp += OnLootPickedUp;
            return _lootLayer;
        }

        private void OnDestroy()
        {
            if (_lootLayer != null)
                _lootLayer.LootPickedUp -= OnLootPickedUp;
        }

        public void DecorateCell(Vector2Int pos, MapData map, Transform parent)
        {
            var layer = GetOrInitLayer();
            if (layer == null || !layer.LootCells.ContainsKey(pos)) return;

            var itemType = layer.LootCells[pos];
            var itemData = ServiceLocator.Get<ItemDatabaseService>().GetSO(itemType);
            var sprite = itemData.Icon;

            var lootGO = Instantiate(_lootPrefab, parent);
            lootGO.transform.position = FindAnyObjectByType<MapRenderManager>().GridToWorldPosition(pos);
            lootGO.transform.Find("ItemRenderer").GetComponent<SpriteRenderer>().sprite = sprite;

            _lootObjects[pos] = lootGO;
        }

        private void OnLootPickedUp(Vector2Int pos)
        {
            if (_lootObjects.TryGetValue(pos, out var go))
            {
                go.transform.DOKill();
                go.transform.DOScale(Vector3.zero, .1f).OnComplete(() => { Destroy(go); });
                _lootObjects.Remove(pos);
            }
        }
    }
}
