using System.Collections.Generic;
using CaveTogether.Common;

namespace CaveTogether.Minigames
{
    public struct MinigameContext
    {
        public int Round;
        public GameConfig Config;
        public List<ulong> Players;
    }
}