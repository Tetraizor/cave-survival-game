using System;
using UnityEngine;

namespace CaveTogether.Items
{
    public class Inventory
    {
        public const int Size = 5;

        public event Action InventoryChanged;

        private readonly ItemInstance[] _slots = new ItemInstance[Size];

        public bool HasItem(int slot) => !_slots[slot].IsEmpty;
        public ItemType GetItemType(int slot) => _slots[slot].Type;

        public bool AddItem(ItemType type)
        {
            for (int i = 0; i < Size; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _slots[i] = new ItemInstance(type);
                    InventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(int slot)
        {
            _slots[slot] = default;
            InventoryChanged?.Invoke();
        }
    }
}
