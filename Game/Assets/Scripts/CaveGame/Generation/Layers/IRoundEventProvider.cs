using System.Collections.Generic;
using CaveTogether.Game.RoundEvents;

namespace CaveTogether.Generation.Layers
{
    public interface IRoundEventProvider
    {
        IEnumerable<RoundEventBase> GetRoundEvents();
    }
}
