using System.Diagnostics;

namespace CaveTogether.Generation
{
    public abstract class MapGenerationLayerBase
    {
        public abstract void Process(MapData mapData, System.Random random);
    }
}