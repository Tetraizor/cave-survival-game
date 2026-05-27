using System;
using System.Collections.Generic;
using CaveTogether.Common;
using CaveTogether.Game;
using CaveTogether.Game.Entities;
using CaveTogether.Game.States;
using CaveTogether.Game.Turn;
using CaveTogether.Game.UI;
using CaveTogether.Services;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveTogether.Minigames
{
    public class MinigameManager : NetworkBehaviour
    {
        public event Action MinigameBegan;
        public event Action MinigameEnded;

        [SerializeField] private MinigameDefinitionSO[] _minigames;

        private TurnManager _turnManager;
        private GameStateManager _gameStateManager;
        private CharacterManager _characterManager;
        private SceneManagerService _sceneManagerService;
        private GameConfig _config;

        private MinigameDefinitionSO _current;
        private MinigameBase _activeMinigame;

        public void Initialize(GameConfig config)
        {
            _config = config;
            _turnManager = FindAnyObjectByType<TurnManager>();
            _gameStateManager = FindAnyObjectByType<GameStateManager>();
            _characterManager = FindAnyObjectByType<CharacterManager>();
            _sceneManagerService = ServiceLocator.Get<SceneManagerService>();
            _turnManager.RoundEnded += OnRoundEnded;
        }

        public void OnMiniGameStateEntered()
        {
            MinigameBegan?.Invoke();

            if (!IsServer) return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnNetworkSceneLoaded;
            _sceneManagerService.LoadScene(_current.SceneName, LoadSceneMode.Additive);
        }

        private void OnRoundEnded(int round)
        {
            if (!IsServer) return;

            _current = _minigames[(round - 1) % _minigames.Length];
            NotifyMinigameStartingRpc(_current.DisplayName);
            _gameStateManager.SwitchStateRpc(GameStateType.MiniGame);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void NotifyMinigameStartingRpc(FixedString64Bytes name)
        {
            FindAnyObjectByType<GameNotificationUI>().Push($"Minigame: {name}");
        }

        private void OnNetworkSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (loadSceneMode != LoadSceneMode.Additive || sceneName != _current.SceneName) return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnNetworkSceneLoaded;

            _activeMinigame = FindAnyObjectByType<MinigameBase>();
            var context = new MinigameContext
            {
                Config = _config,
                PlayerOrder = _turnManager.TurnOrder,
                Round = _turnManager.CurrentRound,
            };
            _activeMinigame.Initialize(context);
            _activeMinigame.Completed += OnMinigameCompleted;
        }

        private void OnMinigameCompleted(MinigameResult result)
        {
            if (!IsServer) return;

            _activeMinigame.Completed -= OnMinigameCompleted;
            _activeMinigame = null;

            ApplyMinigameResultRpc(result);

            NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnNetworkSceneUnloaded;
            _sceneManagerService.UnloadAdditiveScene(_current.SceneName);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ApplyMinigameResultRpc(MinigameResult result)
        {
            for (int i = 0; i < result.PlayerRanking.Length; i++)
            {
                var character = _characterManager.GetCharacter(result.PlayerRanking[i]);
                if (character == null) continue;

                int hd = result.HealthDeltas[i];
                if (hd < 0) character.TakeDamage(-hd);
                else if (hd > 0) character.Heal(hd);

            }
        }

        private void OnNetworkSceneUnloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (sceneName != _current.SceneName) return;

            NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= OnNetworkSceneUnloaded;

            NotifyMinigameEndedRpc();
            _gameStateManager.SwitchStateRpc(GameStateType.Game);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void NotifyMinigameEndedRpc()
        {
            MinigameEnded?.Invoke();
        }
    }
}