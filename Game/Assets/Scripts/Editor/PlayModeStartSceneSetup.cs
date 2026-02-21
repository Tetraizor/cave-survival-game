using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class PlayModeStartSceneSetup
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

        static PlayModeStartSceneSetup()
        {
            SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);

            if (bootstrapScene != null)
            {
                EditorSceneManager.playModeStartScene = bootstrapScene;
            }
            else
            {
                Debug.LogWarning($"Could not find Bootstrap scene at {BootstrapScenePath}. Make sure the path is correct.");
            }
        }
    }
}