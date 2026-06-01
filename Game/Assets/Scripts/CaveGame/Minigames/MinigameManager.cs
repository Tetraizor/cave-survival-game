using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common;
using CaveTogether.Game;
using CaveTogether.Game.RoundEvents;
using CaveTogether.Generation;
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
        private List<RoundEventBase> _roundEvents = new();
        private RoundEventBase _currentEvent;

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

            var mapManager = FindAnyObjectByType<MapManager>();
            if (mapManager != null)
                foreach (var provider in mapManager.Generator.GetRoundEventProviders())
                    _roundEvents.AddRange(provider.GetRoundEvents());

            Debug.Log($"Registered {_roundEvents.Count} round events.");
            Debug.Log($"Registered {_minigames.Length} minigames.");
        }

        public void OnMiniGameStateEntered()
        {
            MinigameBegan?.Invoke();

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnNetworkSceneLoaded;

            if (!IsServer) return;
            _sceneManagerService.LoadScene(_current.SceneName, LoadSceneMode.Additive);
        }

        private void OnRoundEnded(int round)
        {
            foreach (var evt in _roundEvents)
                StartCoroutine(evt.OnRoundPassed(_characterManager));

            var eligibleEvents = _roundEvents.FindAll(e => e.CanHappen());
            int total = _minigames.Length + eligibleEvents.Count;
            if (total == 0) return;

            int index = new System.Random(round).Next(0, total);

            if (index < _minigames.Length)
            {
                _current = _minigames[index];
                _currentEvent = null;
                if (!IsServer) return;
                StartCoroutine(MinigameIntroSequence());
            }
            else
            {
                _currentEvent = eligibleEvents[index - _minigames.Length];
                _current = null;
                if (!IsServer) return;
                StartCoroutine(RoundEventSequence());
            }
        }

        private IEnumerator RoundEventSequence()
        {
            PushNotificationRpc(_currentEvent.BuildupMessage());
            yield return _waitBuildup;

            PushNotificationRpc(_currentEvent.AnnouncementMessage());

            int index = _roundEvents.IndexOf(_currentEvent);
            TriggerRoundEventOnClientsRpc(index, _turnManager.CurrentRound);

            yield return StartCoroutine(_currentEvent.Execute(_turnManager.CurrentRound, _characterManager));

            _gameStateManager.SwitchStateRpc(GameStateType.Game);
        }

        [Rpc(SendTo.NotServer, InvokePermission = RpcInvokePermission.Server)]
        private void TriggerRoundEventOnClientsRpc(int eventIndex, int round)
        {
            StartCoroutine(_roundEvents[eventIndex].Execute(round, _characterManager));
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

        public void PushNotification(string message) => PushNotificationRpc(message);

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PushNotificationRpc(FixedString128Bytes message)
        {
            ServiceLocator.Get<GameNotificationUI>().Push(message.ToString());
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

            var contestants = _characterManager.Characters.Where(c => !c.IsDown && !c.IsEscaped).Select(c => c.OwnerClientId).ToList();
            _activeMinigame = FindAnyObjectByType<MinigameBase>();

            var context = new MinigameContext
            {
                Config = _config,
                Round = _turnManager.CurrentRound,
                Players = contestants
            };

            _activeMinigame.Initialize(context);

            if (IsServer)
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