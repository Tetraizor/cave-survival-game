using UnityEngine;

namespace CaveTogether.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "Cave Together/Item")]
    public class ItemSO : ScriptableObject
    {
        public Sprite Icon;
        public ItemType Type;
    }
}