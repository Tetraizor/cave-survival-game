using CaveGame.Generation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CaveGame.Game
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private GameObject _cellHighlightRenderer;

        private MapData _map;
        private readonly Plane _selectionPlane = new(Vector3.up, 0f);

        private float _cursorYOffset = .03f;

        private Vector2Int _cellPosition;

        public Vector2Int? CellPosition => IsOnMap ? _cellPosition : null;
        public bool IsOnMap { get; private set; }
        public bool IsOnVisibleCell => CellPosition.HasValue && !_map.GetCellRef(CellPosition.Value).IsEmpty; // TODO: fog of war

        public void Initialize(MapData map)
        {
            _map = map;
            _cellHighlightRenderer.SetActive(false);
        }

        public void Deinitialize()
        {
            _cellHighlightRenderer.SetActive(false);
        }

        private void Update()
        {
            if (_map == null) return;

            UpdateCellPosition();
            UpdateHighlight();
        }

        private void UpdateCellPosition()
        {
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

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

            IsOnMap = _map.IsInsideBounds(candidate);
            if (IsOnMap) _cellPosition = candidate;
        }

        private void UpdateHighlight()
        {
            _cellHighlightRenderer.SetActive(IsOnVisibleCell);
            if (!IsOnVisibleCell) return;

            var targetPosition = new Vector3(
                _cellPosition.x * MapRenderManager.CELL_SIZE + MapRenderManager.CELL_SIZE / 2f,
                _cursorYOffset,
                _cellPosition.y * MapRenderManager.CELL_SIZE + MapRenderManager.CELL_SIZE / 2f
            );

            _cellHighlightRenderer.transform.position = targetPosition;
        }
    }
}