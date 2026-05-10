using UnityEngine;
using CaveTogether.Services;
using CaveTogether.Common;
using System.Linq;
using Unity.Netcode;
using CaveTogether.Common.Enums;
using CaveTogether.PlayerData;
using System.Collections;

#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
#endif

namespace CaveTogether.EditorTools
{
    public class EditorAutoStart : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR
            var tags = CurrentPlayer.Tags;

            if (tags.Contains("QuickSinglePlayer") || tags.Contains("QuickHost") || tags.Contains("QuickJoin"))
            {
                var bootstrapManager = FindAnyObjectByType<BootstrapManager>();
                if (bootstrapManager != null)
                    bootstrapManager.enabled = false;
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
#endif
        }

#if UNITY_EDITOR

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
                    CharacterId = "caver",
                    Username = $"Player #{Random.Range(1_000, 10_000).ToString()}"
                };
            }

            var dummyConfig = new GameConfig
            {
                Difficulty = Difficulty.Normal,
                Seed = Random.Range(100_000, 999_999).ToString(),
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