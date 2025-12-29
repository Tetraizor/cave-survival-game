using Unity.Netcode;
using UnityEngine;

namespace CaveGame.Common
{
    /// <summary>
    /// A robust generic Singleton base class for Unity MonoBehaviour.
    /// Usage: public class MyManager : Singleton<MyManager> { }
    /// </summary>
    /// <typeparam name="T">The type of the subclass.</typeparam>
    public abstract class NetworkSingletonBase<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        /// <summary>
        /// Returns the instance of this singleton.
        /// If it doesn't exist, it creates a new GameObject with the component attached.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Search for existing instance
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                            return _instance;
                        }

                        // Create new instance if one doesn't exist
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";

                            // Make it persistent (optional, but standard for singletons)
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Ensure persistence and destroy duplicates if manually placed in the scene.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Deleting extra instance of '{typeof(T)}' attached to '{gameObject.name}'");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Prevents the singleton from being recreated during game shutdown.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }
    }
}