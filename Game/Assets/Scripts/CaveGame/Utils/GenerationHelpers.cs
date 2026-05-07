using CaveGame.Common.Enums;
using UnityEngine;

namespace CaveGame.Utils
{
    public static class GenerationHelpers
    {
        public static Vector2Int GetPositionAtDirection(this Vector2Int self, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Vector2Int(self.x, self.y + 1);

                case Direction.East:
                    return new Vector2Int(self.x + 1, self.y);

                case Direction.South:
                    return new Vector2Int(self.x, self.y - 1);

                case Direction.West:
                    return new Vector2Int(self.x - 1, self.y);

                default:
                case Direction.None:
                    return self;
            }
        }

        public static Direction GetRandomDirection(System.Random rand = null)
        {
            int direction = 1 << rand.Next(0, 4);
            return (Direction)direction;
        }
    }
}