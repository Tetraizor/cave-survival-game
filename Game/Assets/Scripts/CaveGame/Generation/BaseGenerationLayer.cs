using System.Collections.Generic;
using System.Linq;
using CaveGame.Common.Enums;
using CaveGame.Utils;
using UnityEngine;

namespace CaveGame.Generation
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

        public override void Process(MapData mapData, System.Random random)
        {
            _mapData = mapData;

            var cellBaseList = Resources.LoadAll<CellBaseData>(_baseCellScriptablePath).ToList();

            Debug.Log($"Loaded {cellBaseList.Count} cell base scriptables.");
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

            Debug.Log($"Expanded {cellBaseList.Count} base cells into {allVariants.Count} unique rotated variants.");
            Debug.Log($"Found {terminationEnds.Count} termination ends.");

            Vector2Int startCellPosition = new Vector2Int(mapData.Width / 2, mapData.Height / 2);
            var startCellData = cellBaseList.Find(cell => cell.OpenDirections == (Direction.North | Direction.East | Direction.South | Direction.West));

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

                    int distanceToEdge = GetDistanceToEdge(candidatePosition);
                    float terminationChance = 0;

                    if (distanceToEdge <= 3)
                        terminationChance += (3.0f - distanceToEdge) * .34f;

                    if (random.NextDouble() > distanceToEdge)
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

                        allVariants.Shuffle(random);
                        foreach (var variant in allVariants)
                        {
                            bool hasRequired = (variant.RotatedDirections & requiredOpenings) == requiredOpenings;
                            bool hasNoForbidden = (variant.RotatedDirections & forbiddenOpenings) == 0;

                            if (hasRequired & hasNoForbidden)
                            {
                                mapData.Cells[candidatePosition.y, candidatePosition.x] = new CellData
                                {
                                    X = candidatePosition.x,
                                    Y = candidatePosition.y,
                                    Data = variant.Data,
                                    Orientation = variant.RotationSteps
                                };

                                break;
                            }
                        }

                        cellsToPopulate.Push(candidatePosition);
                    }
                }
            }
        }

        // --- HELPER METHODS ---
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
            return cellRef.IsEmpty();
        }
    }
}