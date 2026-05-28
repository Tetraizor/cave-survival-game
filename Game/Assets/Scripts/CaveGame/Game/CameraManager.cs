using CaveTogether.Minigames;
using CaveTogether.Services;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CaveTogether.Game
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(AudioListener))]
    public class CameraManager : MonoBehaviour
    {
        private const float DefaultZoom = 4;

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
        private AudioListener _audioListener;
        private bool _isFocusing;

        private bool _enableMovement;

        private InputService _inputService;

        public void Initialize()
        {
            _enableMovement = true;

            _camera = GetComponent<Camera>();
            _audioListener = GetComponent<AudioListener>();
            _targetZoom = DefaultZoom;

            _inputService = ServiceLocator.Get<InputService>();
            _inputService.CameraMoveInputChanged += OnCameraMoveInputChanged;
            _inputService.CameraZoomInputChanged += OnCameraZoomInputChanged;

            var mm = FindAnyObjectByType<MinigameManager>();
            mm.MinigameBegan -= OnMinigameBegan;
            mm.MinigameBegan += OnMinigameBegan;
            mm.MinigameEnded -= OnMinigameEnded;
            mm.MinigameEnded += OnMinigameEnded;
        }

        private void OnMinigameEnded()
        {
            _camera.enabled = true;
            _audioListener.enabled = true;
        }

        private void OnMinigameBegan()
        {
            _camera.enabled = false;
            _audioListener.enabled = false;
        }

        public void Deinitialize()
        {
            _enableMovement = false;

            _inputService.CameraMoveInputChanged -= OnCameraMoveInputChanged;
            _inputService.CameraZoomInputChanged -= OnCameraZoomInputChanged;
        }

        private void OnDestroy()
        {
            var mm = FindAnyObjectByType<MinigameManager>();
            if (mm == null) return;
            mm.MinigameBegan -= OnMinigameBegan;
            mm.MinigameEnded -= OnMinigameEnded;
        }

        private void OnCameraZoomInputChanged(float input) => _zoomInput = input;
        private void OnCameraMoveInputChanged(Vector2 input) => _moveInput = input;

        private void LateUpdate()
        {
            if (!_enableMovement || _camera == null) return;

            MoveCamera();
            ZoomCamera();
        }

        public void Shake(float duration, float strength = .35f, int vibrato = 20)
        {
            transform.DOShakePosition(duration, strength, vibrato);
        }

        public void FocusOn(Vector3 target, float zoom = DefaultZoom, float duration = .5f)
        {
            float t = (target.y - transform.position.y) / transform.forward.y;
            Vector3 screenCenter = transform.position + transform.forward * t;
            Vector3 destination = transform.position + new Vector3(target.x - screenCenter.x, 0, target.z - screenCenter.z);

            _isFocusing = true;
            _targetZoom = zoom;
            _currentVelocity = Vector3.zero;
            transform.DOMove(destination, duration).SetEase(Ease.OutCubic).OnComplete(() => _isFocusing = false);
            DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, zoom, duration).SetEase(Ease.OutCubic);
        }

        private void MoveCamera()
        {
            if (_isFocusing) return;

            if (_inputService.IsGrabbingCamera)
            {
                float worldUnitsPerPixel = (2f * _camera.orthographicSize) / Screen.height;
                Vector2 delta = _inputService.PointerDelta;
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

            Vector2 mouse = _inputService.PointerScreenPosition;
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