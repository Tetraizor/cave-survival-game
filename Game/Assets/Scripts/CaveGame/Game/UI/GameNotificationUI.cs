using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CaveTogether.Game.UI
{
    public class GameNotificationUI : MonoBehaviour
    {
        [SerializeField] private GameObject _notificationPrefab;

        [SerializeField] private float _typewriterSpeed = 20f;
        [SerializeField] private float _disappearSpeedMultiplier = 3f;
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _stackOffset = 60f;

        private readonly List<RectTransform> _active = new();

        public void Push(string text)
        {
            foreach (var rt in _active)
                rt.DOAnchorPosY(rt.anchoredPosition.y + _stackOffset, 0.3f).SetEase(Ease.OutCubic);

            var go = Instantiate(_notificationPrefab, transform);
            var entryRt = go.GetComponent<RectTransform>();
            float startY = -((RectTransform)transform).rect.height / 6f;
            entryRt.anchoredPosition = new Vector2(0, startY);
            _active.Add(entryRt);

            var tmp = go.GetComponent<TextMeshProUGUI>();

            tmp.text = text;
            tmp.maxVisibleCharacters = 0;

            DOTween.Sequence()
                .Append(DOTween.To(
                    () => tmp.maxVisibleCharacters,
                    x => tmp.maxVisibleCharacters = x,
                    text.Length,
                    text.Length / _typewriterSpeed)
                    .SetEase(Ease.Linear))
                .AppendInterval(_displayDuration)
                .Append(DOTween.To(
                    () => tmp.maxVisibleCharacters,
                    x => tmp.maxVisibleCharacters = x,
                    0,
                    text.Length / (_typewriterSpeed * _disappearSpeedMultiplier))
                    .SetEase(Ease.Linear))
                .OnComplete(() =>
                {
                    _active.Remove(entryRt);
                    Destroy(go);
                });
        }
    }
}
