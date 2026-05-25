using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace CaveTogether.Game.Actions
{
    public class ActionSelector : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _actionCountLabel;
        private bool _hoverState = false;

        public void Initialize(int possibleActionCount)
        {
            Assert.IsTrue(possibleActionCount > 0);
            _actionCountLabel.SetText(possibleActionCount.ToString());
        }

        public void ToggleHoverState(bool state)
        {
            if (state == _hoverState) return;
            _hoverState = state;

            transform.DOScale(state ? Vector3.one * 1.1f : Vector3.one, .1f);
        }
    }
}