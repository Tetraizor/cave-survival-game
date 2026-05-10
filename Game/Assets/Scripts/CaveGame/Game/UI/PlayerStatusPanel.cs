using CaveGame.PlayerData;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CaveGame.Game.UI
{
    public enum PlayerStatusPanelState
    {
        Hidden,
        Shown,
        Raised
    }

    public class PlayerStatusPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private TextMeshProUGUI _healthLabel;
        [SerializeField] private TextMeshProUGUI _energyLabel;

        private ulong _correspondingPlayerId;

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

            _nameLabel.SetText(data.Username.ToString());
            _descriptionLabel.SetText(data.Character.ToString());
            _healthLabel.SetText("Health: 4");
            _energyLabel.SetText("Energy: 4");

            _state = PlayerStatusPanelState.Hidden;
            SetShowState(PlayerStatusPanelState.Shown);
        }
    }
}