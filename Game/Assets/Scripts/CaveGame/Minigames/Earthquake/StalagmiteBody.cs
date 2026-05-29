using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.Minigames.Earthquake
{
    public class StalagmiteBody : NetworkBehaviour
    {
        [SerializeField] private ParticleSystem _breakParticles;
        private EarthquakeMinigame _minigameController;

        private float _targetScale;

        public void Initialize(EarthquakeMinigame minigameController, float delay, float scale = 1f)
        {
            _minigameController = minigameController;
            StartAnimationRpc(delay, scale);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void StartAnimationRpc(float delay, float scale)
        {
            _targetScale = scale;
            transform.localScale = Vector3.zero;
            StartCoroutine(DelayedStart(delay));
        }

        private IEnumerator DelayedStart(float delay)
        {
            yield return new WaitForSeconds(delay);

            transform.DOScale(_targetScale, .5f).OnComplete(() =>
            {
                transform.DOShakePosition(1.0f, .05f, 10).OnComplete(() =>
                {
                    transform.DOMoveY(-1, 1).SetEase(Ease.InCubic).OnComplete(() =>
                    {
                        if (IsServer && IsSpawned) NetworkObject.Despawn();
                    }).SetLink(gameObject);
                }).SetLink(gameObject);
            }).SetLink(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer || !IsSpawned) return;

            if (collision.TryGetComponent<EarthquakePlayerController>(out var emc))
                _minigameController.NotifyPlayerCollision(emc);

            BreakRpc();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void BreakRpc()
        {
            _breakParticles.transform.parent = null;
            _breakParticles.transform.localScale = Vector3.one;
            _breakParticles.Play();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && _minigameController != null)
                _minigameController.NotifyStalagmiteRemoved();
        }
    }
}