using CaveTogether.Common.Enums;
using UnityEditor;
using UnityEngine;

namespace CaveTogether.Enums.Editor
{
    [CustomPropertyDrawer(typeof(Direction))]
    public class DirectionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int currentValue = property.intValue;

            string[] names = { "North", "East", "South", "West" };
            int[] values = { 1, 2, 4, 8 };

            int mask = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if ((currentValue & values[i]) != 0) mask |= (1 << i);
            }

            mask = EditorGUI.MaskField(position, label, mask, names);

            int finalValue = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if ((mask & (1 << i)) != 0) finalValue |= values[i];
            }

            property.intValue = finalValue;
        }
    }
}