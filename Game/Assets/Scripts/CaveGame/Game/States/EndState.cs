using System.Collections;
using CaveTogether.Game.Entities;
using CaveTogether.Game.States;
using CaveTogether.Game.UI;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.States
{
    public class EndState : NetworkBehaviour, IGameState
    {
        public GameStateType Type => GameStateType.End;

        public void Enter()
        {
            StartCoroutine(ExitSequence());
        }

        private IEnumerator ExitSequence()
        {
            var clientCharacter = FindAnyObjectByType<CharacterManager>().GetClientCharacter();

            FindAnyObjectByType<GameNotificationUI>().Push("Game over.");
            yield return new WaitForSeconds(2);

            FindAnyObjectByType<GameNotificationUI>().Push(clientCharacter.IsEscaped ? "You managed to escape." : "You're left at the caves forever.");

            yield break;
        }

        public void Exit() { }
    }
}