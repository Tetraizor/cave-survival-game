using CaveTogether.Services;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CaveTogether.Game
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 100;
        [SerializeField] private float _smoothTime = .15f;
        private Vector2 _moveInput;
        private Vector3 _currentVelocity;

        [Header("Edge Pan")]
        [SerializeField] private float _edgePanThreshold = 50f;

        [Header("Middle Mouse Pan")]
        [SerializeField] private float _middleMousePanSpeed = 1f;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 1.5f;
        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 15f;
        private float _zoomInput;
        private float _targetZoom;

        private Camera _camera;
        private float _defaultZoom;
        private bool _isFocusing;

        public void Initialize()
        {
            _camera = GetComponent<Camera>();
            _defaultZoom = _camera.orthographicSize;
            _targetZoom = _camera.orthographicSize;

            var inputService = ServiceLocator.Get<InputService>();
            inputService.CameraMoveInputChanged += OnCameraMoveInputChanged;
            inputService.CameraZoomInputChanged += OnCameraZoomInputChanged;
        }

        public void Deinitialize()
        {
            var inputService = ServiceLocator.Get<InputService>();
            inputService.CameraMoveInputChanged -= OnCameraMoveInputChanged;
            inputService.CameraZoomInputChanged -= OnCameraZoomInputChanged;
        }

        private void OnCameraZoomInputChanged(float input) => _zoomInput = input;
        private void OnCameraMoveInputChanged(Vector2 input) => _moveInput = input;

        private void LateUpdate()
        {
            if (_camera == null) return;

            MoveCamera();
            ZoomCamera();
        }

        public void FocusOn(Vector3 target)
        {
            float t = (target.y - transform.position.y) / transform.forward.y;
            Vector3 screenCenter = transform.position + transform.forward * t;
            Vector3 destination = transform.position + new Vector3(target.x - screenCenter.x, 0, target.z - screenCenter.z);

            _isFocusing = true;
            _targetZoom = _defaultZoom;
            _currentVelocity = Vector3.zero;
            transform.DOMove(destination, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => _isFocusing = false);
        }

        private void MoveCamera()
        {
            if (_isFocusing) return;

            if (Mouse.current.middleButton.isPressed)
            {
                float worldUnitsPerPixel = (2f * _camera.orthographicSize) / Screen.height;
                Vector2 delta = Mouse.current.delta.ReadValue();
                Vector3 upXZ = new Vector3(transform.up.x, 0, transform.up.z);
                Vector3 drag = (transform.right * -delta.x + upXZ * (-delta.y / upXZ.sqrMagnitude))
                               * worldUnitsPerPixel * _middleMousePanSpeed;
                transform.position += drag;
                _currentVelocity = Vector3.zero;
                return;
            }

            Vector2 totalInput = Vector2.ClampMagnitude(_moveInput + GetEdgePanInput(), 1f);
            Vector3 inputDirection = (transform.right * totalInput.x) + (transform.forward * totalInput.y);
            Vector3 normalizedInputDirection = new Vector3(inputDirection.x, 0, inputDirection.z).normalized;

            Vector3 targetPosition = transform.position + normalizedInputDirection * _moveSpeed * Time.deltaTime;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime);
        }

        private Vector2 GetEdgePanInput()
        {
            if (!Application.isFocused) return Vector2.zero;
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return Vector2.zero;

            Vector2 mouse = Mouse.current.position.ReadValue();
            if (mouse.x < 0 || mouse.x > Screen.width || mouse.y < 0 || mouse.y > Screen.height)
                return Vector2.zero;
            Vector2 pan = Vector2.zero;

            if (mouse.x < _edgePanThreshold)
                pan.x = -(1f - mouse.x / _edgePanThreshold);
            else if (mouse.x > Screen.width - _edgePanThreshold)
                pan.x = 1f - (Screen.width - mouse.x) / _edgePanThreshold;

            if (mouse.y < _edgePanThreshold)
                pan.y = -(1f - mouse.y / _edgePanThreshold);
            else if (mouse.y > Screen.height - _edgePanThreshold)
                pan.y = 1f - (Screen.height - mouse.y) / _edgePanThreshold;

            return pan;
        }

        private void ZoomCamera()
        {
            if (Mathf.Abs(_zoomInput) > float.Epsilon)
            {
                _targetZoom -= _zoomInput * _zoomSpeed;
                _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
            }

            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetZoom, Time.deltaTime * 10f);
        }
    }
}