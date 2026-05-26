using System;
using System.Collections;
using System.Collections.Generic;
using CaveTogether.Common;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Turn;
using CaveTogether.Generation;
using DG.Tweening;
using UnityEngine;

namespace CaveTogether.Game.Entities
{
    public class Character : MonoBehaviour
    {
        public Action<int> HealthChanged;
        public Action<int> EnergyChanged;

        [SerializeField] private GameObject _selectionOutline;
        [SerializeField] private GameObject _highlight;
        [SerializeField] private GameObject _renderer;

        public Vector2Int GridPosition { get; private set; }

        public bool IsDown => Health == 0;
        public bool IsEscaped { get; private set; }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public int Energy { get; private set; }
        public int MaxEnergy { get; private set; }

        public ulong OwnerClientId { get; private set; }

        private MapManager _mapManager;
        private MapRenderManager _mapRenderManager;
        private CharacterManager _characterManager;

        public List<ActionType> PossibleActionTypes { get; private set; } = new();

        public void Initialize(CharacterDataSO characterData, PlayerConfig playerConfig)
        {
            _renderer.transform.eulerAngles = new Vector3(45, 45, 0);
            _selectionOutline.transform.DOScale(_selectionOutline.transform.localScale * 1.1f, 1f).SetLoops(-1, LoopType.Yoyo);
            _highlight.gameObject.SetActive(false);

            _mapManager = FindAnyObjectByType<MapManager>();
            _mapRenderManager = FindAnyObjectByType<MapRenderManager>();
            _characterManager = FindAnyObjectByType<CharacterManager>();

            FindAnyObjectByType<TurnManager>().TurnStarted += OnTurnStarted;
            FindAnyObjectByType<TurnManager>().RoundEnded += OnRoundEnded;

            // Character data assignments
            Health = characterData.MaxHealth;
            MaxHealth = characterData.MaxHealth;

            Energy = characterData.MaxEnergy;
            MaxEnergy = characterData.MaxEnergy;

            PossibleActionTypes.AddRange(characterData.PossibleActionTypes);
            PossibleActionTypes.Add(ActionType.Escape);

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
            var previousPosition = GridPosition;
            GridPosition = position;

            Vector3 target = _mapRenderManager.GridToWorldPosition(position) + GetWorldPositionOffset() + _characterManager.GetCellOffset(this);

            ApplyDirectionFlip(target);
            _characterManager.RefreshCellPositions(position, exclude: this);
            yield return transform.DOMove(target, 0.5f).WaitForCompletion();
            _characterManager.RefreshCellPositions(previousPosition);
        }

        public void RefreshPosition()
        {
            Vector3 target = _mapRenderManager.GridToWorldPosition(GridPosition) + GetWorldPositionOffset() + _characterManager.GetCellOffset(this);
            ApplyDirectionFlip(target);
            transform.DOMove(target, 0.2f).SetEase(Ease.OutCubic);
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
            _renderer.SetActive(false);
            _selectionOutline.SetActive(false);
        }

        public void TakeDamage(int amount) { Health = Mathf.Max(Health - amount, 0); HealthChanged?.Invoke(Health); }
        public void Heal(int amount) { Health = Mathf.Min(Health + amount, MaxHealth); HealthChanged?.Invoke(Health); }

        public void UseEnergy(int energy) { Energy = Mathf.Max(Energy - energy, 0); EnergyChanged?.Invoke(Energy); }
        public void GainEnergy(int energy) { Energy = Mathf.Min(Energy + energy, MaxEnergy); EnergyChanged?.Invoke(Energy); }
        public void ResetEnergy() { Energy = MaxEnergy; EnergyChanged?.Invoke(Energy); }

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
        }
    }
}