using System;
using System.Collections;
using System.Linq;
using CaveGame.Services;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CaveGame.Lobby
{
    public class LobbyManager : NetworkBehaviour
    {
        public struct LobbySeat : INetworkSerializable, IEquatable<LobbySeat>
        {
            public ulong ClientID;
            public bool IsReady;

            public bool IsTaken => ClientID != ulong.MaxValue;

            public LobbySeat(ulong clientId)
            {
                ClientID = clientId;
                IsReady = false;
            }

            public bool Equals(LobbySeat other)
            {
                return ClientID == other.ClientID;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientID);
                serializer.SerializeValue(ref IsReady);
            }
        }

        private const int COUNTDOWN_SECONDS = 5;

        public event Action LobbySeatsSynchronized;

        [SerializeField] private TextMeshProUGUI _clientIdsLabel;
        [SerializeField] private TextMeshProUGUI _countdownLabel;
        [SerializeField] private Button _leaveGameButton;
        [SerializeField] private Button _forceStartGameButton;

        private SessionManagerService _sessionManager;

        public LobbySeat[] LobbySeats { get; private set; } = new LobbySeat[SessionManagerService.MAX_PLAYERS];

        public bool IsClientReady => LobbySeats.ToList().Find(s => s.ClientID == _sessionManager.LocalClientId).IsReady;

        private bool _isCountdownStarted = false;
        private Coroutine _countdownEnumerator = null;

        private void Awake()
        {
            for (int i = 0; i < SessionManagerService.MAX_PLAYERS; i++)
                LobbySeats[i] = new LobbySeat(ulong.MaxValue);

            LobbySeatsSynchronized += OnLobbySeatsSynchronized;
        }

        private void Start()
        {
            _sessionManager = ServiceLocator.Get<SessionManagerService>();
            _sessionManager.SeatsSynchronized += OnSeatsSynchronized;
            _sessionManager.RequestSyncSeatsServerRpc();

            _leaveGameButton.onClick.AddListener(OnLeaveGameButtonPressed);

            if (NetworkManager.Singleton.IsServer)
                _forceStartGameButton.onClick.AddListener(OnForceStartGameButtonPressed);
            else
                _forceStartGameButton.gameObject.SetActive(false);
        }

        public override void OnDestroy()
        {
            LobbySeatsSynchronized -= OnLobbySeatsSynchronized;

            if (_sessionManager != null)
                _sessionManager.SeatsSynchronized -= OnSeatsSynchronized;

            base.OnDestroy();
        }

        private void OnLeaveGameButtonPressed()
        {
            ServiceLocator.Get<GameFlowService>().Leave();
        }

        private void OnForceStartGameButtonPressed()
        {
            if (IsServer) ServiceLocator.Get<SceneManagerService>().LoadScene(Constants.SceneNames.GAME_SCENE_NAME);
        }


        public ref LobbySeat GetLobbySeatRef(ulong clientId)
        {
            for (int i = 0; i < SessionManagerService.MAX_PLAYERS; i++)
            {
                if (LobbySeats[i].IsTaken && LobbySeats[i].ClientID == clientId)
                    return ref LobbySeats[i];
            }

            throw new Exception($"Seat with client id {clientId} not found!");
        }

        #region Ready State

        public void SetReady(bool readiness)
        {
            SyncReadyStateToServerRpc(readiness);
        }

        [Rpc(SendTo.Server)]
        public void SyncReadyStateToServerRpc(bool newReadyState, RpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            ref var clientSeat = ref GetLobbySeatRef(senderId);

            clientSeat.IsReady = newReadyState;
            SyncLobbySeatsRpc(LobbySeats);
        }

        #endregion

        #region Lobby Seat State

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void SyncLobbySeatsRpc(LobbySeat[] updatedSeats)
        {
            LobbySeats = updatedSeats;
            LobbySeatsSynchronized?.Invoke();
        }

        private void OnSeatsSynchronized()
        {
            string clientIdText = "";
            for (int i = 0; i < SessionManagerService.MAX_PLAYERS; i++)
            {
                var seat = _sessionManager.Seats[i];
                LobbySeats[i].ClientID = seat.ClientID;

                if (seat.IsTaken)
                {
                    clientIdText +=
                    _sessionManager.Seats[i].ClientID
                    + $" - {seat.ConnectionData.Username} "
                    + (seat.ClientID == _sessionManager.ServerId ? "(Server) " : "")
                    + (seat.ClientID == _sessionManager.LocalClientId ? "(This Client) " : "")
                    + "\n";
                }
            }

            _clientIdsLabel.SetText(clientIdText);

            LobbySeatsSynchronized?.Invoke();
        }

        private void OnLobbySeatsSynchronized()
        {
            // Check for total readiness
            if (_sessionManager.IsServer && LobbySeats.Any(ls => ls.IsTaken))
            {
                bool isAllReady = true;
                for (int i = 0; i < SessionManagerService.MAX_PLAYERS; i++)
                {
                    if (LobbySeats[i].IsTaken && !LobbySeats[i].IsReady) isAllReady = false;
                }

                SetCountdownStateRpc(isAllReady);
            }
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void SetCountdownStateRpc(bool state)
        {
            if (_isCountdownStarted == state) return;
            _isCountdownStarted = state;

            if (_countdownEnumerator != null) StopCoroutine(_countdownEnumerator);

            if (_isCountdownStarted)
            {
                _countdownEnumerator = StartCoroutine(StartCountdown());
            }
            else
            {
                _countdownLabel.gameObject.SetActive(false);
                StopCoroutine(_countdownEnumerator);
            }
        }

        private IEnumerator StartCountdown()
        {
            _countdownLabel.gameObject.SetActive(true);

            for (int i = COUNTDOWN_SECONDS; i >= 1; i--)
            {
                _countdownLabel.SetText($"Starting in {i} seconds...");

                yield return new WaitForSeconds(1);
            }

            if (IsServer) ServiceLocator.Get<SceneManagerService>().LoadScene(Constants.SceneNames.GAME_SCENE_NAME);
        }

        #endregion
    }
}
