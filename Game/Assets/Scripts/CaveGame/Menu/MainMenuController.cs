using CaveGame.Common;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

namespace CaveGame.Game
{
    public class MainMenuController : SingletonBase<MainMenuController>
    {
        private const string LOBBY_SCENE_NAME = "Lobby";

        public void Host(string address, ushort port)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(address, port, "0.0.0.0");

            bool connectionState = NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.LoadScene(
                LOBBY_SCENE_NAME,
                LoadSceneMode.Single
            );
        }

        public void Join(string address, ushort port)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(address, port, "0.0.0.0");

            bool connectionState = NetworkManager.Singleton.StartClient();
        }
    }
}