using System.Collections.Generic;
using CaveTogether.Common;
using CaveTogether.Game.Actions;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Turn;
using CaveTogether.Game.UI;
using CaveTogether.Generation;
using CaveTogether.Minigames;
using CaveTogether.Services;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveTogether.Game.States
{
    public class StartState : NetworkBehaviour, IGameState
    {
        public GameStateType Type => GameStateType.Start;

        public void Enter()
        {
            if (IsServer) NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }

        public void Exit()
        {
            if (IsServer) NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            var flowService = ServiceLocator.Get<GameFlowService>();
            if (!flowService.CurrentMatchPayload.HasValue)
                Debug.LogError("[StartState] Cannot start a game session without a payload!");

            StartGenerationRpc(flowService.CurrentMatchPayload.Value);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void StartGenerationRpc(GameConfig config)
        {
            var mapManager = FindAnyObjectByType<MapManager>();

            if (mapManager == null)
            {
                Debug.LogError("[StartState] Could not find MapManager in the scene!");
                return;
            }

            mapManager.Initialize(config.Seed.ToString());

            FindAnyObjectByType<ExplorationManager>().Initialize(mapManager.Map);

            FindAnyObjectByType<CharacterManager>().Initialize(config);
            FindAnyObjectByType<ActionUIManager>().Initialize();
            FindAnyObjectByType<GameUIManager>().Initialize(config);
            FindAnyObjectByType<TurnManager>().Initialize(config);
            FindAnyObjectByType<MinigameManager>().Initialize(config);

            if (IsServer)
                FindAnyObjectByType<GameStateManager>().SwitchStateRpc(GameStateType.Game);
        }
    }
}