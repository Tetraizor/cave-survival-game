using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveTogether.Minigames.Earthquake
{
    public class EarthquakeMinigame : MinigameBase
    {
        private List<ulong> _players;

        public override void Initialize(MinigameContext context)
        {
            _players = new List<ulong>(context.PlayerOrder);
            StartCoroutine(RandomOutcomeAfterDelay());
        }

        private IEnumerator RandomOutcomeAfterDelay()
        {
            yield return new WaitForSeconds(5);

            for (int i = _players.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_players[i], _players[j]) = (_players[j], _players[i]);
            }

            int count = _players.Count;
            var result = new MinigameResult
            {
                PlayerRanking = _players.ToArray(),
                HealthDeltas = new int[count],
            };

            if (count > 1) result.HealthDeltas[count - 1] = -1;

            RaiseCompleted(result);
        }
    }
}
