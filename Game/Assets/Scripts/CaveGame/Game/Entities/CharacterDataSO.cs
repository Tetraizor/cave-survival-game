using System;
using CaveTogether.Common.Enums;
using CaveTogether.Items;
using UnityEngine;

namespace CaveTogether.Game.Entities
{
    [Serializable]
    public struct CharacterDataSpriteSet
    {
        public Sprite Head;
        public Sprite Body;
        public Sprite UpperArmF;
        public Sprite LowerArmF;
        public Sprite UpperArmB;
        public Sprite LowerArmB;
        public Sprite UpperLegF;
        public Sprite LowerLegF;
        public Sprite UpperLegB;
        public Sprite LowerLegB;
        public Sprite FootF;
        public Sprite FootB;
    }

    [CreateAssetMenu(fileName = "Character", menuName = "Cave Together/Character", order = 1)]
    public class CharacterDataSO : ScriptableObject
    {
        public string TypeId;
        public string Name;

        public int MaxHealth = 4;
        public int MaxEnergy = 4;

        public CharacterDataSpriteSet SpriteSet;
        public Sprite Portrait;

        public ItemType[] StartingItemTypes = new ItemType[0];

        public ActionType[] PossibleActionTypes = new ActionType[] {
            ActionType.Wait,
            ActionType.Walk,
            ActionType.EndTurn,
            ActionType.Inspect,
            ActionType.UseItem,
            ActionType.Pickup,
        };
    }
}