using CaveTogether.Common.Enums;
using UnityEngine;

namespace CaveTogether.Game.Entities
{
    [CreateAssetMenu(fileName = "Character", menuName = "Cave Together/Character", order = 1)]
    public class CharacterDataSO : ScriptableObject
    {
        public string TypeId;
        public string Name;

        public int MaxHealth = 4;
        public int MaxEnergy = 4;

        public Sprite HeadGraphic;

        public ActionType[] PossibleActions = new ActionType[] {
            ActionType.Wait,
            ActionType.Walk,
            ActionType.EndTurn,
            ActionType.Inspect,
        };
    }
}