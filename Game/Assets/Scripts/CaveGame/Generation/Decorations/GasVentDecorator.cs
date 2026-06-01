using System.Collections.Generic;
using System.Linq;
using CaveTogether.Game;
using CaveTogether.Generation.Layers;
using DG.Tweening;
using UnityEngine;

namespace CaveTogether.Generation.Decorations
{
    public class GasVentDecorator : MonoBehaviour, ICellDecorator
    {
        [SerializeField] private GameObject _ventPrefab;
        [SerializeField] private GameObject _gasPrefab;

        private GasVentGenerationLayer _layer;
        private readonly Dictionary<Vector2Int, GameObject> _gasObjects = new();
        private MapRenderManager _renderManager;
        private ExplorationManager _explorationManager;

        private GasVentGenerationLayer GetOrInitLayer()
        {
            if (_layer != null) return _layer;
            _layer = FindAnyObjectByType<MapManager>()?.Generator.GetLayer<GasVentGenerationLayer>();
            if (_layer != null)
                _layer.GetEvent().GasCellsChanged += OnGasCellsChanged;
            _renderManager = FindAnyObjectByType<MapRenderManager>();
            _explorationManager = FindAnyObjectByType<ExplorationManager>();
            if (_explorationManager != null)
                _explorationManager.CellRevealed += OnCellRevealed;
            return _layer;
        }

        private void OnDestroy()
        {
            if (_layer != null)
                _layer.GetEvent().GasCellsChanged -= OnGasCellsChanged;
            if (_explorationManager != null)
                _explorationManager.CellRevealed -= OnCellRevealed;
        }

        public void DecorateCell(Vector2Int pos, MapData map, Transform parent)
        {
            var layer = GetOrInitLayer();
            if (layer == null || !layer.VentCells.Contains(pos)) return;

            var ventGO = Instantiate(_ventPrefab, parent);
            ventGO.transform.position = _renderManager.GridToWorldPosition(pos);
        }

        private void OnCellRevealed(Vector2Int pos)
        {
            var layer = GetOrInitLayer();
            if (layer == null) return;
            if (_gasObjects.ContainsKey(pos)) return;
            if (!layer.GetEvent().IsGasCell(pos)) return;

            var gasGO = Instantiate(_gasPrefab, transform);
            gasGO.transform.position = _renderManager.GridToWorldPosition(pos);
            gasGO.transform.localScale = Vector3.zero;
            gasGO.transform.DOScale(Vector3.one, 0.3f);
            _gasObjects[pos] = gasGO;
        }

        private void OnGasCellsChanged()
        {
            var layer = GetOrInitLayer();
            if (layer == null) return;

            var activeGasCells = layer.GetEvent().ActiveGasCells;

            var toRemove = new List<Vector2Int>();
            foreach (var kvp in _gasObjects)
            {
                if (!activeGasCells.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                    var go = kvp.Value;
                    go.transform.DOKill();
                    go.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => Destroy(go));
                }
            }
            foreach (var pos in toRemove) _gasObjects.Remove(pos);

            foreach (var pos in activeGasCells)
            {
                if (_gasObjects.ContainsKey(pos)) continue;
                if (_explorationManager != null && !_explorationManager.IsExplored(pos)) continue;
                var gasGO = Instantiate(_gasPrefab, transform);
                gasGO.transform.position = _renderManager.GridToWorldPosition(pos);
                gasGO.transform.localScale = Vector3.zero;
                gasGO.transform.DOScale(Vector3.one, 0.3f);
                _gasObjects[pos] = gasGO;
            }
        }
    }
}
