using CaveTogether.Game.Actions;
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

            cursorManager.Initialize(mapManager.Map);
            cameraManager.Initialize();

            FindAnyObjectByType<ActionManager>().Initialize();
        }

        public void Exit()
        {
            var cursorManager = FindAnyObjectByType<CursorManager>();
            var cameraManager = FindAnyObjectByType<CameraManager>();

            cursorManager.Deinitialize();
            cameraManager.Deinitialize();

            FindAnyObjectByType<ActionManager>().Deinitialize();
        }
    }
}