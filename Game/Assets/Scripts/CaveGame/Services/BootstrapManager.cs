using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaveTogether.Services
{
    public class BootstrapManager : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null;
            SceneManager.LoadSceneAsync(Constants.SceneNames.MENU_SCENE_NAME);
        }
    }
}