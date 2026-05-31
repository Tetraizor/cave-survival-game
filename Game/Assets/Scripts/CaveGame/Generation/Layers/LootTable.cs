using System;
using CaveTogether.Items;
using UnityEngine;

namespace CaveTogether.Generation.Layers
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "Cave Together/Generation/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [Serializable]
        public struct LootEntry
        {
            public ItemType Item;
            [Range(0f, 1f)] public float Chance;
        }

        public LootEntry[] Entries;

        public ItemType Roll(System.Random random)
        {
            foreach (var entry in Entries)
                if (random.NextDouble() < entry.Chance)
                    return entry.Item;
            return ItemType.None;
        }
    }
}
