using UnityEngine;
using CaveTogether.Services;
using CaveTogether.Common;
using CaveTogether.Common.Enums;
using System.Linq;
using Unity.Netcode;
using System.Collections;

#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
using CaveTogether.Game.Turn;
using CaveTogether.Minigames;
#endif

namespace CaveTogether.EditorTools
{
    public class EditorAutoStart : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private MinigameDefinitionSO _debugMinigame;
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            var tags = CurrentPlayer.Tags;

            if (tags.Contains("QuickSinglePlayer") || tags.Contains("QuickHost") || tags.Contains("QuickJoin")
                || tags.Contains("QuickMinigame") || tags.Contains("QuickMinigame2"))
            {
                var bootstrapManager = FindAnyObjectByType<BootstrapManager>();
                if (bootstrapManager != null)
                    bootstrapManager.enabled = false;

                DontDestroyOnLoad(gameObject);
            }
#endif
        }

        private IEnumerator Start()
        {
#if UNITY_EDITOR
            yield return null;

            var tags = CurrentPlayer.Tags;

            if (tags.Contains("QuickSinglePlayer"))
            {
                Debug.Log("<color=cyan>[EditorAutoStart] 'QuickSinglePlayer' tag detected. Auto-hosting game...</color>");

                var dummyPlayerData = new UserConnectionData
                {
                    Username = "Host"
                };

                NetworkManager.Singleton.OnServerStarted += () =>
                {
                    var dummyConfig = new GameConfig
                    {
                        Difficulty = Difficulty.Normal,
                        Seed = Random.Range(100_000, 999_999).ToString(),
                        IsCheatsEnabled = true,
                        Players = new PlayerConfig[]
                        {
                        new PlayerConfig
                        {
                            CharacterId = "caver",
                            Username = "Host",
                            OwnerClientId = 0
                        }
                        }
                    };

                    ServiceLocator.Get<GameFlowService>().StartGame(dummyConfig);
                };
                ServiceLocator.Get<GameFlowService>().Host("127.0.0.1", 7777, dummyPlayerData);
            }
            else if (tags.Contains("QuickHost"))
            {
                Debug.Log("<color=cyan>[EditorAutoStart] 'QuickHost' tag detected. Auto-hosting game...</color>");

                var dummyPlayerData = new UserConnectionData
                {
                    Username = "Host"
                };

                NetworkManager.Singleton.OnServerStarted += () =>
                {
                    StartCoroutine(WaitForPlayersAndStart());
                };
                ServiceLocator.Get<GameFlowService>().Host("127.0.0.1", 7777, dummyPlayerData);
            }
            else if (tags.Contains("QuickJoin"))
            {
                Debug.Log("<color=cyan>[EditorAutoStart] 'QuickJoin' tag detected. Auto-joining game...</color>");
                StartCoroutine(DelayedJoin());
            }
            else if (tags.Contains("QuickMinigame"))
            {
                Debug.Log("<color=cyan>[EditorAutoStart] 'QuickMinigame' tag detected. Auto-hosting and skipping to minigame...</color>");

                var dummyPlayerData = new UserConnectionData { Username = "Host" };

                NetworkManager.Singleton.OnServerStarted += () =>
                {
                    var config = new GameConfig
                    {
                        Difficulty = Difficulty.Normal,
                        Seed = Random.Range(100_000, 999_999).ToString(),
                        IsCheatsEnabled = true,
                        Players = new PlayerConfig[]
                        {
                            new() { CharacterId = "caver", Username = "Host", OwnerClientId = 0 }
                        }
                    };
                    ServiceLocator.Get<GameFlowService>().StartGame(config);
                    StartCoroutine(SkipFirstRound());
                };
                ServiceLocator.Get<GameFlowService>().Host("127.0.0.1", 7777, dummyPlayerData);
            }
            else if (tags.Contains("QuickMinigame2"))
            {
                Debug.Log("<color=cyan>[EditorAutoStart] 'QuickMinigame2' tag detected. Waiting for 1 other player then skipping to minigame...</color>");

                var dummyPlayerData = new UserConnectionData { Username = "Host" };

                NetworkManager.Singleton.OnServerStarted += () =>
                {
                    StartCoroutine(WaitForPlayersAndStartMinigame());
                };
                ServiceLocator.Get<GameFlowService>().Host("127.0.0.1", 7777, dummyPlayerData);
            }
#endif
        }

