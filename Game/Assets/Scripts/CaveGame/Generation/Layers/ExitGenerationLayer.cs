using System.Collections.Generic;
using CaveTogether.Game.Movement;
using CaveTogether.Generation.Features;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public class ExitGenerationLayer : MapGenerationLayerBase
    {
        public override void Process(MapData mapData, System.Random random)
        {
            List<(Vector2Int, int)> distanceByPositionList = new();
            var spawnPosition = mapData.GetFeature<SpawnFeature>().PlayerSpawnPosition;

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    var cellRef = mapData.GetCellRef(x, y);

                    if (cellRef.IsEmpty) continue;

                    int distance = MovementValidator.GetDistance(mapData, spawnPosition, new Vector2Int(x, y));

                    distanceByPositionList.Add((new Vector2Int(x, y), distance));
                }
            }

            distanceByPositionList.Sort((p1, p2) => p1.Item2 - p2.Item2);
            var exitPosition = distanceByPositionList[random.Next(0, (int)(distanceByPositionList.Count * .1f))].Item1;

            var exitFeature = new ExitFeature(exitPosition);

            mapData.AddFeature(exitFeature);
        }
    }
}