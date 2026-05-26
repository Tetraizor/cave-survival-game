using UnityEngine;

namespace CaveTogether.Generation.Features
{
    public class ExitFeature : IMapFeature
    {
        public Vector2Int ExitPosition { get; private set; }

        public ExitFeature(Vector2Int exitPosition) => ExitPosition = exitPosition;
    }
}