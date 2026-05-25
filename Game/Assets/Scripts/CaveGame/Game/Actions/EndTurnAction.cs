using System.Collections;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Turn;
using CaveTogether.Generation;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game.Actions
{
    public class EndTurnAction : GameActionBase
    {
        public override ActionType Type => ActionType.EndTurn;
        public override ActionUIType UIType => ActionUIType.ActionBarGlobal;
        public override string DisplayName => "End Turn";

        public override int GetEnergyCost(MapData map, Character character, ActionRequest request) => character.Energy;
        public override bool IsValid(MapData map, Character character, ActionRequest request) => true;

        public override IEnumerator Execute(MapData map, Character character, ActionRequest request) { yield break; }
    }
}