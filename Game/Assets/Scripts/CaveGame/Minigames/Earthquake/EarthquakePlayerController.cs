using System;
using CaveTogether.Game.Entities;
using CaveTogether.Input;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CaveTogether.Minigames.Earthquake
{
    public class EarthquakePlayerController : NetworkBehaviour
    {
        [SerializeField] private CharacterRenderer _renderer;
        [SerializeField] private TextMeshPro _nameLabel;
        [SerializeField] private TextMeshPro _playerIdentifierLabel;
        [SerializeField] private SpriteRenderer _playerIdentifierTriangle;
        [SerializeField] private ParticleSystem _walkDustParticles;

        private static readonly int _animIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int _animDown = Animator.StringToHash("Down");

        private Animator _animator;

        [Header("Control Properties")]
        [SerializeField] private float _moveSpeed = 3;

        public Character Character { get; private set; }
        public bool IsDown { get; private set; }

        private PlayerControlsInput _controls;
        private float _moveDelta;
        private float _startSize = 1.0f;

        private NetworkVariable<bool> _isMoving = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> _direction = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> _locked = new(writePerm: NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            Character = FindAnyObjectByType<CharacterManager>().GetCharacter(OwnerClientId);
            _animator = _renderer.GetComponent<Animator>();

            _startSize = _renderer.transform.localScale.x;

            _playerIdentifierLabel.color = new Color(1f, 1f, 1f, 0f);
            _playerIdentifierTriangle.color = new Color(1f, 1f, 1f, 0f);

            _renderer.Initialize(Character.CharacterData);
            _nameLabel.SetText(Character.Config.Username.ToString());

            _isMoving.OnValueChanged += OnIsMovingChanged;

            if (IsOwner)
                SetClientCharacter();
        }

        public override void OnNetworkDespawn()
        {
            _isMoving.OnValueChanged -= OnIsMovingChanged;

            if (_controls == null) return;
            _controls.EarthquakeMinigame.Move.performed -= OnPlayerMove;
            _controls.EarthquakeMinigame.Move.canceled -= OnPlayerMove;
            _controls.Disable();
        }

        private void SetClientCharacter()
        {
            _playerIdentifierTriangle.color = new Color(1f, 1f, 1f, 1f);

            _controls = new PlayerControlsInput();
            _controls.EarthquakeMinigame.Move.performed += OnPlayerMove;
            _controls.EarthquakeMinigame.Move.canceled += OnPlayerMove;
            _controls.Enable();
        }

        private void OnPlayerMove(InputAction.CallbackContext context) => _moveDelta = context.ReadValue<float>();

        public void Down()
        {
            if (IsDown) return;
            _animator.SetTrigger(_animDown);
            _animator.SetBool(_animIsMoving, false);
            IsDown = true;
            GetComponent<Collider2D>().enabled = false;

            if (!IsOwner) return;

            _moveDelta = 0;
            _locked.Value = true;
            _isMoving.Value = false;
        }

        private void Update()
        {
            _animator.SetBool(_animIsMoving, _isMoving.Value);
            _renderer.transform.localScale = new Vector3(_direction.Value ? _startSize : -_startSize, _startSize, _startSize);

            if (!IsOwner || _locked.Value || IsDown) return;

            float speedAbs = Mathf.Abs(_moveDelta);
            _isMoving.Value = speedAbs > float.Epsilon;

            if (_isMoving.Value)
            {
                float target = transform.position.x + (_moveDelta * _moveSpeed * Time.deltaTime);
                _direction.Value = _moveDelta > 0;

                transform.position = new Vector3(
                    Mathf.Clamp(target, EarthquakeMinigame.StalagmiteXStart, EarthquakeMinigame.StalagmiteXEnd),
                    transform.position.y,
                    transform.position.z
                );
            }
        }

        private void OnIsMovingChanged(bool previousValue, bool newValue)
        {
            if (newValue)
                _walkDustParticles.Play();
            else
                _walkDustParticles.Stop();
        }
    }
}