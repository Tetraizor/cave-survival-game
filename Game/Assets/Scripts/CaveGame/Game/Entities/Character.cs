using System.Collections.Generic;
using CaveTogether.Common.Enums;
using CaveTogether.Generation;
using Unity.VisualScripting;
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

        private List<ActionType> _possibleActions = new();

        public void Initialize(CharacterDataSO data)
        {
            transform.eulerAngles = new Vector3(45, 45, 0);

            _mapManager = FindAnyObjectByType<MapManager>();
            _mapRenderManager = FindAnyObjectByType<MapRenderManager>();

            Health = data.MaxHealth;
            MaxHealth = data.MaxHealth;

            Energy = data.MaxEnergy;
            MaxEnergy = data.MaxEnergy;

            _possibleActions.AddRange(data.PossibleActions);
        }

        public void SetPosition(Vector2Int position)
        {
            GridPosition = position;
            transform.position = _mapRenderManager.GridToWorldPosition(position) + GetWorldPositionOffset();
        }

        public void UseEnergy(int energy) => Energy = Mathf.Max(Energy - energy, 0);
        public void GainEnergy(int energy) => Energy += Mathf.Min(energy, MaxEnergy);
        public void ResetEnergy() => Energy = MaxEnergy;

        public bool CanDoAction(ActionType type) => _possibleActions.Contains(type);

        private Vector3 GetWorldPositionOffset() => new Vector3(1, 0, 1) * MapRenderManager.CELL_SIZE / 2;
    }
}