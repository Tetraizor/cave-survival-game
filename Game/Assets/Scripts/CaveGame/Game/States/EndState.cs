using CaveTogether.Game.States;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.States
{
    public class EndState : NetworkBehaviour, IGameState
    {
        public GameStateType Type => GameStateType.End;

        public void Enter()
        {
            Debug.Log("Game Ended");
        }

        public void Exit() { }
    }
}