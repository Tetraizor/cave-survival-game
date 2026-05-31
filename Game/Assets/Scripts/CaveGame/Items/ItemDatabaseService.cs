using System;
using System.Collections.Generic;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Items
{
    public class ItemDatabaseService : MonoBehaviour, IService
    {
        private const string ItemDataPath = "Data/Items";

        private Dictionary<ItemType, ItemSO> _itemDataLookup;
        private Dictionary<ItemType, Func<ItemBase>> _factories;

        private void Awake()
        {
            ServiceLocator.Register<ItemDatabaseService>(this);
            DontDestroyOnLoad(this);

            _itemDataLookup = new Dictionary<ItemType, ItemSO>();
            foreach (var so in Resources.LoadAll<ItemSO>(ItemDataPath))
                _itemDataLookup[so.Type] = so;

            _factories = new Dictionary<ItemType, Func<ItemBase>>
            {
                { ItemType.MedKit, () => new MedKit() },
            };
        }

        public ItemBase CreateItem(ItemType type) =>
            _factories.TryGetValue(type, out var factory) ? factory() : null;

        public ItemSO GetSO(ItemType type) =>
            _itemDataLookup.TryGetValue(type, out var so) ? so : null;
    }
}
