using CaveGame.Services;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CaveGame.Lobby
{
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _clientIdsLabel;
        [SerializeField] private Button _leaveGameButton;

        private SessionManagerService _sessionManager;

        private void Start()
        {
            _sessionManager = ServiceLocator.Get<SessionManagerService>();
            _sessionManager.SeatsSynchronized += OnSeatsSynchronized;
            _sessionManager.RequestSyncSeatsServerRpc();

            _leaveGameButton.onClick.AddListener(OnLeaveGameButtonPressed);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_sessionManager != null)
                _sessionManager.SeatsSynchronized -= OnSeatsSynchronized;
        }

        private void OnSeatsSynchronized()
        {
            string clientIdText = "";
            for (int i = 0; i < SessionManagerService.MAX_PLAYERS; i++)
            {
                if (_sessionManager.Seats[i].IsTaken)
                    clientIdText += _sessionManager.Seats[i].ClientID + "\n";
            }

            _clientIdsLabel.SetText(clientIdText);
        }

        private void OnLeaveGameButtonPressed()
        {
            ServiceLocator.Get<GameFlowService>().Leave();
        }
    }
}
