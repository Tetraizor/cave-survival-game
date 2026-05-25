using System.Collections;
using System.Collections.Generic;
using CaveTogether.Common;
using CaveTogether.Common.Enums;
using CaveTogether.Generation;
using DG.Tweening;
using UnityEngine;

namespace CaveTogether.Game.Entities
{
    public class Character : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public int Energy { get; private set; }
        public int MaxEnergy { get; private set; }

        public ulong OwnerClientId { get; private set; }

        private MapManager _mapManager;
        private MapRenderManager _mapRenderManager;

        public List<ActionType> PossibleActionTypes { get; private set; } = new();

        public void Initialize(CharacterDataSO characterData, PlayerConfig playerConfig)
        {
            transform.eulerAngles = new Vector3(45, 45, 0);

            _mapManager = FindAnyObjectByType<MapManager>();
            _mapRenderManager = FindAnyObjectByType<MapRenderManager>();

            // Character data assignments
            Health = characterData.MaxHealth;
            MaxHealth = characterData.MaxHealth;

            Energy = characterData.MaxEnergy;
            MaxEnergy = characterData.MaxEnergy;

            PossibleActionTypes.AddRange(characterData.PossibleActionTypes);

            // Player config assignments
            OwnerClientId = playerConfig.OwnerClientId;
        }

        public void SetPosition(Vector2Int position)
        {
            GridPosition = position;
            transform.position = _mapRenderManager.GridToWorldPosition(position) + GetWorldPositionOffset();
        }

        public IEnumerator MoveToCell(Vector2Int position)
        {
            GridPosition = position;
            Vector3 target = _mapRenderManager.GridToWorldPosition(position) + GetWorldPositionOffset();
            yield return transform.DOMove(target, 0.15f).WaitForCompletion();
        }

        public void UseEnergy(int energy) => Energy = Mathf.Max(Energy - energy, 0);
        public void GainEnergy(int energy) => Energy += Mathf.Min(energy, MaxEnergy);
        public void ResetEnergy() => Energy = MaxEnergy;

        public bool CanDoAction(ActionType type) => PossibleActionTypes.Contains(type);

        private Vector3 GetWorldPositionOffset() => new Vector3(1, 0, 1) * MapRenderManager.CELL_SIZE / 2;
    }
}