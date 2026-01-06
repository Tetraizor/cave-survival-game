using System.Diagnostics;

namespace CaveGame.Generation
{
    public abstract class MapGenerationLayerBase
    {
        public abstract void Process(MapData mapData, System.Random random);
    }
}