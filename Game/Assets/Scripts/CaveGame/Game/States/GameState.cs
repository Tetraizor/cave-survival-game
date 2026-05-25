using CaveTogether.Game.Actions;
using CaveTogether.Game.Turn;
using CaveTogether.Generation;
using Unity.Netcode;

namespace CaveTogether.Game.States
{
    public class GameState : NetworkBehaviour, IGameState
    {
        public GameStateType Type => GameStateType.Game;

        public void Enter()
        {
            var mapManager = FindAnyObjectByType<MapManager>();

            var cursorManager = FindAnyObjectByType<CursorManager>();
            var cameraManager = FindAnyObjectByType<CameraManager>();
            var mapActionManager = FindAnyObjectByType<MapActionManager>();
            var actionManager = FindAnyObjectByType<ActionManager>();

            cursorManager.Initialize(mapManager.Map);
            cameraManager.Initialize();
            actionManager.Initialize();
            mapActionManager.Initialize();

            if (IsServer) FindAnyObjectByType<TurnManager>().StartRoundRpc();
        }

        public void Exit()
        {
            var actionManager = FindAnyObjectByType<ActionManager>();
            var mapActionManager = FindAnyObjectByType<MapActionManager>();
            var cameraManager = FindAnyObjectByType<CameraManager>();
            var cursorManager = FindAnyObjectByType<CursorManager>();

            mapActionManager.Deinitialize();
            actionManager.Deinitialize();
            cameraManager.Deinitialize();
            cursorManager.Deinitialize();
        }
    }
}