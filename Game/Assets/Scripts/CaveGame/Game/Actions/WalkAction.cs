using System;
using System.Collections;
using CaveTogether.Common.Enums;
using CaveTogether.Game.CellEffects;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Movement;
using CaveTogether.Generation;

namespace CaveTogether.Game.Actions
{
    public class WalkAction : GameActionBase
    {
        private static readonly UnityEngine.WaitForSeconds CellSpawnWait = new(0.3f);
        public override ActionType Type => ActionType.Walk;
        public override ActionUIType UIType => ActionUIType.ContextualCell;

        public override string DisplayName => "Walk here";

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            var explorationManager = UnityEngine.Object.FindAnyObjectByType<ExplorationManager>();

            Func<UnityEngine.Vector2Int, bool> isPassable = explorationManager != null ? explorationManager.IsExplored : null;

            var path = MapPathFinder.GetPath(map, character.GridPosition, request.TargetCell, isPassable);
            if (path == null) { character.SetPosition(request.TargetCell); yield break; }

            for (int i = 1; i < path.Length; i++)
            {
                bool wasExplored = explorationManager == null || explorationManager.IsExplored(path[i]);
                explorationManager?.RevealFromPosition(path[i]);

                if (!wasExplored)
                    yield return CellSpawnWait;

                yield return character.StartCoroutine(character.MoveToCell(path[i]));

                var effectManager = UnityEngine.Object.FindAnyObjectByType<CellEffectManager>();
                if (effectManager != null)
                    yield return character.StartCoroutine(
                        effectManager.TriggerEffects(CellEffectTrigger.OnEnter, path[i], character));
            }
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request)
        {
            int cost = 1;
            var mapManager = UnityEngine.Object.FindAnyObjectByType<MapManager>();

            var generator = mapManager != null ? mapManager.Generator : null;
            if (generator != null)
                foreach (var modifier in generator.GetCostModifiers())
                    cost += modifier.GetEntryCostModifier(request.TargetCell);

            return cost;
        }

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            if (request.TargetCell == character.GridPosition) return false;
            return MovementValidator.GetWalkableNeighbours(map, character.GridPosition)
                .Contains(request.TargetCell);
        }
    }
}