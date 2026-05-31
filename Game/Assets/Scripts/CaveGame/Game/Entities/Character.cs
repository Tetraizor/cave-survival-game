using System;
using System.Collections;
using System.Collections.Generic;
using CaveTogether.Common;
using CaveTogether.Common.Enums;
using CaveTogether.Game.CellEffects;
using CaveTogether.Game.Turn;
using CaveTogether.Game;
using CaveTogether.Generation;
using DG.Tweening;
using UnityEngine;
using CaveTogether.Items;

namespace CaveTogether.Game.Entities
{
    public class Character : MonoBehaviour
    {
        public Action<int, int> HealthChanged;
        public Action<int, int> EnergyChanged;

        [SerializeField] private GameObject _selectionOutline;
        [SerializeField] private GameObject _highlight;
        [SerializeField] private CharacterRenderer _renderer;

        public Vector2Int GridPosition { get; private set; }

        public bool IsDown => Health == 0;
        public bool IsEscaped { get; private set; }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public int Energy { get; private set; }
        public int MaxEnergy { get; private set; }

        public ulong OwnerClientId { get; private set; }

        public Inventory Inventory { get; private set; }

        private MapRenderManager _mapRenderManager;
        private CharacterManager _characterManager;

        private static readonly WaitForSeconds _waitHurt = new(0.1f);

        private static readonly int _animIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int _animDown = Animator.StringToHash("Down");
        private static readonly int _animGetUp = Animator.StringToHash("GetUp");
        private static readonly int _animHurt = Animator.StringToHash("Hurt");

        private Animator _animator;

        public CharacterDataSO CharacterData { get; private set; }
        public PlayerConfig Config { get; private set; }

        public List<ActionType> PossibleActionTypes { get; private set; } = new();

        public void Initialize(CharacterDataSO characterData, PlayerConfig playerConfig)
        {
            CharacterData = characterData;
            Config = playerConfig;

            _renderer.transform.eulerAngles = new Vector3(45, 45, 0);
            _renderer.Initialize(characterData);
            _animator = _renderer.GetComponent<Animator>();

            _selectionOutline.transform.DOScale(_selectionOutline.transform.localScale * 1.1f, 1f).SetLoops(-1, LoopType.Yoyo);
            _highlight.gameObject.SetActive(false);

            _mapRenderManager = FindAnyObjectByType<MapRenderManager>();
            _characterManager = FindAnyObjectByType<CharacterManager>();

            FindAnyObjectByType<TurnManager>().TurnStarted += OnTurnStarted;
            FindAnyObjectByType<TurnManager>().RoundEnded += OnRoundEnded;

            // Character data assignments
            Health = characterData.MaxHealth;
            MaxHealth = characterData.MaxHealth;

            Energy = characterData.MaxEnergy;
            MaxEnergy = characterData.MaxEnergy;

            Inventory = new Inventory();
            foreach (var startingItemType in characterData.StartingItemTypes) Inventory.AddItem(startingItemType);

            PossibleActionTypes.AddRange(characterData.PossibleActionTypes);

            // Player config assignments
            OwnerClientId = playerConfig.OwnerClientId;
        }

        public void SetPosition(Vector2Int position)
        {
            var previousPosition = GridPosition;
            GridPosition = position;
            transform.position = _mapRenderManager.GridToWorldPosition(position) + GetWorldPositionOffset() + _characterManager.GetCellOffset(this);
            _characterManager.RefreshCellPositions(position, exclude: this);
            _characterManager.RefreshCellPositions(previousPosition);
        }

        public IEnumerator MoveToCell(Vector2Int position)
        {
            _animator.SetBool(_animIsMoving, true);

            var previousPosition = GridPosition;
            GridPosition = position;

            Vector3 target = _mapRenderManager.GridToWorldPosition(position) + GetWorldPositionOffset() + _characterManager.GetCellOffset(this);

            ApplyDirectionFlip(target);
            _characterManager.RefreshCellPositions(position, exclude: this);
            yield return transform.DOMove(target, 1f).SetEase(Ease.InOutQuad).WaitForCompletion();
            _characterManager.RefreshCellPositions(previousPosition);

            _animator.SetBool(_animIsMoving, false);
            yield return new WaitForSeconds(.2f);
        }

        public void RefreshPosition()
        {
            Vector3 target = _mapRenderManager.GridToWorldPosition(GridPosition) + GetWorldPositionOffset() + _characterManager.GetCellOffset(this);
            float duration = Vector3.Distance(transform.position, target) / MapRenderManager.CELL_SIZE;
            ApplyDirectionFlip(target);
            transform.DOMove(target, duration).SetEase(Ease.OutCubic);
        }

