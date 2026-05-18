using System;
using System.Collections;
using CaveTogether.Common;
using CaveTogether.Game.Turn;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.Minigames
{
    public class MinigameManager : NetworkBehaviour
    {
        private TurnManager _turnManager;

        public void Initialize(GameConfig config)
        {
            _turnManager = FindAnyObjectByType<TurnManager>();
            _turnManager.RoundEnded += OnRoundEnded;
        }

        private void OnRoundEnded(int round)
        {
            if (!IsServer) return;

            StartCoroutine(Placeholder_StartNextRoundAfterDelay());
        }

        private IEnumerator Placeholder_StartNextRoundAfterDelay()
        {
            for (int i = 1; i <= 3; i++)
            {
                Debug.Log($"Waiting for minigame to end... {i}s");
                yield return new WaitForSeconds(1);
            }

            _turnManager.StartRoundRpc();
        }
    }
}