using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common.Enums;
using CaveTogether.Utils;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public struct CellVariant
    {
        public CellBaseData Data;
        public int RotationSteps; // 0, 1, 2, or 3 (multiply by 90 for degrees)
        public Direction RotatedDirections;
    }

    public class BaseGenerationLayer : MapGenerationLayerBase
    {
        private static string _baseCellScriptablePath = "Data/Generation/CellBase";
        private MapData _mapData;

        // Generation parameters
        private float _targetFilledCellsPercentage = .3f;
        private float _minimumFillRatio = .6f;
        private int _maxGenerationAttempts = 25;

        private float _branchTerminationChance = .1f;
        private float _adjacentWallTerminationBonus = .3f;
        private float _highConnectivityPenalty = .7f;

        public override void Process(MapData mapData, System.Random random)
        {
            _mapData = mapData;

            int targetFilledCells = (int)(mapData.Width * mapData.Height * _targetFilledCellsPercentage);
            int minimumFilledCells = (int)(targetFilledCells * _minimumFillRatio);
            int filledCellsSoFar = 0;
            int attempts = 0;

            var cellBaseList = Resources.LoadAll<CellBaseData>(_baseCellScriptablePath).ToList();

            List<CellVariant> allVariants = new List<CellVariant>();
            Dictionary<Direction, CellVariant> terminationEnds = new Dictionary<Direction, CellVariant>();

            foreach (var cell in cellBaseList)
            {
                for (int r = 0; r < 4; r++) // 4 possible rotations (0, 90, 180, 270)
                {
                    Direction rotatedDirs = DirectionHelpers.RotateDirectionsClockwise(cell.OpenDirections, r);

                    if (!allVariants.Any(v => v.Data == cell && v.RotatedDirections == rotatedDirs))
                    {
                        allVariants.Add(new CellVariant
                        {
                            Data = cell,
                            RotationSteps = r,
                            RotatedDirections = rotatedDirs
                        });
                    }
                }
            }

            allVariants.ForEach(variant =>
            {
                if (variant.RotatedDirections == Direction.North) terminationEnds.Add(Direction.North, variant);
                else if (variant.RotatedDirections == Direction.East) terminationEnds.Add(Direction.East, variant);
                else if (variant.RotatedDirections == Direction.South) terminationEnds.Add(Direction.South, variant);
                else if (variant.RotatedDirections == Direction.West) terminationEnds.Add(Direction.West, variant);
            });

            Vector2Int startCellPosition = new Vector2Int(mapData.Width / 2, mapData.Height / 2);
            var startCellData = cellBaseList.Find(cell => cell.OpenDirections == (Direction.North | Direction.East | Direction.South | Direction.West));

            do
            {
                for (int y = 0; y < mapData.Height; y++)
                    for (int x = 0; x < mapData.Width; x++)
                        mapData.Cells[y, x] = default;

                filledCellsSoFar = 0;
                attempts++;

                _mapData[startCellPosition.y, startCellPosition.x] = new CellData
                {
                    X = startCellPosition.x,
                    Y = startCellPosition.y,
                    Orientation = 0,
                    Data = startCellData
                };

                Stack<Vector2Int> cellsToPopulate = new Stack<Vector2Int>();
                cellsToPopulate.Push(startCellPosition);

                Direction[] allDirections = { Direction.North, Direction.East, Direction.South, Direction.West };

                while (cellsToPopulate.Count > 0)
                {
                    Vector2Int cellPosition = cellsToPopulate.Pop();
                    ref var cell = ref _mapData.GetCellRef(cellPosition.x, cellPosition.y);

                    var openDirectionsList = DirectionHelpers.DirectionFlagToList(DirectionHelpers.RotateDirectionsClockwise(cell.Data.OpenDirections, cell.Orientation));
                    openDirectionsList.Shuffle(random);

                    foreach (var dir in openDirectionsList)
                    {
                        Vector2Int candidatePosition = DirectionHelpers.AddDirectionToPosition(cellPosition, dir);

                        if (!IsCellFree(candidatePosition)) continue;

                        // Environmental parameters calculation
                        int distanceToEdge = GetDistanceToEdge(candidatePosition);
                        float distanceToEdgeNormalized = (float)distanceToEdge / mapData.Width;

                        // Dont fill the map too much
                        float currentFilledCelledByTargetPercentage = (float)filledCellsSoFar / targetFilledCells;

                        // Pre-compute neighbor connections: required openings + adjacent wall count
                        Direction precomputedRequired = Direction.None;
                        int adjacentWallCount = 0;
                        foreach (var d in allDirections)
                        {
                            var neighborPos = DirectionHelpers.AddDirectionToPosition(candidatePosition, d);
                            if (IsOutOfBounds(neighborPos) || IsCellFree(neighborPos)) continue;

                            var neighborCell = mapData.GetCellRef(neighborPos);
                            Direction neighborDoors = DirectionHelpers.RotateDirectionsClockwise(neighborCell.Data.OpenDirections, neighborCell.Orientation);

                            if ((DirectionHelpers.GetOppositeDirection(d) & neighborDoors) > 0)
                                precomputedRequired |= d;
                            else
                                adjacentWallCount++;
                        }

                        float terminationChance = _branchTerminationChance * distanceToEdgeNormalized;
                        terminationChance += adjacentWallCount * _adjacentWallTerminationBonus;

                        if (distanceToEdge <= 3)
                            terminationChance += (3.0f - distanceToEdge) * .34f;

                        // Only terminate if no other placed neighbor requires a connection here
                        bool canTerminate = precomputedRequired == DirectionHelpers.GetOppositeDirection(dir);
                        if (canTerminate && random.NextDouble() < terminationChance)
                        {
                            // Terminate
                            var terminationEnd = terminationEnds[DirectionHelpers.GetOppositeDirection(dir)];
                            mapData.Cells[candidatePosition.y, candidatePosition.x] = new CellData
                            {
                                X = candidatePosition.x,
                                Y = candidatePosition.y,
                                Data = terminationEnd.Data,
                                Orientation = terminationEnd.RotationSteps
                            };

                            filledCellsSoFar++;
                        }
                        else
                        {
                            // Branch
                            Direction requiredOpenings = Direction.None;
                            Direction forbiddenOpenings = Direction.None;

                            foreach (var d in allDirections)
                            {
                                var neighborPos = DirectionHelpers.AddDirectionToPosition(candidatePosition, d);

                                if (IsOutOfBounds(neighborPos)) forbiddenOpenings |= d;

                                if (!IsCellFree(neighborPos))
                                {
                                    var neighborCell = mapData.GetCellRef(neighborPos);
                                    Direction neighborRotatedDoors = DirectionHelpers.RotateDirectionsClockwise(neighborCell.Data.OpenDirections, neighborCell.Orientation);

                                    if ((DirectionHelpers.GetOppositeDirection(d) & neighborRotatedDoors) > 0) requiredOpenings |= d;
                                    else forbiddenOpenings |= d;
                                }
                            }

                            var validVariants = allVariants
                                .Where(v => (v.RotatedDirections & requiredOpenings) == requiredOpenings
                                         && (v.RotatedDirections & forbiddenOpenings) == 0)
                                .ToList();

                            if (validVariants.Count > 0)
                            {
                                int neighborConnectionCount = DirectionHelpers.DirectionFlagToList(precomputedRequired).Count;

                                float VariantWeight(CellVariant v)
                                {
                                    float w = v.Data.GetCalculatedWeight(distanceToEdgeNormalized, currentFilledCelledByTargetPercentage);
                                    if (DirectionHelpers.DirectionFlagToList(v.RotatedDirections).Count >= 3 && neighborConnectionCount >= 2)
                                        w *= _highConnectivityPenalty;
                                    return w;
                                }

                                float totalWeight = validVariants.Sum(VariantWeight);
                                float pick = (float)(random.NextDouble() * totalWeight);
                                float cumulative = 0f;

                                CellVariant chosen = validVariants[0];
                                foreach (var variant in validVariants)
                                {
                                    cumulative += VariantWeight(variant);
                                    if (pick < cumulative) { chosen = variant; break; }
                                }

                                mapData.Cells[candidatePosition.y, candidatePosition.x] = new CellData
                                {
                                    X = candidatePosition.x,
                                    Y = candidatePosition.y,
                                    Data = chosen.Data,
                                    Orientation = chosen.RotationSteps
                                };

                                filledCellsSoFar++;
                            }

                            cellsToPopulate.Push(candidatePosition);
                        }
                    }
                }
            }
            while (filledCellsSoFar < minimumFilledCells && attempts < _maxGenerationAttempts);

            Debug.Log($"[BaseGenerationLayer] Map generated in {attempts} attempts.");
        }

        private int GetDistanceToEdge(Vector2Int position)
        {
            int xDistance = Mathf.Min(position.x, _mapData.Width - position.x - 1);
            int yDistance = Mathf.Min(position.y, _mapData.Height - position.y - 1);

            return Mathf.Min(xDistance, yDistance);
        }

        public bool IsOutOfBounds(Vector2Int position)
        {
            return position.x < 0 || position.x >= _mapData.Width || position.y < 0 || position.y >= _mapData.Height;
        }

        public bool IsCellFree(Vector2Int position)
        {
            if (IsOutOfBounds(position)) return false;
            ref CellData cellRef = ref _mapData.GetCellRef(position);
            return cellRef.IsEmpty;
        }
    }
}