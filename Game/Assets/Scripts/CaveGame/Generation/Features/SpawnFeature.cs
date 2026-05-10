using UnityEngine;

namespace CaveTogether.Generation.Features
{
    public class SpawnFeature : IMapFeature
    {
        public Vector2Int PlayerSpawnPosition { get; private set; }

        public SpawnFeature(Vector2Int spawnPoint)
        {
            PlayerSpawnPosition = spawnPoint;
        }
    }
}