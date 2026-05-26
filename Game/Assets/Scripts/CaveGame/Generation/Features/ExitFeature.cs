using UnityEngine;

namespace CaveTogether.Generation.Features
{
    public class ExitFeature : IMapFeature
    {
        public Vector2Int ExitPosition { get; }

        public ExitFeature(Vector2Int exitPosition)
        {
            ExitPosition = exitPosition;
        }
    }
}
