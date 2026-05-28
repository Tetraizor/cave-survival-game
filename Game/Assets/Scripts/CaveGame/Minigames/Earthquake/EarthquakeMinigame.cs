using System.Collections;
using System.Collections.Generic;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Minigames.Earthquake
{
    public class EarthquakeMinigame : MinigameBase
    {
        private List<ulong> _players;
        private MinigameResult _result;

        public override void Initialize(MinigameContext context)
        {
            _players = new List<ulong>(context.PlayerOrder);
            StartCoroutine(RandomOutcomeAfterDelay());
        }

        private IEnumerator RandomOutcomeAfterDelay()
        {
            ServiceLocator.Get<TransitionService>().StartTransition(false);

            yield return new WaitForSeconds(5);

            for (int i = _players.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_players[i], _players[j]) = (_players[j], _players[i]);
            }

            int count = _players.Count;
            _result = new MinigameResult
            {
                PlayerRanking = _players.ToArray(),
                HealthDeltas = new int[count],
            };

            if (count > 1) _result.HealthDeltas[count - 1] = -1;

            ServiceLocator.Get<TransitionService>().StartTransition(true);
            ServiceLocator.Get<TransitionService>().TransitionCompleted += GameEnd_OnTransitionCompleted;
        }

        private void GameEnd_OnTransitionCompleted()
        {
            ServiceLocator.Get<TransitionService>().TransitionCompleted -= GameEnd_OnTransitionCompleted;
            RaiseCompleted(_result);
        }
    }
}
