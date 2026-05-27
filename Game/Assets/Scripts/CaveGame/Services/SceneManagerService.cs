using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveTogether.Services
{
    public class SceneManagerService : NetworkBehaviour, IService
    {
        public event Action<string, LoadSceneMode> SceneLoadCompleted;
        public event Action<string> SceneUnloadCompleted;

        private void Awake()
        {
            ServiceLocator.Register<SceneManagerService>(this);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadEventCompleted;
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += HandleUnloadEventCompleted;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompleted;
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= HandleUnloadEventCompleted;
            }
        }

        public void LoadScene(string sceneName, LoadSceneMode sceneMode = LoadSceneMode.Single)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[SceneManagerService] Clients cannot initiate scene load.");
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, sceneMode);
        }

        public void UnloadAdditiveScene(string sceneName)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[SceneManagerService] Clients cannot initiate scene unload.");
                return;
            }

            Scene sceneToUnload = SceneManager.GetSceneByName(sceneName);

            if (sceneToUnload.IsValid() && sceneToUnload.isLoaded)
            {
                NetworkManager.Singleton.SceneManager.UnloadScene(sceneToUnload);
            }
            else
            {
                Debug.LogError($"[SceneManagerService] Cannot unload {sceneName} because it is not loaded or invalid.");
            }
        }

        #region Event Handlers

        private void HandleUnloadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            SceneUnloadCompleted?.Invoke(sceneName);
        }

        private void HandleLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            SceneLoadCompleted?.Invoke(sceneName, loadSceneMode);
        }

        #endregion
    }
}