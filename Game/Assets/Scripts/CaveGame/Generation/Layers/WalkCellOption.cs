using System;
using System.Collections;
using CaveTogether.Game.Entities;

namespace CaveTogether.Generation.Layers
{
    public class WalkCellOption
    {
        public string Title;
        public int ExtraEnergyCost;
        public Func<Character, IEnumerator> OnEnter;
    }
}