#if UNITY_EDITOR

        private IEnumerator SkipFirstRound()
        {
            TurnManager tm = null;
            while (tm == null || tm.TurnOrder.Count == 0)
            {
                tm = FindAnyObjectByType<TurnManager>();
                yield return null;
            }

            if (_debugMinigame != null)
            {
                var mm = FindAnyObjectByType<MinigameManager>();
                var field = typeof(MinigameManager).GetField("_minigames",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(mm, new MinigameDefinitionSO[] { _debugMinigame });
            }

            int turnsRemaining = tm.TurnOrder.Count;
            bool turnReady = tm.CurrentRound > 0;
            void OnTurnStarted(ulong _) => turnReady = true;
            tm.TurnStarted += OnTurnStarted;

            while (turnsRemaining > 0)
            {
                yield return new WaitUntil(() => turnReady);
                turnReady = false;
                turnsRemaining--;
                tm.AdvanceTurnRpc();
            }

            tm.TurnStarted -= OnTurnStarted;
        }

        private IEnumerator WaitForPlayersAndStartMinigame()
        {
            var sessionManager = ServiceLocator.Get<SessionManagerService>();

            Debug.Log("<color=yellow>[EditorAutoStart] QuickMinigame2: waiting for 1 other player...</color>");

            while (sessionManager.OccupiedSeatCount < 2)
                yield return null;

            var seats = sessionManager.Seats.Where(s => s.IsTaken).ToList();
            var players = new PlayerConfig[seats.Count];
            for (int i = 0; i < seats.Count; i++)
            {
                players[i] = new PlayerConfig
                {
                    OwnerClientId = seats[i].ClientID,
                    CharacterId = i == 0 ? "caver" : "medic",
                    Username = $"Player #{Random.Range(1_000, 10_000)}"
                };
            }

            var config = new GameConfig
            {
                Difficulty = Difficulty.Normal,
                Seed = Random.Range(100_000, 999_999).ToString(),
                IsCheatsEnabled = true,
                Players = players,
            };

            ServiceLocator.Get<GameFlowService>().StartGame(config);
            StartCoroutine(SkipFirstRound());
        }

        private IEnumerator WaitForPlayersAndStart()
        {
            var sessionManager = ServiceLocator.Get<SessionManagerService>();

            Debug.Log($"<color=yellow>[EditorAutoStart] Host waiting for 1 other player to join...</color>");

            while (sessionManager.OccupiedSeatCount < 2)
            {
                yield return null;
            }

            Debug.Log("<color=green>[EditorAutoStart] All players connected! Building config and starting...</color>");

            var seats = sessionManager.Seats.Where(s => s.IsTaken).ToList();
            var players = new PlayerConfig[seats.Count];

            for (int i = 0; i < seats.Count; i++)
            {
                players[i] = new PlayerConfig
                {
                    OwnerClientId = seats[i].ClientID,
                    CharacterId = i == 0 ? "caver" : "medic",
                    Username = $"Player #{Random.Range(1_000, 10_000).ToString()}"
                };
            }

            var dummyConfig = new GameConfig
            {
                Difficulty = Difficulty.Normal,
                Seed = Random.Range(100_000, 999_999).ToString(),
                IsCheatsEnabled = true,
                Players = players,
            };

            ServiceLocator.Get<GameFlowService>().StartGame(dummyConfig);
        }

        private IEnumerator DelayedJoin()
        {
            yield return new WaitForSeconds(1f);

            var dummyPlayerData = new UserConnectionData { Username = "Client" };
            ServiceLocator.Get<GameFlowService>().Join("127.0.0.1", 7777, dummyPlayerData);
        }
#endif
    }
}
