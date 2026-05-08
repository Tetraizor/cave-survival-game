using System;
using System.Collections.Generic;
using CaveGame.Generation;
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
            string randomSeed = UnityEngine.Random.Range(1000000, 9999999).ToString();
            StartGenerationRpc(randomSeed);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void StartGenerationRpc(string seed)
        {
            var mapManager = FindAnyObjectByType<MapManager>();

            if (mapManager == null)
            {
                Debug.LogError("[StartState] Coult not find MapManager in the scene!");
                return;
            }

            mapManager.Initialize(seed);

            if (IsServer)
            {
                FindAnyObjectByType<GameStateManager>().SwitchStateRpc(GameStateType.Game);
            }
        }
    }
}