        public void LookAt(Vector2Int targetCell)
        {
            Vector3 target = _mapRenderManager.GridToWorldPosition(targetCell) + GetWorldPositionOffset();
            ApplyDirectionFlip(target);
        }

        private void ApplyDirectionFlip(Vector3 target)
        {
            Vector3 delta = target - transform.position;
            float side = delta.x - delta.z;
            if (Mathf.Abs(side) > 0.01f)
            {
                Vector3 scale = _renderer.transform.localScale;
                scale.x = side > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                _renderer.transform.localScale = scale;
            }
        }

        public void Escape()
        {
            IsEscaped = true;
            _renderer.gameObject.SetActive(false);
            _selectionOutline.SetActive(false);
        }

        public IEnumerator WalkTo(Vector3 worldPosition)
        {
            _animator.SetBool(_animIsMoving, true);
            ApplyDirectionFlip(worldPosition);
            float duration = Mathf.Max(Vector3.Distance(transform.position, worldPosition) / MapRenderManager.CELL_SIZE, 0.05f);
            yield return transform.DOMove(worldPosition, duration).WaitForCompletion();
            _animator.SetBool(_animIsMoving, false);
        }

        public IEnumerator WalkToGridPosition()
        {
            Vector3 target = _mapRenderManager.GridToWorldPosition(GridPosition) + GetWorldPositionOffset() + _characterManager.GetCellOffset(this);
            float duration = Mathf.Max(Vector3.Distance(transform.position, target) / MapRenderManager.CELL_SIZE, 0.5f);
            _animator.SetBool(_animIsMoving, true);
            ApplyDirectionFlip(target);
            yield return transform.DOMove(target, duration).WaitForCompletion();
            _animator.SetBool(_animIsMoving, false);
        }

        public void PlayAnimationTrigger(string triggerName)
        {
            _animator.SetTrigger(triggerName);
        }

        public IEnumerator InspectCell(Action onReveal)
        {
            _animator.SetTrigger("Inspect");
            yield return new WaitForSeconds(2f);
            onReveal?.Invoke();
            yield return new WaitForSeconds(1.5f);
        }

        public IEnumerator TakeDamageSequence(int amount)
        {
            _animator.SetTrigger(_animHurt);
            yield return _waitHurt;
            TakeDamage(amount);
        }

        public void TakeDamage(int amount)
        {
            int previousHealth = Health;
            Health = Mathf.Max(Health - amount, 0);

            if (Health == 0) _animator.SetTrigger(_animDown);

            HealthChanged?.Invoke(previousHealth, Health);
        }

        public void Heal(int amount)
        {
            bool wasDown = IsDown;
            int previousHealth = Health;
            Health = Mathf.Min(Health + amount, MaxHealth);

            if (wasDown && !IsDown)
                StartCoroutine(HealSequence());
            else
                HealthChanged?.Invoke(previousHealth, Health);
        }

        private IEnumerator HealSequence()
        {
            int previousHealth = Health;
            _animator.SetTrigger(_animGetUp);

            yield return new WaitForSeconds(2.5f);
            HealthChanged?.Invoke(previousHealth, Health);

            RefreshPosition();
        }

        public void UseEnergy(int energy)
        {
            int previousEnergy = Energy;
            Energy = Mathf.Max(Energy - energy, 0);
            EnergyChanged?.Invoke(previousEnergy, Energy);
        }

        public void GainEnergy(int energy)
        {
            int previousEnergy = Energy;
            Energy = Mathf.Min(Energy + energy, MaxEnergy);
            EnergyChanged?.Invoke(previousEnergy, Energy);
        }

        public void ResetEnergy()
        {
            int previousEnergy = Energy;
            Energy = MaxEnergy;
            EnergyChanged?.Invoke(previousEnergy, Energy);
        }

        public bool CanDoAction(ActionType type) => PossibleActionTypes.Contains(type);

        private Vector3 GetWorldPositionOffset() => new Vector3(1, 0, 1) * MapRenderManager.CELL_SIZE / 2;

        public void SetHighlight(bool state)
        {
            _highlight.SetActive(state);
        }

        private void OnRoundEnded(int roundNumber)
        {
            _selectionOutline.SetActive(false);
        }

        private void OnTurnStarted(ulong clientId)
        {
            _selectionOutline.SetActive(clientId == OwnerClientId);
            if (clientId != OwnerClientId) return;

            var effectManager = FindAnyObjectByType<CellEffectManager>();
            if (effectManager != null)
                StartCoroutine(effectManager.TriggerEffects(CellEffectTrigger.OnTurnStart, GridPosition, this));
        }
    }
}