using System;
using UnityEditor;
using UnityEngine;

namespace CaveGame.CommonEnums
{
    [CustomPropertyDrawer(typeof(Direction))]
    public class DirectionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. Get the current value
            int currentValue = property.intValue;

            // 2. Use MaskField but with your specific names
            // This creates a dropdown that only shows your 4 directions
            string[] names = { "North", "East", "South", "West" };
            int[] values = { 1, 2, 4, 8 };

            // We convert the bitmask to an index-based mask for the field
            int mask = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if ((currentValue & values[i]) != 0) mask |= (1 << i);
            }

            // 3. Draw the field
            mask = EditorGUI.MaskField(position, label, mask, names);

            // 4. Convert the index-based mask back to your enum bitmask
            int finalValue = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if ((mask & (1 << i)) != 0) finalValue |= values[i];
            }

            // 5. Save it back
            property.intValue = finalValue;
        }
    }

    [Flags]
    public enum Direction
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3,
    }
}