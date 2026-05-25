using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Generation;

namespace CaveTogether.Game.Actions
{
    public abstract class GameActionBase
    {
        public abstract ActionType Type { get; }
        public abstract ActionUIType UIType { get; }
        public abstract string DisplayName { get; }

        public abstract int GetEnergyCost(MapData map, Character character, ActionRequest request);

        public abstract bool IsValid(MapData map, Character character, ActionRequest request);
        public abstract void Execute(MapData map, Character character, ActionRequest request);
    }
}