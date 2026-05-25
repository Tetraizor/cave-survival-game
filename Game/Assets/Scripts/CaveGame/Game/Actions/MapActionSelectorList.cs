using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class MapActionSelectorList : MonoBehaviour
    {
        public Action<ActionRequest> RequestSelected;

        [SerializeField] private GameObject _mapActionSelectorRowPrefab;
        [SerializeField] private Transform _rowContainer;

        public void Initialize(List<CellActionEntry> cellActionEntries)
        {
            foreach (var entry in cellActionEntries)
            {
                var mapActionSelectorRowGO = Instantiate(_mapActionSelectorRowPrefab, _rowContainer);
                var mapActionSelector = mapActionSelectorRowGO.GetComponent<MapActionSelectorRow>();
                mapActionSelector.Initialize(entry);
                mapActionSelector.RequestSelected += OnRequestSelected;
            }
        }

        private void OnRequestSelected(ActionRequest request)
        {
            RequestSelected?.Invoke(request);
        }
    }
}