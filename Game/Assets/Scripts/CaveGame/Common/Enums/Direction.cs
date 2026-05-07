using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CaveGame.Common.Enums
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

    [Flags]
    public enum Direction
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3,
    }

    public static class DirectionHelpers
    {
        public static List<Direction> DirectionFlagToList(Direction direction)
        {
            List<Direction> list = new();

            if ((direction & Direction.North) > 0) list.Add(Direction.North);
            if ((direction & Direction.East) > 0) list.Add(Direction.East);
            if ((direction & Direction.South) > 0) list.Add(Direction.South);
            if ((direction & Direction.West) > 0) list.Add(Direction.West);

            return list;
        }

        public static Direction RotateDirectionsClockwise(Direction dir, int steps)
        {
            Direction current = dir;
            for (int i = 0; i < steps; i++)
            {
                Direction next = Direction.None;
                if ((current & Direction.North) != 0) next |= Direction.East;
                if ((current & Direction.East) != 0) next |= Direction.South;
                if ((current & Direction.South) != 0) next |= Direction.West;
                if ((current & Direction.West) != 0) next |= Direction.North;
                current = next;
            }
            return current;
        }

        public static Direction GetOppositeDirection(Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                Direction.West => Direction.East,
                _ => Direction.None,
            };
        }

        public static Vector2Int AddDirectionToPosition(Vector2Int position, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    position.y += 1;
                    break;
                case Direction.East:
                    position.x += 1;
                    break;
                case Direction.South:
                    position.y -= 1;
                    break;
                case Direction.West:
                    position.x -= 1;
                    break;
                default:
                    break;
            }

            return position;
        }

        public static Vector2Int ToVector(this Direction direction)
        {
            return direction switch
            {
                Direction.North => new Vector2Int(0, 1),
                Direction.East => new Vector2Int(1, 0),
                Direction.South => new Vector2Int(0, -1),
                Direction.West => new Vector2Int(-1, 0),
                _ => new Vector2Int()
            };
        }
    }
}