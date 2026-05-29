using System.Collections.Generic;
using CaveTogether.Services;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CaveTogether.Game.UI
{
    public class GameNotificationUI : MonoBehaviour, IService
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<GameNotificationUI>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameNotificationUI>();
        }

        [SerializeField] private GameObject _notificationPrefab;

        [SerializeField] private float _typewriterSpeed = 20f;
        [SerializeField] private float _disappearSpeedMultiplier = 3f;
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _stackOffset = 60f;

        private readonly List<(RectTransform rt, float targetY)> _active = new();

        public void Push(string text)
        {
            float startY = -((RectTransform)transform).rect.height / 6f;

            for (int i = 0; i < _active.Count; i++)
            {
                float newTargetY = _active[i].targetY + _stackOffset;
                _active[i] = (_active[i].rt, newTargetY);
                _active[i].rt.DOAnchorPosY(newTargetY, 0.3f).SetEase(Ease.OutCubic);
            }

            var go = Instantiate(_notificationPrefab, transform);
            var entryRt = go.GetComponent<RectTransform>();
            entryRt.anchoredPosition = new Vector2(0, startY);
            _active.Add((entryRt, startY));

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
                    _active.RemoveAll(e => e.rt == entryRt);
                    Destroy(go);
                });
        }
    }
}
