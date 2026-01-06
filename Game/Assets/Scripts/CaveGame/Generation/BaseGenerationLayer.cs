using System.Collections.Generic;
using System.Linq;
using CaveGame.CommonEnums;
using CaveGame.Utils;
using UnityEngine;

namespace CaveGame.Generation
{
    public class BaseGenerationLayer : MapGenerationLayerBase
    {
        private static string _baseCellScriptablePath = "Data/Generation/CellBase";

        private MapData _mapData;

        public override void Process(MapData mapData, System.Random random)
        {
            _mapData = mapData;

            var cellBaseList = Resources.LoadAll<CellBaseData>(_baseCellScriptablePath).ToList();

            Debug.Log($"Loaded {cellBaseList.Count} cell base scriptables.");

            Vector2Int startCellPosition = new Vector2Int(mapData.Width / 2, mapData.Height / 2);
            _mapData[startCellPosition.y, startCellPosition.x] = new CellData
            {
                X = startCellPosition.x,
                Y = startCellPosition.y,
                Orientation = 0,
                Data = cellBaseList.Find(cell =>
                    cell.OpenDirections == (Direction.North | Direction.East | Direction.South | Direction.West))
            };

            cellBaseList.ForEach(cell => Debug.Log(cell.OpenDirections));

            Stack<Vector2Int> cellsToPopulate = new();
            cellsToPopulate.Push(startCellPosition);

            while (cellsToPopulate.TryPeek(out _))
            {
                Vector2Int cellPosition = cellsToPopulate.Pop();
                ref var cellData = ref _mapData.GetCellRef(cellPosition.x, cellPosition.y);

                List<Direction> directionsToCheck = new();
                if ((cellData.Data.OpenDirections & Direction.North) != 0) directionsToCheck.Add(Direction.North);
                if ((cellData.Data.OpenDirections & Direction.East) != 0) directionsToCheck.Add(Direction.East);
                if ((cellData.Data.OpenDirections & Direction.South) != 0) directionsToCheck.Add(Direction.South);
                if ((cellData.Data.OpenDirections & Direction.West) != 0) directionsToCheck.Add(Direction.West);

                directionsToCheck.Shuffle(random);
                var directionEnumerator = directionsToCheck.GetEnumerator();

                while (directionEnumerator.MoveNext())
                {
                    Vector2Int candidatePosition = cellPosition.GetPositionAtDirection(directionEnumerator.Current);
                    bool isCellFree = IsCellFree(candidatePosition);
                    if (isCellFree)
                    {
                        Direction emptySpotsRelative = Direction.None;
                        if (IsCellFree(candidatePosition.GetPositionAtDirection(Direction.North))) emptySpotsRelative |= Direction.North;
                        if (IsCellFree(candidatePosition.GetPositionAtDirection(Direction.East))) emptySpotsRelative |= Direction.East;
                        if (IsCellFree(candidatePosition.GetPositionAtDirection(Direction.South))) emptySpotsRelative |= Direction.South;
                        if (IsCellFree(candidatePosition.GetPositionAtDirection(Direction.West))) emptySpotsRelative |= Direction.West;

                        cellBaseList.Shuffle(random);

                        var cellEnumerator = cellBaseList.GetEnumerator();
                        while (cellEnumerator.MoveNext())
                        {
                            var cellScriptable = cellEnumerator.Current;
                            if (emptySpotsRelative == (emptySpotsRelative & cellScriptable.OpenDirections))
                            {
                                mapData.Cells[candidatePosition.y, candidatePosition.x] = new CellData
                                {
                                    X = candidatePosition.x,
                                    Y = candidatePosition.y,
                                    Data = cellScriptable
                                };

                                cellsToPopulate.Push(candidatePosition);
                                break;
                            }

                        }

                        break;
                    }
                }
            }
        }

        public bool IsCellFree(Vector2Int position)
        {
            if (position.x < 0 || position.x >= _mapData.Width || position.y < 0 || position.y >= _mapData.Height) return false;

            ref CellData cellRef = ref _mapData.GetCellRef(position);
            return cellRef.IsEmpty();
        }
    }
}