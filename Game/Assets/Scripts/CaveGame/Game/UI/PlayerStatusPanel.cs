using System;
using System.Linq;
using CaveTogether.Common;
using CaveTogether.Game.Entities;
using CaveTogether.Items;
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

        [SerializeField] private Image _portrait;

        [SerializeField] private Image[] _heartSprites;
        [SerializeField] private Image[] _energySprites;

        [SerializeField] private Image[] _itemSprites;

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
            _character.Inventory.InventoryChanged += OnInventoryChanged;

            _nameLabel.SetText(data.Username.ToString());
            _descriptionLabel.SetText(data.CharacterId.ToString());
            _portrait.sprite = _character.CharacterData.Portrait;

            _state = PlayerStatusPanelState.Hidden;
            SetShowState(PlayerStatusPanelState.Shown);

            for (int i = 0; i < _energySprites.Length; i++)
                _energySprites[i].transform.parent.gameObject.SetActive(i < _character.MaxEnergy);

            for (int i = 0; i < _heartSprites.Length; i++)
                _heartSprites[i].transform.parent.gameObject.SetActive(i < _character.MaxHealth);

            OnEnergyChanged(_character.Energy, _character.Energy);
            OnHealthChanged(_character.Energy, _character.Health);

            OnInventoryChanged();
        }

        private void OnEnergyChanged(int previousEnergy, int newEnergy)
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

        private void OnHealthChanged(int previousHealth, int newHealth)
        {
            if (previousHealth > newHealth)
            {
                transform.GetComponent<RectTransform>().DOKill();
                transform.GetComponent<RectTransform>().DOShakeAnchorPos(.6f, 8f, 10);
                FindAnyObjectByType<CameraManager>().Shake(.4f, .2f);
            }

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

        private void OnInventoryChanged()
        {
            var previousSprites = _itemSprites.Select(s => s.sprite).ToList();
            var itemDb = FindAnyObjectByType<ItemDatabaseService>();


            for (int i = 0; i < _itemSprites.Length; i++)
            {
                var itemSlot = _itemSprites[i];
                var itemData = itemDb.GetSO(_character.Inventory.GetItemType(i));
                var newSprite = itemData != null ? itemData.Icon : null;

                if (previousSprites[i] != newSprite)
                {
                    itemSlot.sprite = newSprite;
                    itemSlot.transform.parent.GetComponent<RectTransform>().DOKill();
                    itemSlot.transform.parent.GetComponent<RectTransform>().DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), .2f);
                }

                itemSlot.color = new Color(1, 1, 1, newSprite == null ? 0 : 1);
            }
        }
    }
}