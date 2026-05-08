using Unity.Netcode;
using UnityEngine;

namespace CaveGame.Game.States
{
    public class GameState : NetworkBehaviour, IGameState
    {
        public GameStateType Type => GameStateType.Game;

        public void Enter()
        {
            Debug.Log("Game begins");
        }

        public void Exit() { }
    }
}