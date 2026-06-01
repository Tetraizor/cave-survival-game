using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CaveTogether.Game.CellEffects;
using CaveTogether.Generation.Layers;

namespace CaveTogether.Generation
{
    public class MapGenerator
    {
        public Action<MapGenerationLayerBase> GenerationLayerFinished;

        public string Seed { get; private set; } = "";
        public MapData Map { get; private set; }

        private List<MapGenerationLayerBase> MapGenerationPipeline = new();

        public MapGenerator(MapData map, string seed = "")
        {
            MapGenerationPipeline.Add(new BaseGenerationLayer());
            MapGenerationPipeline.Add(new SpawnGenerationLayer());
            MapGenerationPipeline.Add(new ExitGenerationLayer());
            MapGenerationPipeline.Add(new VineGenerationLayer());
            MapGenerationPipeline.Add(new GasVentGenerationLayer());
            MapGenerationPipeline.Add(new LootGenerationLayer());

            Seed = seed;
            Map = map;
        }

        public T GetLayer<T>() where T : MapGenerationLayerBase =>
            MapGenerationPipeline.OfType<T>().FirstOrDefault();

        public IEnumerable<ICellCostModifier> GetCostModifiers() =>
            MapGenerationPipeline.OfType<ICellCostModifier>();

        public IEnumerable<ICellEffectProvider> GetEffectProviders() =>
            MapGenerationPipeline.OfType<ICellEffectProvider>();

        public IEnumerable<IRoundEventProvider> GetRoundEventProviders() =>
            MapGenerationPipeline.OfType<IRoundEventProvider>();

        public IEnumerable<ICellWalkOptionsProvider> GetWalkOptionProviders() =>
            MapGenerationPipeline.OfType<ICellWalkOptionsProvider>();

        public void GenerateMap()
        {
            var generationEnumerator = MapGenerationPipeline.GetEnumerator();

            var hasher = MD5.Create();
            var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(Seed));

            int seedHashed = BitConverter.ToInt32(hashed, 0);
            Random random = new Random(seedHashed);

            while (generationEnumerator.MoveNext())
            {
                generationEnumerator.Current.Process(Map, random);
                GenerationLayerFinished?.Invoke(generationEnumerator.Current);
            }
        }
    }
}