using System;
using System.Collections;
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
        [SerializeField] private float _transitionDuration = 1f;
        [SerializeField] private float _returnTransitionDuration = 1f;

        private TurnManager _turnManager;
        private GameStateManager _gameStateManager;
        private CharacterManager _characterManager;
        private SceneManagerService _sceneManagerService;
        private GameConfig _config;

        private MinigameDefinitionSO _current;
        private MinigameBase _activeMinigame;

        private static readonly WaitForSeconds _waitBuildup = new(2f);
        private static readonly WaitForSeconds _waitAnnouncement = new(1.5f);

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
            StartCoroutine(MinigameIntroSequence());
        }

        private IEnumerator MinigameIntroSequence()
        {
            PushNotificationRpc(_current.BuildupMessage);
            yield return _waitBuildup;

            PushNotificationRpc(_current.AnnouncementMessage);
            ShakeCameraRpc();
            yield return _waitAnnouncement;

            PlayMinigameTransitionRpc();
            yield return new WaitForSeconds(_transitionDuration);

            _gameStateManager.SwitchStateRpc(GameStateType.MiniGame);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PushNotificationRpc(FixedString128Bytes message)
        {
            FindAnyObjectByType<GameNotificationUI>().Push(message.ToString());
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void ShakeCameraRpc()
        {
            var cm = FindAnyObjectByType<CameraManager>();
            if (cm != null) cm.Shake(2);
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
            StartCoroutine(GameReturnSequence());
        }

        private IEnumerator GameReturnSequence()
        {
            NotifyMinigameEndedRpc();
            PlayGameReturnTransitionRpc();
            yield return new WaitForSeconds(_returnTransitionDuration);
            _gameStateManager.SwitchStateRpc(GameStateType.Game);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void NotifyMinigameEndedRpc()
        {
            MinigameEnded?.Invoke();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PlayMinigameTransitionRpc()
        {
            var cm = FindAnyObjectByType<CameraManager>();
            cm.FocusOn(_characterManager.GetClientCharacter().transform.position, 0.1f);

            var tm = ServiceLocator.Get<TransitionService>();

            tm.StartTransition(true);
            tm.TransitionCompleted += MinigameStart_OnTransitionCompleted;
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PlayGameReturnTransitionRpc()
        {
            var cm = FindAnyObjectByType<CameraManager>();
            cm.FocusOn(_characterManager.GetClientCharacter().transform.position);

            var tm = ServiceLocator.Get<TransitionService>();

            tm.StartTransition(false);
            tm.TransitionCompleted += MinigameEnd_OnTransitionCompleted;
        }

        private void MinigameStart_OnTransitionCompleted()
        {
            var tm = ServiceLocator.Get<TransitionService>();
            tm.TransitionCompleted -= MinigameStart_OnTransitionCompleted;
        }

        private void MinigameEnd_OnTransitionCompleted()
        {
            var tm = ServiceLocator.Get<TransitionService>();
            tm.TransitionCompleted -= MinigameEnd_OnTransitionCompleted;
        }
    }
}