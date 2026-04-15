using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CaveGame.Services
{
    public struct Seat
    {
        public Seat(ulong clientId)
        {
            ClientID = clientId;
        }

        public ulong ClientID { get; private set; }

        public bool IsTaken => ClientID != ulong.MaxValue;
    }

    public class SessionManagerService : MonoBehaviour, IService
    {
        public const int MAX_PLAYERS = 4;

        public bool HasMultiplayerSessionBegan { get; set; } = false;

        public Seat[] Seats { get; private set; }

        public int OccupiedSeatCount => Seats.Count(s => s.IsTaken);
        public int EmptySeatCount => Seats.Count(s => !s.IsTaken);

        private void Awake()
        {
            ServiceLocator.Register<SessionManagerService>(this);
            DontDestroyOnLoad(this);

            Seats = new Seat[MAX_PLAYERS];
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                Seats[i] = new Seat(ulong.MaxValue);
            }
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        public void SetSessionState(bool sessionState)
        {
            HasMultiplayerSessionBegan = sessionState;
        }

        public Seat GetLocalSeat()
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (Seats[i].ClientID == clientId) return Seats[i];
            }

            throw new Exception("Local client is not in seats");
        }

        public void OnPlayerJoined(ulong clientId)
        {
            if (!HasMultiplayerSessionBegan) throw new Exception("Session has not begun yet!");

            if (EmptySeatCount == 0)
            {
                throw new Exception("Session is full!");
            }

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (!Seats[i].IsTaken)
                {
                    Seats[i] = new Seat(clientId);
                    break;
                }
            }
        }

        public void OnPlayerLeft(ulong clientId)
        {
            if (!HasMultiplayerSessionBegan) throw new Exception("Session has not begun yet!");

            if (OccupiedSeatCount == 0)
            {
                throw new Exception("Session has no players!");
            }

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (Seats[i].ClientID == clientId)
                {
                    Seats[i] = new Seat(ulong.MaxValue);
                    return;
                }
            }

            throw new Exception($"Player with client ID {clientId} could not be found!");
        }

        private void OnClientConnected(ulong clientId)
        {
            OnPlayerJoined(clientId);
            Debug.Log($"Player with cid {clientId} joined");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            OnPlayerLeft(clientId);
            Debug.Log($"Player with cid {clientId} disconnected");
        }
    }
}