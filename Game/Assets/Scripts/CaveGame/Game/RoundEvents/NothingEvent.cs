using System.Collections;
using CaveTogether.Game.Entities;
using UnityEngine;

namespace CaveTogether.Game.RoundEvents
{
    public class NothingEvent : RoundEventBase
    {
        public override string AnnouncementMessage() => "The cave holds its breath.";

        public override string BuildupMessage() => "Something stirs in the dark...";

        public override IEnumerator Execute(int round, CharacterManager characterManager)
        {
            yield return new WaitForSeconds(1);
        }
    }
}