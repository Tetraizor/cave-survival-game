namespace CaveTogether.Generation.Layers
{
    public abstract class MapGenerationLayerBase
    {
        public abstract void Process(MapData mapData, System.Random random);
    }
}