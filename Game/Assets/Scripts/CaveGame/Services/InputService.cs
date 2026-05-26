using System;
using CaveTogether.Input;
using UnityEngine;

namespace CaveTogether.Services
{
    public class InputService : MonoBehaviour, IService
    {
        private PlayerControlsInput _controls;

        public event Action<Vector2> CameraMoveInputChanged;
        public event Action<float> CameraZoomInputChanged;

        public event Action DebugPanelToggled;

        public bool IsCellSelectedThisFrame => _controls.Gameplay.SelectCell.WasPressedThisFrame();
        public bool IsGrabbingCamera => _controls.Gameplay.GrabCamera.IsPressed();
        public Vector2 PointerDelta => _controls.Gameplay.PointerDelta.ReadValue<Vector2>();
        public Vector2 PointerScreenPosition => _controls.Gameplay.PointerPosition.ReadValue<Vector2>();

        private void Awake()
        {
            ServiceLocator.Register<InputService>(this);
            DontDestroyOnLoad(gameObject);

            _controls = new PlayerControlsInput();

            //  _controls.Gameplay.MoveCamera.performed += ctx => CameraMoveInputChanged?.Invoke(ctx.ReadValue<Vector2>());
            _controls.Gameplay.MoveCamera.performed += ctx => CameraMoveInputChanged?.Invoke(ctx.ReadValue<Vector2>());
            _controls.Gameplay.MoveCamera.canceled += ctx => CameraMoveInputChanged?.Invoke(Vector2.zero);
            _controls.Gameplay.ToggleDebugPanel.performed += _ => DebugPanelToggled?.Invoke();

            _controls.Gameplay.ZoomCamera.performed += ctx =>
            {
                float rawScroll = ctx.ReadValue<float>();
                float normalizedScroll = Mathf.Sign(rawScroll) * (Mathf.Abs(rawScroll) > 0 ? 1 : 0);
                CameraZoomInputChanged?.Invoke(normalizedScroll);
            };
            _controls.Gameplay.ZoomCamera.canceled += ctx => CameraZoomInputChanged?.Invoke(0f);
        }

        private void OnEnable()
        {
            _controls.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _controls.Gameplay.Disable();
        }
    }
}