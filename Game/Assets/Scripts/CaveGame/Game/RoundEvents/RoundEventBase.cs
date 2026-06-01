using System.Collections;
using CaveTogether.Game.Entities;

namespace CaveTogether.Game.RoundEvents
{
    public abstract class RoundEventBase
    {
        public abstract string BuildupMessage();
        public abstract string AnnouncementMessage();

        public virtual bool CanHappen() => true;

        public virtual IEnumerator OnRoundPassed(CharacterManager characterManager) { yield break; }

        public abstract IEnumerator Execute(int round, CharacterManager characterManager);
    }
}
