using CaveTogether.Common;
using CaveTogether.Game.Entities;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CaveTogether.Game.UI
{
    public enum PlayerStatusPanelState
    {
        Hidden,
        Shown,
        Raised
    }

    public class PlayerStatusPanel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;

        [SerializeField] private Image[] _heartSprites;
        [SerializeField] private Image[] _energySprites;

        private ulong _correspondingPlayerId;
        private Character _character;

        private PlayerStatusPanelState _state = PlayerStatusPanelState.Shown;

        private float _moveTime = .3f;

        private float _hiddenY = -500;
        private float _shownY = 0;
        private float _raisedY = 500;

        private void OnDestroy()
        {
            Destroy(transform.parent.gameObject);
        }

        public void SetShowState(PlayerStatusPanelState state)
        {
            if (state == _state) return;
            _state = state;

            switch (state)
            {
                case PlayerStatusPanelState.Hidden:
                    GetComponent<RectTransform>().DOAnchorPosY(_hiddenY, _moveTime);
                    break;
                case PlayerStatusPanelState.Shown:
                    GetComponent<RectTransform>().DOAnchorPosY(_shownY, _moveTime);
                    break;
                case PlayerStatusPanelState.Raised:
                    GetComponent<RectTransform>().DOAnchorPosY(_raisedY, _moveTime);
                    break;
            }
        }

        public void Initialize(PlayerConfig data)
        {
            _correspondingPlayerId = data.OwnerClientId;
            _character = FindAnyObjectByType<CharacterManager>().GetCharacter(_correspondingPlayerId);
            _character.HealthChanged += OnHealthChanged;
            _character.EnergyChanged += OnEnergyChanged;

            _nameLabel.SetText(data.Username.ToString());
            _descriptionLabel.SetText(data.CharacterId.ToString());

            _state = PlayerStatusPanelState.Hidden;
            SetShowState(PlayerStatusPanelState.Shown);

            for (int i = 0; i < _energySprites.Length; i++)
                _energySprites[i].transform.parent.gameObject.SetActive(i < _character.MaxEnergy);

            for (int i = 0; i < _heartSprites.Length; i++)
                _heartSprites[i].transform.parent.gameObject.SetActive(i < _character.MaxHealth);

            OnEnergyChanged(_character.Energy);
            OnHealthChanged(_character.Health);
        }

        private void OnEnergyChanged(int newEnergy)
        {
            for (int i = 0; i < _energySprites.Length; i++)
            {
                bool active = i < newEnergy;
                _energySprites[i].transform.DOKill();
                _energySprites[i].transform
                    .DOScale(active ? Vector3.one : Vector3.zero, 0.2f)
                    .SetEase(active ? Ease.OutBack : Ease.InBack)
                    .SetDelay(i * 0.05f);
            }
        }

        private void OnHealthChanged(int newHealth)
        {
            for (int i = 0; i < _heartSprites.Length; i++)
            {
                bool active = i < newHealth;
                _heartSprites[i].transform.DOKill();
                _heartSprites[i].transform
                    .DOScale(active ? Vector3.one : Vector3.zero, 0.2f)
                    .SetEase(active ? Ease.OutBack : Ease.InBack)
                    .SetDelay(i * 0.05f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            FindAnyObjectByType<CameraManager>().FocusOn(_character.transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(Vector3.one, .1f);

            _character.SetHighlight(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(Vector3.one * 1.1f, .1f);

            _character.SetHighlight(true);
        }
    }
}