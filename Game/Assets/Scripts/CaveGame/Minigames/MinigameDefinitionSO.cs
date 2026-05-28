using UnityEngine;

namespace CaveTogether.Minigames
{
    [CreateAssetMenu(menuName = "Cave Together/Minigame Definition")]
    public class MinigameDefinitionSO : ScriptableObject
    {
        public string SceneName;
        public string DisplayName;
        public string BuildupMessage;
        public string AnnouncementMessage;
    }
}