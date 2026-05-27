using System.Collections.Generic;
using CaveTogether.Common;

namespace CaveTogether.Minigames
{
    public struct MinigameContext
    {
        public GameConfig Config;
        public List<ulong> PlayerOrder;
        public int Round;
    }
}