using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Input;
using CaveTogether.Services;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveTogether.Minigames.Earthquake
{
    public class EarthquakeMinigame : MinigameBase
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _stalagmitePrefab;

        private List<EarthquakePlayerController> _players;
        private List<ulong> _playerIds;
        private MinigameResult _result;
        private int _activeStalagmiteCount;

        public const float StalagmiteStartY = 3.6f;
        public const float GameDuration = 10f;
        public const float StalagmiteDelay = .25f;

        private static readonly WaitForSeconds _waitEndDelay = new(5f);
        public const float StalagmiteXStart = -4.5f;
        public const float StalagmiteXEnd = 4.5f;

        public override void Initialize(MinigameContext context)
        {
            ServiceLocator.Get<TransitionService>().StartTransition(false);
            ServiceLocator.Get<TransitionService>().TransitionCompleted += GameStart_OnTransitionCompleted;

            _players = new List<EarthquakePlayerController>();
            _playerIds = new List<ulong>(context.Players);

            if (!NetworkManager.Singleton.IsServer) return;

            foreach (var playerId in _playerIds)
            {
                var playerGO = Instantiate(_playerPrefab);
                SceneManager.MoveGameObjectToScene(playerGO, gameObject.scene);
                var networkObject = playerGO.GetComponent<NetworkObject>();
                networkObject.SpawnWithOwnership(playerId);

                _players.Add(playerGO.GetComponent<EarthquakePlayerController>());
            }
        }

        public void NotifyPlayerCollision(EarthquakePlayerController player)
        {
            if (IsServer) NotifyPlayerCollisionRpc(player.Character.OwnerClientId);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void NotifyPlayerCollisionRpc(ulong playerId)
        {
            var player = FindObjectsByType<EarthquakePlayerController>().ToList().Find(p => p.Character.OwnerClientId == playerId);
            player.Down();
        }

        public void NotifyStalagmiteRemoved()
        {
            _activeStalagmiteCount--;
        }

        private void GameStart_OnTransitionCompleted()
        {
            ServiceLocator.Get<TransitionService>().TransitionCompleted -= GameStart_OnTransitionCompleted;

            // Start game
            if (IsServer) StartCoroutine(GameLogic());
        }

        private void GameEnd_OnTransitionCompleted()
        {
            ServiceLocator.Get<TransitionService>().TransitionCompleted -= GameEnd_OnTransitionCompleted;
            RaiseCompleted(_result);
        }

        private IEnumerator GameLogic()
        {
            Camera.main.DOShakePosition(100, .025f, 5).SetLink(Camera.main.gameObject);

            yield return new WaitForSeconds(2);

            System.Random rand = new();
            float startTime = Time.time;

            while (Time.time - startTime < GameDuration)
            {
                int waveSize = rand.Next(1, 4);
                for (int i = 0; i < waveSize; i++)
                {
                    float x = StalagmiteXStart + (float)rand.NextDouble() * (StalagmiteXEnd - StalagmiteXStart);
                    float scale = 0.5f + (float)rand.NextDouble() * 1f;

                    var go = Instantiate(_stalagmitePrefab, new Vector3(x, StalagmiteStartY, 0), Quaternion.identity);
                    SceneManager.MoveGameObjectToScene(go, gameObject.scene);
                    go.GetComponent<NetworkObject>().SpawnWithOwnership(0);
                    go.GetComponent<StalagmiteBody>().Initialize(this, 0f, scale);
                    _activeStalagmiteCount++;
                }

                yield return new WaitForSeconds(StalagmiteDelay * (0.5f + (float)rand.NextDouble()));
            }

            StartCoroutine(EndGameSequence());
        }

        private IEnumerator EndGameSequence()
        {
            yield return new WaitUntil(() => _activeStalagmiteCount <= 0);
            Camera.main.DOKill();

            FindAnyObjectByType<MinigameManager>().PushNotification("The tremors fade... the caves grow still.");

            yield return _waitEndDelay;

            var healthDeltas = new List<int>();
            foreach (var player in _players)
            {
                healthDeltas.Add(player.IsDown ? -1 : 0);
            }

            _result = new MinigameResult
            {
                PlayerRanking = _playerIds.ToArray(),
                HealthDeltas = healthDeltas.ToArray()
            };

            ServiceLocator.Get<TransitionService>().StartTransition(true);
            ServiceLocator.Get<TransitionService>().TransitionCompleted += GameEnd_OnTransitionCompleted;
        }
    }
}
