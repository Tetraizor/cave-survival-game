using System;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 100;
        [SerializeField] private float _smoothTime = .15f;
        private Vector2 _moveInput;
        private Vector3 _currentVelocity;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 1.5f;
        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 15f;
        private float _zoomInput;
        private float _targetZoom;

        private Camera _camera;

        public void Initialize()
        {
            _camera = GetComponent<Camera>();
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

        private void MoveCamera()
        {
            Vector3 inputDirection = (transform.right * _moveInput.x) + (transform.forward * _moveInput.y);
            Vector3 normalizedInputDirection = new Vector3(inputDirection.x, 0, inputDirection.z).normalized;

            Vector3 targetPosition = transform.position + (normalizedInputDirection * _moveSpeed * Time.deltaTime);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime);
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