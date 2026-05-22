using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace CaveTogether.Game.Actions
{
    public class ActionSelector : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _actionCountLabel;

        public void Initialize(int possibleActionCount)
        {
            Assert.IsTrue(possibleActionCount > 0);
            _actionCountLabel.SetText(possibleActionCount.ToString());
        }
    }
}