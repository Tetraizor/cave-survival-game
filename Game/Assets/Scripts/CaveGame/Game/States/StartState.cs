using System;
using System.Collections.Generic;
using CaveGame.Common;
using CaveGame.Game.UI;
using CaveGame.Generation;
using CaveGame.Services;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveGame.Game.States
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

            SetupPlayersRpc(flowService.CurrentMatchPayload.Value);
            StartGenerationRpc(flowService.CurrentMatchPayload.Value);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void SetupPlayersRpc(GameConfig config)
        {
            FindAnyObjectByType<GameUIManager>().Initialize(config);
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

            if (IsServer)
                FindAnyObjectByType<GameStateManager>().SwitchStateRpc(GameStateType.Game);
        }
    }
}