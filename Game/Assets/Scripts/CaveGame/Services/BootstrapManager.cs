using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveGame.Services
{
    public class BootstrapManager : MonoBehaviour
    {
        public const int INITIAL_SCENE_INDEX = 1;

        private IEnumerator Start()
        {
            yield return null;

            SceneManager.LoadSceneAsync(INITIAL_SCENE_INDEX);
        }
    }
}