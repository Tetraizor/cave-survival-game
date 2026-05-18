using System;
using System.Collections.Generic;
using System.Linq;
using CaveTogether.Common;
using Unity.Netcode;
using UnityEngine;

namespace CaveTogether.Services
{
    public class SessionManagerService : NetworkBehaviour, IService
    {
        public const int MAX_PLAYERS = 4;

        public Action SeatsSynchronized;

        public bool IsSessionOpen { get; private set; } = false;

        public Seat[] Seats { get; private set; }

        private readonly Dictionary<ulong, UserConnectionData> _pendingData = new();

        public int OccupiedSeatCount => Seats.Count(s => s.IsTaken);
        public int EmptySeatCount => Seats.Count(s => !s.IsTaken);

        public ulong LocalClientId => NetworkManager.LocalClientId;
        public ulong ServerId => NetworkManager.ServerClientId;

        private void Awake()
        {
            ServiceLocator.Register<SessionManagerService>(this);
            DontDestroyOnLoad(this);

            Seats = new Seat[MAX_PLAYERS];
            for (int i = 0; i < MAX_PLAYERS; i++)
                Seats[i] = new Seat(ulong.MaxValue);
        }

        private void Start()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.ConnectionApprovalCallback += OnApprovalCheck;
        }

        public override void OnDestroy()
        {
            if (NetworkManager)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.ConnectionApprovalCallback -= OnApprovalCheck;
            }

            base.OnDestroy();
        }

        public void SetSessionState(bool isOpen)
        {
            IsSessionOpen = isOpen;
        }

        public void StoreLocalUserData(UserConnectionData data)
        {
            _pendingData[NetworkManager.ServerClientId] = data;
        }

        public void ResetSession()
        {
            IsSessionOpen = false;
            for (int i = 0; i < MAX_PLAYERS; i++)
                Seats[i] = new Seat(ulong.MaxValue);
        }

        public Seat GetLocalSeat()
        {
            ulong clientId = NetworkManager.LocalClientId;
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (Seats[i].ClientID == clientId) return Seats[i];
            }

            throw new Exception("Local client is not in seats");
        }

        public void OnPlayerJoined(ulong clientId)
        {
            if (!IsSessionOpen)
            {
                Debug.LogWarning($"Player {clientId} tried to join but session is not open.");
                return;
            }

            if (EmptySeatCount == 0)
            {
                Debug.LogWarning($"Player {clientId} tried to join but lobby is full.");
                return;
            }

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (!Seats[i].IsTaken)
                {
                    _pendingData.TryGetValue(clientId, out var connectionData);
                    _pendingData.Remove(clientId);

                    Seats[i] = new Seat(clientId) { ConnectionData = connectionData };
                    if (IsServer) SyncSeatsRpc(Seats, RpcTarget.ClientsAndHost);
                    break;
                }
            }
        }

        public void OnPlayerLeft(ulong clientId)
        {
            if (!IsSessionOpen)
            {
                Debug.LogWarning($"Player {clientId} tried to leave but session is not open.");
                return;
            }

            if (OccupiedSeatCount == 0)
            {
                Debug.LogWarning("Player left but no seats are occupied.");
                return;
            }

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (Seats[i].ClientID == clientId)
                {
                    Seats[i] = new Seat(ulong.MaxValue);
                    if (IsServer) SyncSeatsRpc(Seats, RpcTarget.ClientsAndHost);
                    return;
                }
            }

            Debug.LogWarning($"Player {clientId} not found in seats on disconnect.");
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SyncSeatsRpc(Seat[] updatedSeats, RpcParams rpcParams = default)
        {
            Seats = updatedSeats;
            SeatsSynchronized?.Invoke();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestSyncSeatsServerRpc(RpcParams rpcParams = default)
        {
            ulong requesterId = rpcParams.Receive.SenderClientId;
            SyncSeatsRpc(Seats, RpcTarget.Single(requesterId, RpcTargetUse.Temp));
        }

        private void OnApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (!IsSessionOpen)
            {
                response.Approved = false;
                response.Reason = "Session is not open";
                return;
            }

            if (EmptySeatCount == 0)
            {
                response.Approved = false;
                response.Reason = "Lobby is full";
                return;
            }

            if (request.Payload != null && request.Payload.Length > 0)
            {
                var json = System.Text.Encoding.UTF8.GetString(request.Payload);
                _pendingData[request.ClientNetworkId] = JsonUtility.FromJson<UserConnectionData>(json);
            }

            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            OnPlayerJoined(clientId);
            Debug.Log($"Player {clientId} joined");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;
            if (!IsSessionOpen) return;

            if (NetworkManager.ShutdownInProgress) return;

            OnPlayerLeft(clientId);
            Debug.Log($"Player {clientId} disconnected");
        }
    }
}
