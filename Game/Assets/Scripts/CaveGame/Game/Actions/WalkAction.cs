using System;
using System.Collections;
using System.Collections.Generic;
using CaveTogether.Common.Enums;
using CaveTogether.Game.CellEffects;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Movement;
using CaveTogether.Generation;
using CaveTogether.Generation.Layers;

namespace CaveTogether.Game.Actions
{
    public class WalkAction : GameActionBase
    {
        private static readonly UnityEngine.WaitForSeconds CellSpawnWait = new(0.3f);

        public override ActionType Type => ActionType.Walk;
        public override ActionUIType UIType => ActionUIType.ContextualCell;
        public override string DisplayName => "Walk here";

        public override IEnumerable<CellActionEntry> GetEntries(MapData map, Character character, int x, int y)
        {
            var pos = new UnityEngine.Vector2Int(x, y);
            if (!MovementValidator.GetWalkableNeighbours(map, character.GridPosition).Contains(pos))
                yield break;

            int baseCost = BaseCost(map, pos);
            var explorationManager = UnityEngine.Object.FindAnyObjectByType<ExplorationManager>();
            bool isExplored = explorationManager == null || explorationManager.IsExplored(pos);
            var options = isExplored ? GetOptionsForCell(map, pos) : new List<WalkCellOption>();

            if (options.Count == 0)
            {
                if (baseCost <= character.Energy)
                    yield return MakeEntry(pos, DisplayName, baseCost, 0);
            }
            else
            {
                for (byte i = 0; i < options.Count; i++)
                {
                    int cost = baseCost + options[i].ExtraEnergyCost;
                    if (cost <= character.Energy)
                        yield return MakeEntry(pos, options[i].Title, cost, i);
                }
            }
        }

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request)
        {
            int baseCost = BaseCost(map, request.TargetCell);
            var options = GetOptionsForCell(map, request.TargetCell);
            int extra = options.Count > 0 && request.ItemActionIndex < options.Count
                ? options[request.ItemActionIndex].ExtraEnergyCost
                : 0;
            return baseCost + extra;
        }

        public override bool IsValid(MapData map, Character character, ActionRequest request)
        {
            if (request.TargetCell == character.GridPosition) return false;
            return MovementValidator.GetWalkableNeighbours(map, character.GridPosition)
                .Contains(request.TargetCell);
        }

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request)
        {
            var explorationManager = UnityEngine.Object.FindAnyObjectByType<ExplorationManager>();
            var options = GetOptionsForCell(map, request.TargetCell);

            Func<UnityEngine.Vector2Int, bool> isPassable = explorationManager != null
                ? explorationManager.IsExplored
                : null;

            var path = MapPathFinder.GetPath(map, character.GridPosition, request.TargetCell, isPassable);
            if (path == null) { character.SetPosition(request.TargetCell); yield break; }

            for (int i = 1; i < path.Length; i++)
            {
                bool wasExplored = explorationManager == null || explorationManager.IsExplored(path[i]);
                explorationManager?.RevealFromPosition(path[i]);

                if (!wasExplored)
                    yield return CellSpawnWait;

                yield return character.StartCoroutine(character.MoveToCell(path[i]));

                if (options.Count > 0 && request.ItemActionIndex < options.Count)
                    yield return character.StartCoroutine(options[request.ItemActionIndex].OnEnter(character));

                var effectManager = UnityEngine.Object.FindAnyObjectByType<CellEffectManager>();
                if (effectManager != null)
                    yield return character.StartCoroutine(
                        effectManager.TriggerEffects(CellEffectTrigger.OnEnter, path[i], character));
            }
        }

        private static int BaseCost(MapData map, UnityEngine.Vector2Int pos)
        {
            int cost = 1;
            var generator = UnityEngine.Object.FindAnyObjectByType<MapManager>()?.Generator;
            if (generator != null)
                foreach (var modifier in generator.GetCostModifiers())
                    cost += modifier.GetEntryCostModifier(pos);
            return cost;
        }

        private static List<WalkCellOption> GetOptionsForCell(MapData map, UnityEngine.Vector2Int pos)
        {
            var options = new List<WalkCellOption>();
            var generator = UnityEngine.Object.FindAnyObjectByType<MapManager>()?.Generator;
            if (generator != null)
                foreach (var provider in generator.GetWalkOptionProviders())
                    foreach (var opt in provider.GetWalkOptions(pos))
                        options.Add(opt);
            return options;
        }

        private CellActionEntry MakeEntry(UnityEngine.Vector2Int pos, string title, int cost, byte optionIndex) =>
            new CellActionEntry
            {
                Type = Type,
                Title = title,
                EnergyCost = cost,
                Position = pos,
                Request = new ActionRequest { Type = Type, TargetCell = pos, ItemActionIndex = optionIndex }
            };
    }
}
