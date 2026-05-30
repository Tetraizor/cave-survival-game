using System;
using CaveTogether.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CaveTogether.Lobby.LobbyManager;

namespace CaveTogether.Lobby
{
    public class PlayerCard : MonoBehaviour
    {
        [SerializeField] private RectTransform _emptyCardElementsContainer;
        [SerializeField] private RectTransform _occupiedCardElementsContainer;

        [SerializeField] private TextMeshProUGUI _playerNameLabel;
        [SerializeField] private TextMeshProUGUI _playerReadinessLabel;
        [SerializeField] private TextMeshProUGUI _readyButtonLabel;
        [SerializeField] private TextMeshProUGUI _readyNonClientLabel;

        [SerializeField] private Button _readyButton;

        [SerializeField] private Button _previousCharacterButton;
        [SerializeField] private Button _nextCharacterButton;

        [SerializeField] private int _correspondingSeatSlot = 0;

        private LobbyManager _lobbyManager;
        private SessionManagerService _sessionManager;

        public ref LobbySeat CorrespondingSeat => ref _lobbyManager.LobbySeats[_correspondingSeatSlot];

        private bool IsClientCard()
        {
            ulong clientId = _sessionManager.LocalClientId;
            return CorrespondingSeat.ClientID == clientId;
        }

        private void Awake()
        {
            _readyButton.onClick.AddListener(ReadyButtonPressed);
        }

        private void Start()
        {
            _sessionManager = ServiceLocator.Get<SessionManagerService>();

            _lobbyManager = FindAnyObjectByType<LobbyManager>();
            if (!_lobbyManager) throw new Exception("[PlayerCard] LobbyManager could not be found!");

            _lobbyManager.LobbySeatsSynchronized += OnLobbySeatsSynchronized;
        }

        private void OnDestroy()
        {
            if (_lobbyManager)
                _lobbyManager.LobbySeatsSynchronized -= OnLobbySeatsSynchronized;
        }

        private void OnLobbySeatsSynchronized()
        {
            UpdateCard();
        }

        private void ReadyButtonPressed()
        {
            _lobbyManager.SetReady(!_lobbyManager.IsClientReady);
        }

        private void UpdateCard()
        {
            var seat = _sessionManager.Seats[_correspondingSeatSlot];

            _emptyCardElementsContainer.gameObject.SetActive(!seat.IsTaken);
            _occupiedCardElementsContainer.gameObject.SetActive(seat.IsTaken);

            _readyButton.gameObject.SetActive(IsClientCard());
            _readyButtonLabel.SetText(CorrespondingSeat.IsReady ? "<color=\"green\">Ready</color>" : "<color=\"red\">Not Ready</color>");
            _readyNonClientLabel.SetText(CorrespondingSeat.IsReady ? "<color=\"green\">Ready</color>" : "<color=\"red\">Not Ready</color>");
            _playerNameLabel.SetText(seat.ConnectionData.Username.ToString());
        }
    }
}