namespace CaveGame.Game.States
{
    public interface IGameState
    {
        public GameStateType Type { get; }

        public void Enter();
        public void Exit();
    }
}