using System;
using CaveTogether.Common.Enums;
using CaveTogether.Generation.Features;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    public class SpawnGenerationLayer : MapGenerationLayerBase
    {
        public override void Process(MapData mapData, System.Random random)
        {
            var centerPoint = new Vector2Int(mapData.Width / 2, mapData.Height / 2);
            ref CellData centerCellRef = ref mapData.GetCellRef(centerPoint.x, centerPoint.y);

            if (centerCellRef.IsEmpty || centerCellRef.Data.OpenDirections != (Direction.North | Direction.East | Direction.South | Direction.West))
                throw new Exception("[SpawnGenerationLayer] Cannot find a valid spawn point!");

            var spawnFeature = new SpawnFeature(centerPoint);

            mapData.AddFeature(spawnFeature);
        }
    }
}