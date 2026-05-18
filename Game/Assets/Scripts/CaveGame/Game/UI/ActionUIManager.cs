using System;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Actions;
using CaveTogether.Game.Entities;
using CaveTogether.Game.Turn;
using CaveTogether.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Game.UI
{
    public class ActionUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform _actionBarContainer;
        [SerializeField] private Button _skipTurnButton;
        [SerializeField] private TextMeshProUGUI _skipTurnButtonLabel;

        private ActionManager _actionManager;
        private TurnManager _turnManager;
        private CharacterManager _characterManager;

        private Character _clientCharacter;

        public void Initialize()
        {
            var clientId = ServiceLocator.Get<SessionManagerService>().LocalClientId;

            _actionManager = FindAnyObjectByType<ActionManager>();
            _turnManager = FindAnyObjectByType<TurnManager>();
            _characterManager = FindAnyObjectByType<CharacterManager>();

            _clientCharacter = _characterManager.GetCharacter(clientId);

            _turnManager.TurnStarted += OnTurnStarted;
            _turnManager.RoundEnded += OnRoundEnded;
            _skipTurnButton.onClick.AddListener(OnSkipTurnButtonPressed);
        }

        public void Deinitialize()
        {
            _turnManager.TurnStarted -= OnTurnStarted;
            _turnManager.RoundEnded -= OnRoundEnded;
            _skipTurnButton.onClick.RemoveAllListeners();
        }

        private void OnTurnStarted(ulong turnOwnerId)
        {
            bool isOwnersTurn = turnOwnerId == _clientCharacter.OwnerClientId;
            _skipTurnButton.gameObject.SetActive(isOwnersTurn);
        }

        private void OnRoundEnded(int round)
        {
            _skipTurnButton.gameObject.SetActive(false);
        }

        private void OnSkipTurnButtonPressed()
        {
            _actionManager.RequestAction(_clientCharacter, new ActionRequest() { Type = ActionType.EndTurn });
        }
    }
}