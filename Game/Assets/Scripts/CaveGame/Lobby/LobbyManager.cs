using System;
using System.Collections;
using System.Linq;
using CaveTogether.Common;
using CaveTogether.Common.Enums;
using CaveTogether.Game.Entities;
using CaveTogether.Services;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace CaveTogether.Lobby
{
    public class LobbyManager : NetworkBehaviour
    {
        public struct LobbySeat : INetworkSerializable, IEquatable<LobbySeat>
        {
            public ulong ClientID;
            public bool IsReady;
            public FixedString32Bytes CharacterTypeId;

            public bool IsTaken => ClientID != ulong.MaxValue;

            public LobbySeat(ulong clientId)
            {
                ClientID = clientId;
                IsReady = false;
                CharacterTypeId = "caver";
            }

            public bool Equals(LobbySeat other)
            {
                return ClientID == other.ClientID;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientID);
                serializer.SerializeValue(ref IsReady);
                serializer.SerializeValue(ref CharacterTypeId);
            }
        }

        private const int COUNTDOWN_SECONDS = 5;

        public event Action LobbySeatsSynchronized;

        [SerializeField] private Button _readyButton;
        [SerializeField] private TextMeshProUGUI _readyButtonLabel;

        [SerializeField] private Button _leaveGameButton;
        [SerializeField] private Button _forceStartGameButton;
        [SerializeField] private TextMeshProUGUI _countdownLabel;

        [SerializeField] private TMP_InputField _seedField;
        [SerializeField] private TMP_Dropdown _difficultyDropdown;

        [SerializeField] public CharacterDataSO[] CharacterData;

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
            _readyButton.onClick.AddListener(OnReadyButtonPressed);

            if (NetworkManager.Singleton.IsServer)
            {
                _forceStartGameButton.onClick.AddListener(OnForceStartGameButtonPressed);
            }
            else
            {
                _forceStartGameButton.gameObject.SetActive(false);
                _seedField.gameObject.SetActive(false);
                _difficultyDropdown.gameObject.SetActive(false);
            }

            ServiceLocator.Get<TransitionService>().StartTransition(false);
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

        private void OnReadyButtonPressed()
        {
            SetReady(!IsClientReady);
        }

        private void OnForceStartGameButtonPressed()
        {
            if (IsServer) StartGame();
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

        public void SetCharacter(string characterTypeId)
        {
            SyncCharacterStateToServerRpc(characterTypeId);
        }

        [Rpc(SendTo.Server)]
        public void SyncCharacterStateToServerRpc(FixedString32Bytes characterTypeId, RpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            ref var clientSeat = ref GetLobbySeatRef(senderId);

            clientSeat.CharacterTypeId = characterTypeId;

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
            for (int i = 0; i < SessionManagerService.MAX_PLAYERS; i++)
                LobbySeats[i].ClientID = _sessionManager.Seats[i].ClientID;

            LobbySeatsSynchronized?.Invoke();
        }

        private void OnLobbySeatsSynchronized()
        {
            _readyButtonLabel.SetText(IsClientReady ? "Ready" : "Not Ready");

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

            if (IsServer)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            FadeOutClientsRpc();

            var ts = ServiceLocator.Get<TransitionService>();
            ts.StartTransition(true);
            ts.TransitionCompleted += CompleteTransition;
        }

        [Rpc(SendTo.NotServer, InvokePermission = RpcInvokePermission.Server)]
        private void FadeOutClientsRpc()
        {
            ServiceLocator.Get<TransitionService>().StartTransition(true);
        }

        private void CompleteTransition()
        {
            var ts = ServiceLocator.Get<TransitionService>();
            ts.TransitionCompleted -= CompleteTransition;

            var validSeats = _sessionManager.Seats.Where(ls => ls.IsTaken).ToList();
            var players = new PlayerConfig[validSeats.Count];

            for (int i = 0; i < validSeats.Count; i++)
            {
                var seat = validSeats[i];
                var lobbySeat = LobbySeats.ToList().Find(ls => ls.ClientID == seat.ClientID);

                players[i] = new PlayerConfig
                {
                    OwnerClientId = seat.ClientID,
                    Username = seat.ConnectionData.Username,
                    CharacterId = lobbySeat.CharacterTypeId,
                };
            }

            var config = new GameConfig
            {
                Difficulty = (Difficulty)_difficultyDropdown.value,
                Seed = _seedField.text.IsNullOrEmpty() ? UnityEngine.Random.Range(100_000, 999_999).ToString() : _seedField.text,
                Players = players,
                IsCheatsEnabled = true,
            };

            ServiceLocator.Get<GameFlowService>().StartGame(config);
        }

        #endregion
    }
}
