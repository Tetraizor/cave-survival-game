using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Services
{
    public class TransitionService : MonoBehaviour, IService
    {
        public event Action TransitionCompleted;
        public event Action TransitionStarted;

        [SerializeField] private Image _background;

        private const float TransitionDuration = .3f;
        private bool _transitionState = false;
        private bool _isTransitioning = false;

        private void Awake()
        {
            ServiceLocator.Register<TransitionService>(this);
            DontDestroyOnLoad(gameObject);
        }

        public void StartTransition(bool state)
        {
            if (_isTransitioning || _transitionState == state) return;
            StartCoroutine(StartTransitionCoroutine(state));
        }

        public IEnumerator StartTransitionCoroutine(bool state)
        {
            _transitionState = state;

            _isTransitioning = true;
            TransitionStarted?.Invoke();

            _background.raycastTarget = true;
            _background.DOFade(state ? 1 : 0, TransitionDuration);
            yield return new WaitForSeconds(TransitionDuration);

            _background.raycastTarget = false;
            _isTransitioning = false;
            TransitionCompleted?.Invoke();
        }
    }
}