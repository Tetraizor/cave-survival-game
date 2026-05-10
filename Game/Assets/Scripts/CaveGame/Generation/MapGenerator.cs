using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

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
            Seed = seed;
            Map = map;
        }

        public void GenerateMap()
        {
            var generationEnumerator = MapGenerationPipeline.GetEnumerator();

            var hasher = MD5.Create();
            var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(Seed));

            int seedHashed = BitConverter.ToInt32(hashed, 0);
            System.Random random = new System.Random(seedHashed);

            while (generationEnumerator.MoveNext())
            {
                generationEnumerator.Current.Process(Map, random);
                GenerationLayerFinished?.Invoke(generationEnumerator.Current);
            }
        }
    }
}