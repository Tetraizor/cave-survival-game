using System;
using System.Collections.Generic;
using CaveGame.Common;
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
            if (flowService.CurrentMatchPayload.HasValue)
            {
                StartGenerationRpc(flowService.CurrentMatchPayload.Value);
            }
            else
            {
                Debug.LogError("[StartState] Cannot start a game session without a payload!");
            }

            string randomSeed = UnityEngine.Random.Range(1000000, 9999999).ToString();
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