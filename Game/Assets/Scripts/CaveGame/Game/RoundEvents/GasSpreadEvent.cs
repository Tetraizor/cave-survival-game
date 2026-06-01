using System;
using System.Collections;
using System.Collections.Generic;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;
using CaveTogether.Items;
using UnityEngine;

namespace CaveTogether.Game.RoundEvents
{
    public class GasSpreadEvent : RoundEventBase
    {
        private readonly IReadOnlyCollection<Vector2Int> _ventCells;
        private readonly MapData _mapData;
        private readonly HashSet<Vector2Int> _activeGasCells = new();

        private int _radius = -1; // -1 = inactive
        private int _cooldownRounds = 0;

        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        private static readonly WaitForSeconds WaitBetweenVents = new(1.5f);

        public event Action GasCellsChanged;

        public bool IsGasCell(Vector2Int pos) => _activeGasCells.Contains(pos);
        public IReadOnlyCollection<Vector2Int> ActiveGasCells => _activeGasCells;

        public GasSpreadEvent(IReadOnlyCollection<Vector2Int> ventCells, MapData mapData)
        {
            _ventCells = ventCells;
            _mapData = mapData;
        }

        public override int Weight => 2;
        public override bool CanHappen() => _radius < 0 && _cooldownRounds == 0;

        public override IEnumerator OnRoundPassed(CharacterManager characterManager)
        {
            if (_radius < 0)
            {
                if (_cooldownRounds > 0) _cooldownRounds--;
                yield break;
            }

            _radius++;

            if (_radius > 2)
            {
                _radius = -1;
                _cooldownRounds = 1;
                _activeGasCells.Clear();
                GasCellsChanged?.Invoke();
                yield break;
            }

            RebuildGasCells();
            GasCellsChanged?.Invoke();

            foreach (var character in characterManager.Characters)
            {
                if (character == null || character.IsDown || character.Inventory.HasItemOfType(ItemType.GasMask)) continue;
                if (_activeGasCells.Contains(character.GridPosition))
                    yield return character.StartCoroutine(character.TakeDamageSequence(1));
            }
        }

        public override IEnumerator Execute(int round, CharacterManager characterManager)
        {
            _radius = 0;
            RebuildGasCells();
            GasCellsChanged?.Invoke();

            var camera = UnityEngine.Object.FindAnyObjectByType<CameraManager>();
            var exploration = UnityEngine.Object.FindAnyObjectByType<ExplorationManager>();
            var renderManager = UnityEngine.Object.FindAnyObjectByType<MapRenderManager>();

            if (camera != null && exploration != null && renderManager != null)
            {
                foreach (var vent in _ventCells)
                {
                    if (!exploration.IsExplored(vent)) continue;
                    camera.FocusOn(renderManager.GridToWorldPosition(vent));
                    yield return WaitBetweenVents;
                }
            }
        }

        private void RebuildGasCells()
        {
            _activeGasCells.Clear();
            foreach (var vent in _ventCells)
                foreach (var cell in BfsFlood(vent, _radius))
                    _activeGasCells.Add(cell);
        }

        private IEnumerable<Vector2Int> BfsFlood(Vector2Int center, int radius)
        {
            var visited = new HashSet<Vector2Int> { center };
            var frontier = new List<Vector2Int> { center };
            yield return center;

            for (int step = 0; step < radius; step++)
            {
                var next = new List<Vector2Int>();
                foreach (var pos in frontier)
                {
                    foreach (var dir in Directions)
                    {
                        var neighbor = pos + dir;

                        if (visited.Contains(neighbor)) continue;

                        visited.Add(neighbor);

                        if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= _mapData.Width || neighbor.y >= _mapData.Height) continue;
                        if (_mapData.GetCellRef(neighbor.x, neighbor.y).IsEmpty) continue;

                        next.Add(neighbor);

                        yield return neighbor;
                    }
                }
                frontier = next;
            }
        }

        public override string BuildupMessage() => "A foul smell rises from the ground...";
        public override string AnnouncementMessage() => "Toxic gas is seeping through the vents!";
    }
}
