using CaveGame.Common;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveGame.Services
{
    public class GameFlowService : MonoBehaviour, IService
    {
        private void Awake()
        {
            if (ServiceLocator.Services.ContainsKey(typeof(GameFlowService)))
            {
                Destroy(gameObject);
                return;
            }

            ServiceLocator.Register<GameFlowService>(this);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            if (ServiceLocator.Services.TryGetValue(typeof(GameFlowService), out var registered) && ReferenceEquals(registered, this))
                ServiceLocator.Unregister<GameFlowService>();
        }

        public void Host(string address, ushort port, PlayerConnectionData data)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(address, port, "0.0.0.0");

            ServiceLocator.Get<SessionManagerService>().StoreLocalPlayerData(data);

            ServiceLocator.Get<SessionManagerService>().SetSessionState(true);
            bool connectionState = NetworkManager.Singleton.StartHost();
            if (connectionState)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.LOBBY_SCENE_NAME, LoadSceneMode.Single);
            }
            else
            {
                ServiceLocator.Get<SessionManagerService>().SetSessionState(false);
            }
        }

        public void Join(string address, ushort port, PlayerConnectionData data)
        {
            var payload = JsonUtility.ToJson(data);
            Debug.Log($"[Join] Payload JSON: '{payload}'");
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(payload);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(address, port, "0.0.0.0");

            ServiceLocator.Get<SessionManagerService>().SetSessionState(true);
            bool connectionState = NetworkManager.Singleton.StartClient();
            if (!connectionState) ServiceLocator.Get<SessionManagerService>().SetSessionState(false);
        }

        public void Leave()
        {
            NetworkManager.Singleton.Shutdown();
            ServiceLocator.Get<SessionManagerService>().ResetSession();
            SceneManager.LoadScene(Constants.SceneNames.MENU_SCENE_NAME, LoadSceneMode.Single);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer) return;
            Leave();
        }
    }
}
