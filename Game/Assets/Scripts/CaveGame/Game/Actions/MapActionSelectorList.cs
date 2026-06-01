using System;
using System.Collections.Generic;
using CaveTogether.Generation;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Game.Actions
{
    public class MapActionSelectorList : MonoBehaviour
    {
        public Action<ActionRequest> RequestSelected;

        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _mapActionSelectorRowPrefab;
        [SerializeField] private Transform _rowContainer;
        [SerializeField] private LineRenderer _lineRenderer;

        private Vector3 _anchorWorldPos;
        private Camera _camera;
        private bool _isAnimating;

        private const float UpOffset = 3f;
        private const float ScreenPadding = 150f;
        private const float ReferenceOrthographicSize = 5f;
        private const float MinWorldY = 2f;
        private const float AnimDuration = 0.25f;

        public void Initialize(List<CellActionEntry> cellActionEntries, Vector3 anchorWorldPos)
        {
            var canvas = GetComponent<Canvas>();
            canvas.sortingLayerID = SortingLayer.NameToID("UI");
            canvas.sortingOrder = 1;

            _anchorWorldPos = anchorWorldPos;
            _camera = Camera.main;
            transform.rotation = _camera.transform.rotation;

            _closeButton.onClick.AddListener(() => FindAnyObjectByType<MapActionManager>().CloseActionSelector());

            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.SetPosition(0, _anchorWorldPos + _camera.transform.up * (MapRenderManager.CELL_SIZE * 0.5f));
            _lineRenderer.SetPosition(1, _anchorWorldPos);

            var (targetPos, targetScale) = ComputeTargetTransform();
            transform.position = _anchorWorldPos;
            transform.localScale = Vector3.zero;

            _isAnimating = true;
            DOTween.Sequence()
                .Join(transform.DOMove(targetPos, AnimDuration).SetEase(Ease.OutCubic))
                .Join(transform.DOScale(targetScale, AnimDuration).SetEase(Ease.OutBack))
                .OnComplete(() => _isAnimating = false);

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
            if (!_isAnimating)
            {
                var (pos, scale) = ComputeTargetTransform();
                transform.position = pos;
                transform.localScale = Vector3.one * scale;
            }

            Vector3 lineStart = _anchorWorldPos + _camera.transform.up * (MapRenderManager.CELL_SIZE * 0.5f);
            float halfHeight = GetComponent<RectTransform>().rect.height * transform.lossyScale.y * 0.5f;
            Vector3 lineEnd = transform.position - _camera.transform.up * halfHeight;
            _lineRenderer.SetPosition(0, lineStart);
            _lineRenderer.SetPosition(1, lineEnd);
        }

        private (Vector3 position, float scale) ComputeTargetTransform()
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

            return (pos, _camera.orthographicSize / ReferenceOrthographicSize);
        }

        private void OnRequestSelected(ActionRequest request)
        {
            RequestSelected?.Invoke(request);
        }
    }
}