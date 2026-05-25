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

        private Vector3 _anchorWorldPos;
        private Camera _camera;

        private const float UpOffset = 3f;
        private const float ScreenPadding = 150f;
        private const float ReferenceOrthographicSize = 5f;
        private const float MinWorldY = 2f;

        public void Initialize(List<CellActionEntry> cellActionEntries, Vector3 anchorWorldPos)
        {
            _anchorWorldPos = anchorWorldPos;
            _camera = Camera.main;
            transform.rotation = _camera.transform.rotation;
            UpdatePosition();

            foreach (var entry in cellActionEntries)
            {
                var mapActionSelectorRowGO = Instantiate(_mapActionSelectorRowPrefab, _rowContainer);
                var mapActionSelector = mapActionSelectorRowGO.GetComponent<MapActionSelectorRow>();
                mapActionSelector.Initialize(entry);
                mapActionSelector.RequestSelected += OnRequestSelected;
            }
        }

        private void LateUpdate() => UpdatePosition();

        private void UpdatePosition()
        {
            Vector3 idealWorldPos = _anchorWorldPos + _camera.transform.up * UpOffset;

            Vector3 screenPos = _camera.WorldToScreenPoint(idealWorldPos);
            screenPos.x = Mathf.Clamp(screenPos.x, ScreenPadding, Screen.width - ScreenPadding);
            screenPos.y = Mathf.Clamp(screenPos.y, ScreenPadding, Screen.height - ScreenPadding);

            var pos = _camera.ScreenToWorldPoint(screenPos);

            if (pos.y < MinWorldY)
            {
                float yDeficit = MinWorldY - pos.y;
                pos -= _camera.transform.forward * (yDeficit / -_camera.transform.forward.y);
            }

            transform.position = pos;
            transform.localScale = Vector3.one * (_camera.orthographicSize / ReferenceOrthographicSize);
        }

        private void OnRequestSelected(ActionRequest request)
        {
            RequestSelected?.Invoke(request);
        }
    }
}