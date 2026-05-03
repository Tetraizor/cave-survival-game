using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CaveGame.Services
{
    public struct Seat : INetworkSerializable, IEquatable<Seat>
    {
        public Seat(ulong clientId)
        {
            ClientID = clientId;
        }

        public ulong ClientID;

        public bool IsTaken => ClientID != ulong.MaxValue;

        public bool Equals(Seat other)
        {
            return ClientID == other.ClientID;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientID);
        }
    }

    public class SessionManagerService : NetworkBehaviour, IService
    {
        public const int MAX_PLAYERS = 4;

        public Action SeatsSynchronized;

        public bool IsSessionOpen { get; private set; } = false;

        public Seat[] Seats { get; private set; }

        public int OccupiedSeatCount => Seats.Count(s => s.IsTaken);
        public int EmptySeatCount => Seats.Count(s => !s.IsTaken);

        public ulong LocalClientId => NetworkManager.LocalClientId;

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

            if (NetworkManager.IsServer)
                NetworkManager.ConnectionApprovalCallback += OnApprovalCheck;
        }

        public override void OnDestroy()
        {
            if (NetworkManager)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;

                if (NetworkManager.IsServer)
                    NetworkManager.ConnectionApprovalCallback -= OnApprovalCheck;
            }

            base.OnDestroy();
        }

        public void SetSessionState(bool isOpen)
        {
            IsSessionOpen = isOpen;
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
                    Seats[i] = new Seat(clientId);
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
            Debug.Log($"Seats synced. Occupied: {OccupiedSeatCount}");
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

            OnPlayerLeft(clientId);
            Debug.Log($"Player {clientId} disconnected");
        }
    }
}
