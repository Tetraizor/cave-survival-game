namespace CaveTogether.Items
{
    public struct ItemInstance
    {
        public ItemType Type;

        public bool IsEmpty => Type == ItemType.None;

        public ItemInstance(ItemType type)
        {
            Type = type;
        }
    }
}
