using System;
using CaveTogether.Generation;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game
{
    public class CursorManager : MonoBehaviour
    {
        public Action<Vector2Int> CellClicked;

        public Action<Vector2Int> CellHoverEnter;
        public Action<Vector2Int> CellHoverExit;

        private MapData _map;
        private readonly Plane _selectionPlane = new(Vector3.up, 0f);
        private Vector2Int _cellPosition;

        private ExplorationManager _explorationManager;
        private InputService _inputService;

        public Vector2Int? CellPosition => IsOnMap ? _cellPosition : null;
        public bool IsOnMap { get; private set; }
        public bool IsOnVisibleCell =>
            CellPosition.HasValue &&
            !_map.GetCellRef(CellPosition.Value).IsEmpty &&
            (_explorationManager == null ||
             _explorationManager.IsExplored(CellPosition.Value) ||
             _explorationManager.IsFrontier(CellPosition.Value));

        public void Initialize(MapData map)
        {
            _map = map;
            _explorationManager = FindAnyObjectByType<ExplorationManager>();
            _inputService = ServiceLocator.Get<InputService>();
        }

        public void Deinitialize() { }

        private void Update()
        {
            if (_map == null) return;

            UpdateCellPosition();
            CheckForClicks();
        }

        private void CheckForClicks()
        {
            if (!IsOnVisibleCell) return;
            if (!_inputService.IsCellSelectedThisFrame) return;
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            CellClicked?.Invoke(_cellPosition);
        }

        private void UpdateCellPosition()
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                if (IsOnMap)
                {
                    IsOnMap = false;
                    CellHoverExit?.Invoke(_cellPosition);
                }
                return;
            }

            var ray = Camera.main.ScreenPointToRay(_inputService.PointerScreenPosition);

            if (!_selectionPlane.Raycast(ray, out float distance))
            {
                IsOnMap = false;
                return;
            }

            var worldPoint = ray.GetPoint(distance);
            var candidate = new Vector2Int(
                Mathf.FloorToInt(worldPoint.x / MapRenderManager.CELL_SIZE),
                Mathf.FloorToInt(worldPoint.z / MapRenderManager.CELL_SIZE)
            );

            var oldCellPosition = _cellPosition;

            IsOnMap = _map.IsInsideBounds(candidate);
            if (IsOnMap) _cellPosition = candidate;

            if (oldCellPosition != _cellPosition)
            {
                CellHoverEnter?.Invoke(_cellPosition);
                CellHoverExit?.Invoke(oldCellPosition);
            }
        }


    }
}