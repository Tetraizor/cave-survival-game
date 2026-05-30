using System;
using System.Linq;
using CaveTogether.Game.Entities;
using CaveTogether.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaveTogether.Lobby
{
    public class LobbyPlayer : MonoBehaviour
    {
        [SerializeField] private CharacterRenderer _renderer;
        [SerializeField] private int _correspondingSeat = 0;
        [SerializeField] private string _startingCharacter = "caver";

        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _characterTypeLabel;
        [SerializeField] private TextMeshProUGUI _readinessLabel;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;

        [SerializeField] private Color _readyLabelColor;
        [SerializeField] private Color _notReadyLabelColor;

        private LobbyManager _lobbyManager;
        private SessionManagerService _sessionManager;
        private bool _characterInitialized;
        private bool _seatAnimationInitialized;

        public ref LobbyManager.LobbySeat CorrespondingSeat => ref _lobbyManager.LobbySeats[_correspondingSeat];

        private bool IsClientCard()
        {
            ulong clientId = _sessionManager.LocalClientId;
            return CorrespondingSeat.ClientID == clientId;
        }

        private void Start()
        {
            _sessionManager = ServiceLocator.Get<SessionManagerService>();

            _leftButton.onClick.AddListener(OnLeftButtonPressed);
            _rightButton.onClick.AddListener(OnRightButtonPressed);

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
            if (!_characterInitialized && IsClientCard())
            {
                _characterInitialized = true;
                _lobbyManager.SetCharacter(_startingCharacter);
            }
            UpdateCard();
        }

        private void OnRightButtonPressed()
        {
            var characters = _lobbyManager.CharacterData.ToList();
            int currentIndex = characters.IndexOf(characters.Find(c => c.TypeId == CorrespondingSeat.CharacterTypeId));

            string nextCharacterTypeId = characters[(currentIndex + 1) % characters.Count].TypeId;

            _lobbyManager.SetCharacter(nextCharacterTypeId);
        }

        private void OnLeftButtonPressed()
        {
            var characters = _lobbyManager.CharacterData.ToList();
            int currentIndex = characters.IndexOf(characters.Find(c => c.TypeId == CorrespondingSeat.CharacterTypeId));

            string nextCharacterTypeId = characters[(currentIndex - 1 + characters.Count) % characters.Count].TypeId;

            _lobbyManager.SetCharacter(nextCharacterTypeId);
        }

        private void UpdateCard()
        {
            bool isTaken = CorrespondingSeat.IsTaken;
            _renderer.gameObject.SetActive(isTaken);
            _nameLabel.gameObject.SetActive(isTaken);
            _characterTypeLabel.gameObject.SetActive(isTaken);
            _readinessLabel.gameObject.SetActive(isTaken);

            _leftButton.gameObject.SetActive(isTaken && IsClientCard());
            _rightButton.gameObject.SetActive(isTaken && IsClientCard());

            if (!isTaken) return;

            if (!_seatAnimationInitialized)
            {
                _seatAnimationInitialized = true;
                _renderer.GetComponent<Animator>().SetTrigger(_correspondingSeat.ToString());
            }

            var seat = _sessionManager.Seats.ToList().Find(s => s.ClientID == CorrespondingSeat.ClientID);
            var characterData = _lobbyManager.CharacterData.ToList().Find(cd => cd.TypeId == CorrespondingSeat.CharacterTypeId);

            if (characterData != null) _renderer.Initialize(characterData);

            _nameLabel.SetText(seat.ConnectionData.Username);
            _characterTypeLabel.SetText(characterData.Name);
            _readinessLabel.SetText(CorrespondingSeat.IsReady ? "Ready" : "Not Ready");
            _readinessLabel.color = CorrespondingSeat.IsReady ? _readyLabelColor : _notReadyLabelColor;
        }
    }
}