using CaveTogether.Game.States;
using Unity.Netcode;

namespace CaveTogether.Minigames
{
    public class MiniGameState : NetworkBehaviour, IGameState
    {
        public GameStateType Type => GameStateType.MiniGame;

        public void Enter() => FindAnyObjectByType<MinigameManager>().OnMiniGameStateEntered();
        public void Exit() { }
    }
}